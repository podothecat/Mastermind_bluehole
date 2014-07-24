using DataContext;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace TeraCrawler.TargetCrawler
{

    public class NaverCrawler : Crawler
    {
        // 네이버에 로그인하기 위한 임시 아이디를 만들었습니다
        // id: podothecat@naver.com
        // password: Bluehole!

        private const string id = "podothecat";
        private const string password = "Bluehole!";

        private const string cafeUrl = "http://m.cafe.naver.com/sd92";
        private const string loginReqUrl = "https://nid.naver.com/nidlogin.login";
        private const string loginFormUrl = loginReqUrl + "?&url=http://m.cafe.naver.com/sd92";
        private const string categoryUrl = "http://m.cafe.naver.com/ArticleList.nhn?search.boardtype=L&search.menuid={0}&search.questionTab=A&search.clubid=13518432&search.totalCount=201&search.page={1}";
        private const string articlUrl = "http://m.cafe.naver.com/ArticleRead.nhn?clubid=13518432&articleid={1}&page=1&boardtype=L&menuid={0}";
        // 각종 상수들

        // 카테고리Id ( menuid라는 파라미터로 전송됨 )
        // 자유게시판 : 104
        // 벨릭섭게 : 409
        // 아룬섭게 : 399
        // 카이아섭게 : 426

        // 게시글id는 articleid라는 파라미터로 전송됨

        private ChromeDriver driver = null;

        public NaverCrawler()
        {
            var option = new ChromeOptions();
            option.AddExtension("3.7_0.crx");
            driver = new ChromeDriver(option);

            while ( driver.WindowHandles.Count < 2 )
            {
                Thread.Sleep(100);
            }

            driver.SwitchTo().Window(driver.WindowHandles[1]);
            
            WebDriverWait _wait = new WebDriverWait(driver, new TimeSpan(0, 1, 0));

            driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 10));

            _wait.Until(d => d.FindElement(By.Id("user_email")));
            driver.FindElementById("user_email").SendKeys(String.Format("{0}@naver.com\n", id));
            _wait.Until(d => d.FindElement(By.Id("user_password")));
            driver.FindElementById("user_password").SendKeys(password);
            driver.FindElementById("loginbtn").Click();

            driver.SwitchTo().Window(driver.WindowHandles[0]);
            driver.Url = loginFormUrl;
            driver.FindElementByCssSelector("#id").SendKeys(id);
            driver.FindElementByCssSelector("#pw").SendKeys(password);

            driver.FindElementByCssSelector("input.int_jogin").Click();
        }

        public override void ParseArticlePage(Article article)
        {
            throw NotImplementedException();
            /*
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
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
             */
        }


        public override IEnumerable<Article> ParsePagingPage(string rawHtml)
        {
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(rawHtml);

            foreach (var articleNode in htmlDoc.DocumentNode.SelectNodes("//*[@id=\"article_lst_section\"]/ul[2]/li[1]/a"))
            {
                var link = String.Format("http://m.cafe.naver.com/{0}",articleNode.Attributes["href"].Value);

                var match = new Regex("articleid=(?<articleid>[0-9]+)").Match(link);

                var articleId = int.Parse(match.Groups["articleid"].Value);

                var article = new Article
                {
                    Game = Games.tera,
                    TargetSite = TargetSites.naver,
                    CategoryId = CategoryId,
                    ArticleId = articleId,
                    Link = link,
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
            return string.Format(categoryUrl, CategoryId, pageNo);
        }

        protected override string MakeArticlePageAddress(int articleId)
        {
            return string.Format(articlUrl, CategoryId, articleId);
        }
    }
}
