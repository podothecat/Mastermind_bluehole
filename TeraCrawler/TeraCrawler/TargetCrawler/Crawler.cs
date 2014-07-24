using System.Collections.Concurrent;
using System.Threading;
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
        internal Games GameType { get; set; }
        internal TargetSites TargetSite { get; set; }
        internal int CategoryId { get; set; }

        internal int CurrentWorkingPage = 1;
        internal ConcurrentQueue<Article> ArticleQueueToCrawl = new ConcurrentQueue<Article>();

        #region Generate
        public static Crawler Get(TargetSites target, int categoryId)
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
                    crawler = new NaverCrawler();
                    break;
                case TargetSites.thisisgame:
                    //crawler = new ThisIsGameCrawler();
                    break;
                default:
                    throw new Exception("Invalid TargetSite parameter");
            }

            crawler.CategoryId = categoryId;

            return crawler;
        }
        #endregion

        public void CollectArticleList()
        {
            using (var context = new TeraDataContext())
            {
                var jumpPagingSize = 1;
                var address = MakePagingPageAddress(CurrentWorkingPage);
                foreach (var article in ParsePagingPage(address.CrawlIt(Encoding.UTF8)))
                {
                    try
                    {
                        // 문제가 있당... 큰일이당... 이를 우짜누...
                        // 우짜긴 배째 ㄱ=
                        if (article.ArticleId == 0) continue;

                        // 기존 데이터가 존재하는지 확인한 후 페이지 건너뛰기
                        if (context.Articles.Any(e => e.ArticleId == article.ArticleId))
                        {
                            var prevArticleCount = context.Articles.Count(e => e.ArticleId < article.ArticleId);
                            jumpPagingSize = prevArticleCount / PagingSize();

                            continue;
                        }
                        else
                        {
                            jumpPagingSize = 1;
                        }

                        ArticleQueueToCrawl.Enqueue(article);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Error occurred during CollectArticleList for ArticleID: {0} and Link: {1}", article.ArticleId, article.Link);
                        Logger.Log(ex);
                    }
                }

                CurrentWorkingPage += Math.Max(1, jumpPagingSize);
            }
        }

        public void CrawlArticles()
        {
            while (ArticleQueueToCrawl.Count > 0)
            {
                ThreadPool.QueueUserWorkItem(item =>
                {
                    Article article = null;
                    while (!ArticleQueueToCrawl.TryDequeue(out article)) {}
                    
                    try
                    {
                        var address = MakeArticlePageAddress(article.ArticleId);
                        article.RawHtml = address.CrawlIt(Encoding.GetEncoding(51949));
                        ParseArticlePage(article);
                        using (var context = new TeraDataContext())
                        {
                            context.Articles.InsertOnSubmit(article);
                            context.SubmitChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Error occurred ArticleID: {0}, Link: {1}", article.ArticleId, article.Link);
                        Logger.Log(ex);
                    }

                }, ArticleQueueToCrawl);
            }
        }

        protected abstract int PagingSize();
        protected abstract string MakePagingPageAddress(int pageNo);
        public abstract IEnumerable<Article> ParsePagingPage(string rawHtml);
        
        protected abstract string MakeArticlePageAddress(int articleId);
        public abstract void ParseArticlePage(Article article);
        
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
                    Logger.Log(new Exception(url) { Source = ex.Source });
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
