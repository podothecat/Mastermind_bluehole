using System.Threading;
using DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeraCrawler.TargetCrawler;

namespace TeraCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            // TeraCrawler.exe [Target Site] [Begin Date] [End Date]
            if (args.Length != 3)
            {
                Logger.Log("Invalid execution parameters.");
                Logger.Log("TeraCrawler.exe [Target Site] [Category ID] [Begin Date] [End Date]");
                var targetSiteList = Enum.GetValues(typeof (TargetSites)).OfType<TargetSites>().ToList();
                Logger.Log("[Target Site] must be one of ({0})/", string.Join(",", targetSiteList));
                Logger.Log("[Begin Date] includes begin date - closed interval");
                Logger.Log("[End Date] excludes end date - opened interval");

                return;
            }

            var targetSite = (TargetSites)Enum.Parse(typeof (TargetSites), args[0]);
            var categoryId = int.Parse(args[1]);
            var beginDate = DateTime.Parse(args[2]);
            var endDate = DateTime.Parse(args[3]);

            var crawler = Crawler.Get(targetSite, categoryId, beginDate, endDate);
            crawler.CollectArticleList();
            while (crawler.IsWorking())
            {
                crawler.CrawlArticles();
                crawler.CollectArticleList();

                Thread.Sleep(5000);
            }
        }
    }
}
