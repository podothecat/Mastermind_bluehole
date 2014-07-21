using System.Collections.Concurrent;
using DataContext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TeraCrawler.TargetCrawler;

namespace TeraCrawler.TargetCrawler
{
    public abstract class Crawler
    {
        internal int CategoryId { get; set; }
        internal DateTime BeginDate { get; set; }
        internal DateTime EndDate { get; set; }

        internal Queue<int> ArticleQueueToCrawl = new Queue<int>();

        #region Generate
        public static Crawler Get(TargetSites target, int categoryId, DateTime beginDate, DateTime endDate)
        {
            Crawler crawler = null;

            switch (target)
            {
                case TargetSites.gamemeca:
                    //crawler = new GameMecaCrawler();
                    break;
                case TargetSites.hangame:
                    //crawler = new HangameCrawler();
                    break;
                case TargetSites.inven:
                    crawler = new InvenCrawler();
                    break;
                case TargetSites.naver:
                    //crawler = new NaverCrawler();
                    break;
                case TargetSites.thisisgame:
                    //crawler = new ThisIsGameCrawler();
                    break;
                default:
                    throw new Exception("Invalid TargetSite parameter");
            }

            crawler.BeginDate = beginDate;
            crawler.EndDate = endDate;
            crawler.CategoryId = categoryId;

            return crawler;
        }
        #endregion

        public void CollectArticleList()
        {
            var address = MakePagingPageAddress(1);
            var pagingArticleList = ParsePagingPage(address.CrawlIt(Encoding.UTF8)).ToList();
        }

        public bool IsWorking()
        {
            // 대상 일자가 현재 시간보다 뒤라면 계속 수집
            if (DateTime.Now < EndDate)
                return true;

            if (ArticleQueueToCrawl.Count > 0)
                return true;

            return false;
        }

        public void CrawlArticles()
        {
            while (ArticleQueueToCrawl.Count > 0)
            {
                var articleId = ArticleQueueToCrawl.Dequeue();
                var address = MakeArticlePageAddress(articleId);
                var article = ParseArticlePage(address.CrawlIt(Encoding.UTF8));

                using (var context = new TeraDataContext())
                {
                    context.Articles.InsertOnSubmit(article);
                    context.SubmitChanges();
                }
            }
        }

        public abstract Article ParseArticlePage(string rawHtml);
        
        public abstract IEnumerable<Article> ParsePagingPage(string rawHtml);
        
        protected abstract string MakePagingPageAddress(int pageNo);

        protected abstract string MakeArticlePageAddress(int articleId);
    }

    public static class CrawlHelper
    {
        public static string CrawlIt(this string url, Encoding encoding, int timeout = 3000)
        {
            var tryCount = 0;
            while (true)
            {
                try
                {
                    var webRequest = (HttpWebRequest)WebRequest.Create(url);
                    webRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.0)";
                    webRequest.CookieContainer = new CookieContainer();
                    webRequest.AllowAutoRedirect = true;
                    webRequest.Timeout = timeout;

                    using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
                    using (var reader = new StreamReader(webResponse.GetResponseStream(), encoding))
                    {
                        var rawHtml = reader.ReadToEnd();
                        var statusCode = webResponse.StatusCode;

                        return rawHtml;
                    }
                }
                catch (WebException ex)
                {
                    Logger.Log(new Exception(url));
                    Logger.Log(ex);
                    tryCount++;

                    // 3번 시도한 후 안될 경우 더 이상 시도하지 않음
                    if (tryCount == 3)
                        throw;
                }
            }
        }
    }
}
