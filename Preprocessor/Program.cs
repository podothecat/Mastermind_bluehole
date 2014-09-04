using DataContext;
using HtmlAgilityPack;
using kr.ac.kaist.swrc.jhannanum.comm;
using kr.ac.kaist.swrc.jhannanum.hannanum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using TeraCrawler;

namespace Preprocessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var workflow = WorkflowFactory.getPredefinedWorkflow(WorkflowFactory.WORKFLOW_NOUN_EXTRACTOR);
            var lastWorkArticleId = 0;

            try
            {
                workflow.activateWorkflow(true);

                while (true)
                {
                    using (var context = new TeraDataContext())
                    {
                        // initializing data
                        if (!context.CheckPoints.Any(e => e.AnalysisPhase == AnalysisPhase.Preprocess))
                        {
                            context.CheckPoints.InsertOnSubmit(new CheckPoint
                            {
                                AnalysisPhase = AnalysisPhase.Preprocess,
                                ProcessedArticleId = 0,
                            });

                            context.SubmitChanges();
                        }

                        // get an article to work
                        var processedInfo = context.CheckPoints.Where(e => e.AnalysisPhase == AnalysisPhase.Preprocess).First();

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

                        // clean up text
                        var document = new HtmlDocument();
                        document.LoadHtml(article.ContentHtml);
                        var htmlCleanText = WebUtility.HtmlDecode(document.DocumentNode.InnerText).Trim();

                        // nlp preprocessing
                        workflow.analyze(htmlCleanText);

                        // result to xml document
                        var xDoc = new XDocument();
                        xDoc.Add(new XElement("Document"));
                        foreach (var sentence in workflow.getResultOfDocument(new Sentence(0, 0, false)))
                        {
                            xDoc.Root.Add(new XElement(
                                "Sentence",
                                string.Join(
                                    ",",
                                    sentence.Eojeols.Where(e => e.length > 0).Select(e => string.Join(",", e.Morphemes.Where(t => t.Length > 0))))));
                        }


                        // update to database
                        article.Keywords = xDoc.ToString();
                        processedInfo.ProcessedArticleId = article.ArticleAutoId;

                        context.SubmitChanges();
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Log("Exception occurred during preprocessing Article Auto ID: {0}", lastWorkArticleId);
                Logger.Log(ex);
            }
            finally
            {
                workflow.close();
            }
        }
    }
}
