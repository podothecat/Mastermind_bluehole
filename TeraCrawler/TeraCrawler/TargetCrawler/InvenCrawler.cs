using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContext;
using HtmlAgilityPack;

namespace TeraCrawler.TargetCrawler
{
    public class InvenCrawler : Crawler
    {
        public override Article ParseArticlePage(string rawHtml, int articleId)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(rawHtml);

            var article = new Article
            {
                Game = Games.tera,
                TargetSite = TargetSites.inven,
                Link = MakeArticlePageAddress(articleId),
                CrawledTime = DateTime.Now,
                RawHtml = rawHtml,
            };

            // 본문이 삭제된 경우
            if (htmlDoc.DocumentNode.ChildNodes.Count == 1)
                return article;

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

            return article;
        }


        public override IEnumerable<Article> ParsePagingPage(string rawHtml, int pageNo)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(rawHtml);

            foreach (var articleListTableNode in htmlDoc.DocumentNode.SelectNodes("//table[@width='710']").Skip(3))
            {
                var textList = articleListTableNode.SelectNodes("//td[@align='center']").Select(e => e.InnerText.Trim()).Where(e => e.Length > 0);
                var lastArticleId = int.Parse(textList.ElementAt(1));

                yield return new Article
                {

                };
            }

            throw new NotImplementedException();
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
