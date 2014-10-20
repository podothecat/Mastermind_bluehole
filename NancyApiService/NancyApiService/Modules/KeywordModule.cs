using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using Nancy;
using Nancy.Json;
using NancyApiService.DataModel;
using NancyApiService.Helper;
using System.Xml.Linq;

namespace NancyApiService
{
    public class KeywordModule : NancyModule
    {
        private TeraArticlesDataContext _dataContext = new TeraArticlesDataContext();

        public KeywordModule() : base("/tera")
        {
            JsonSettings.MaxJsonLength = Int32.MaxValue;

            Get["/article"] = _ =>
            {
                return Response.AsJson(
                    _dataContext.Articles
                        .Select(e => new
                        {
                            e.ArticleAutoId,
                            e.ArticleId,
                            e.Author,
                            e.ContentHtml,
                            e.Keywords,
                        })
                        .First());
            };

            Get["/"] = _ =>
            {
                return View["Keywords.html"];
            };

            Get["/keywords/{beginDate}/{endDate}"] = _ =>
            {
                var beginDate = DateTime.Parse((string)_.beginDate);
                var endDate = DateTime.Parse((string)_.endDate);
                var keywordsList = _dataContext.Articles
                    .Where(e => e.ArticleWrittenTime >= beginDate)
                    .Where(e => e.ArticleWrittenTime <= endDate)
                    .Select(e => e.Keywords)
                    .ToList();

                var wordCount = new Dictionary<string, int>();
                foreach (var keywords in keywordsList.Where(e => !string.IsNullOrEmpty(e)))
                {
                    var xDoc = XDocument.Parse(keywords);
                    var keywordList = xDoc
                        .XPathSelectElements("//Document/Sentence")
                        .SelectMany(e => e.Value.Split(','))
                        .Where(e => e.Contains("/"))
                        .Select(e => new { Word = e.Split('/')[0], Tag = e.Split('/')[1], })
                        .Where(e => e.Word.Length > 1)
                        .Select(e => (e.Tag[0] == 'V' ? e.Word + "다" : e.Word))
                        .Distinct();

                    foreach (var keyword in keywordList)
                    {
                        if (!wordCount.ContainsKey(keyword)) wordCount.Add(keyword, 0);
                        wordCount[keyword] = wordCount[keyword] + 1;
                    }
                }

                return Response.AsJson(wordCount.OrderByDescending(e => e.Value).Take(50));
            };
        }
    }
}