using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DataContext;
using HtmlAgilityPack;

namespace TeraCrawler.TargetCrawler
{
    public class InvenCrawler : Crawler
    {
        public override void ParseArticlePage(Article article)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(article.RawHtml);

            // 본문이 삭제된 경우
            if (htmlDoc.DocumentNode.ChildNodes.Count == 1) return;

            var articleWriterNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='articleWriter']/span");
            article.Author = articleWriterNode.Attributes["onclick"].Value.Replace("layerNickName('", "").Replace("','pbNickNameHandler')", "");

            var writtenTimeNode =
                htmlDoc.DocumentNode.SelectSingleNode("//div[@class='articleInfo']/div[@class='articleDate']");
            article.ArticleWrittenTime = DateTime.Parse(writtenTimeNode.InnerText);

            var articleTitleNode =
                htmlDoc.DocumentNode.SelectSingleNode("//div[@class='articleSubject ']/div[@class='articleTitle']/h1");
            article.Title = articleTitleNode.InnerText;

            var articleContentNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='powerbbsContent']");
            article.ContentHtml = articleContentNode.InnerHtml;
        }


        public override IEnumerable<Article> ParsePagingPage(string rawHtml)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(rawHtml);

            foreach (var articleNode in htmlDoc.DocumentNode.SelectNodes("//tr[@height=28 and @bgcolor='white']"))
            {
                var link = articleNode.SelectSingleNode("td[@class='bbsSubject']").SelectSingleNode("a").Attributes["href"].Value;
                const string token = "=&l=";
                var articleId = int.Parse(link.Substring(link.LastIndexOf(token) + token.Length));

                var article = new Article
                {
                    Game = Games.tera,
                    TargetSite = TargetSites.inven,
                    CategoryId = CategoryId,
                    ArticleId = articleId,
                    Link = link,
                    Author = articleNode.SelectSingleNode("td[@align='left']").InnerText.Trim(),
                };

                yield return article;
            }
        }

        protected override int PagingSize()
        {
            return 50;
        }

        protected override string MakePagingPageAddress(int pageNo)
        {
            return string.Format("http://www.inven.co.kr/board/powerbbs.php?come_idx={0}&sort=PID&&p={1}", CategoryId, pageNo);
        }

        protected override string MakeArticlePageAddress(int articleId)
        {
            return string.Format("http://www.inven.co.kr/board/powerbbs.php?come_idx={0}&l={1}", CategoryId, articleId);
        }
    }
}
