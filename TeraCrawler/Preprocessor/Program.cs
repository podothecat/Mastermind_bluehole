using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using TeraCrawler;

namespace Preprocessor
{
    class Program
    {
        static string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }

        static void Main(string[] args)
        {
            var lastWorkArticleId = 0;
            var contentFilePath = @"";
            while (true)
            {
                try
                {
                    using (var context = new TeraArticleDataContext())
                    {
                        #region initializing data
                        if (!context.CheckPoints.Any(e => e.AnalysisPhase == (int)AnalysisPhase.Preprocess))
                        {
                            context.CheckPoints.InsertOnSubmit(new CheckPoint
                            {
                                AnalysisPhase = (int)AnalysisPhase.Preprocess,
                                ProcessedArticleId = 0,
                            });
                            context.SubmitChanges();
                        }
                        #endregion

                        #region get an article to work
                        var processedInfo = context.CheckPoints.Where(e => e.AnalysisPhase == (int)AnalysisPhase.Preprocess).First();

                        // no article - Sleep thread
                        if (!context.Articles.Any(e => e.ArticleAutoId > processedInfo.ProcessedArticleId))
                        {
                            Logger.Log("All documents are preprocessed!");
                            Thread.Sleep(60 * 1000);
                            continue;
                        }

                        // fetch article
                        var article = context.Articles.Where(e => e.ArticleAutoId > processedInfo.ProcessedArticleId).OrderBy(e => e.ArticleAutoId).First();
                        lastWorkArticleId = article.ArticleAutoId;
                        #endregion

                        #region clean up text and make XML Document

                        var document = new HtmlDocument();
                        document.LoadHtml(article.ContentHtml.Replace("<br>", "\n"));
                        var htmlCleanText = WebUtility.HtmlDecode(document.DocumentNode.InnerText).Trim();
                        htmlCleanText = ReplaceHexadecimalSymbols(htmlCleanText);

                        contentFilePath = string.Format(@"C:\mecab\{0}.in", article.ArticleAutoId);
                        File.WriteAllText(contentFilePath, htmlCleanText);

                        var p = new Process();
                        p.StartInfo.RedirectStandardError = true;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.FileName = @"C:\mecab\mecab.exe";
                        p.StartInfo.Arguments = @"-r C:\mecab\dic\dicrc -d C:\mecab\dic " + contentFilePath;
                        p.StartInfo.RedirectStandardInput = true;
                        p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                        p.Start();

                        var xDoc = new XDocument();
                        xDoc.Add(new XElement("Document"));
                        xDoc.Root.Add(new XElement("HtmlCleanDocument", htmlCleanText));

                        var result = p.StandardOutput.ReadToEnd();
                        foreach (var sentence in result.Split(new List<string> { "EOS" }.ToArray(), StringSplitOptions.RemoveEmptyEntries))
                        {
                            var morphemeList = sentence
                                .Split('\n')
                                .Select(e => e.Split('\t', ','))
                                .Where(e => e.Length > 8)
                                .Select(e => new
                                {
                                    Token = e[0].Trim(),
                                    Tag = e[1].Trim(),
                                    CombinedResult = e[7].Trim(),
                                })
                                .Where(e => (e.Tag.Contains("NN") || e.Tag.Contains("VV") || e.Tag.Contains("VA")))
                                .ToList();

                            if (morphemeList.Count == 0) continue;

                            var preprocessedMorphemeList = new List<string>();
                            foreach (var morpheme in morphemeList)
                            {
                                if (morpheme.CombinedResult == "*")
                                {
                                    preprocessedMorphemeList.Add(morpheme.Token + "/" + morpheme.Tag);
                                }
                                else
                                {
                                    preprocessedMorphemeList.AddRange(morpheme.CombinedResult.Split('+').Where(e => (e.Contains("NN") || e.Contains("VV") || e.Contains("VA"))));
                                }
                            }
                            xDoc.Root.Add(new XElement("Sentence", string.Join(",", preprocessedMorphemeList)));
                        }
                        File.Delete(contentFilePath);

                        #endregion

                        #region Update to database

                        article.Keywords = xDoc.ToString();
                        processedInfo.ProcessedArticleId = article.ArticleAutoId;

                        context.SubmitChanges();

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    if (File.Exists(contentFilePath)) File.Delete(contentFilePath);
                }
                finally
                {
                }


            } // end - while(true)
        }
    }
}
