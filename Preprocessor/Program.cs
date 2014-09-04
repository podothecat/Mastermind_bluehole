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

namespace Preprocessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var workflow = WorkflowFactory.getPredefinedWorkflow(WorkflowFactory.WORKFLOW_NOUN_EXTRACTOR);
            
            try
            {
                workflow.activateWorkflow(true);
                var processedArticleId = 0;

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
                        //var processedArticleId = context.CheckPoints.Where(e => e.AnalysisPhase == AnalysisPhase.Preprocess).First().ProcessedArticleId;

                        // no article - Sleep thread
                        if (!context.Articles.Any(e => e.ArticleAutoId > processedArticleId))
                        {
                            Thread.Sleep(60 * 1000);
                            continue;
                        }

                        var article = context.Articles.Where(e => e.ArticleAutoId > processedArticleId).OrderBy(e => e.ArticleAutoId).First();

                        var document = new HtmlDocument();
                        document.LoadHtml(article.ContentHtml);

                        var htmlCleanText = WebUtility.HtmlDecode(document.DocumentNode.InnerText).Trim();

                        Console.WriteLine(htmlCleanText);
                        Console.WriteLine("=========== END OF DOCUMENT ============");

                        workflow.analyze(htmlCleanText);
                        foreach (var sentence in workflow.getResultOfDocument(new Sentence(0, 0, false)))
                        {
                            foreach (var eojeol in sentence.Eojeols)
                            {
                                foreach (var morpheme in eojeol.Morphemes)
                                {
                                    Console.Write(morpheme);
                                    Console.Write(" ");
                                }
                            }
                        }

                        Console.WriteLine();
                        Console.WriteLine("=========== END OF MORPHEME ============");

                        processedArticleId = article.ArticleAutoId;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }
            finally
            {
                workflow.close();
            }
        }
    }
}
