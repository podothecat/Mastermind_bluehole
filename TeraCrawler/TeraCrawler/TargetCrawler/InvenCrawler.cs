using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Xml.Linq;

namespace TeraCrawler.TargetCrawler
{
    public class InvenCrawler : Crawler
    {
        public InvenCrawler()
        {
            encoding = Encoding.GetEncoding(51949);
        }

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
                    Game = (int)Games.tera,
                    TargetSite = (int)TargetSites.inven,
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

        protected override IEnumerable<string> MakeCommentPageAddresses(Article article)
        {
            throw new NotImplementedException();
        }

        public override void ParseCommentPage(string commentPage, ref IList<Comment> comments)
        {
            throw new NotImplementedException();
        }

        protected override IList<Comment> CrawlComments(Article article)
        {
            IList<Comment> comments = new List<Comment>();

            var dummy = (long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;
            var client = (HttpWebRequest)WebRequest.Create(string.Format("http://www.inven.co.kr/common/board/comment.xml.php?dummy={0}", dummy));
            {
                client.ContentType = "application/x-www-form-urlencoded";
                client.Headers.Add("charset", "UTF-8");

                client.Method = "POST";
                using (var writer = new StreamWriter(client.GetRequestStream()))
                {
                    writer.Write(string.Format(@"comeidx={0}&articlecode={1}", article.CategoryId, article.ArticleId));
                }

                using (var reader = new StreamReader(client.GetResponse().GetResponseStream()))
                {
                    var commentXDoc = XDocument.Parse(reader.ReadToEnd());
                    foreach (var commentItem in commentXDoc.XPathSelectElements("//resultdata/commentlist/item"))
                    {
                        comments.Add(new Comment {
                            CommentId = int.Parse(commentItem.Attribute("cmtidx").Value),
                            ParentCommentId = int.Parse(commentItem.Attribute("cmtpidx").Value),
                            ArticleAutoId = article.ArticleAutoId,
                            Author = commentItem.XPathSelectElement("o_name").Value,
                            ContentHtml = commentItem.XPathSelectElement("o_comment").Value,
                            CommentWrittenTime = DateTime.Now, 
                            LikeCount = int.Parse(commentItem.XPathSelectElement("o_recommend").Value),
                            DislikeCount = int.Parse(commentItem.XPathSelectElement("o_notrecommend").Value),
                        });
                    }
                }
            }

            return comments;
        }
    }
}
