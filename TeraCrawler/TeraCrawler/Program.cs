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
            // TeraCrawler.exe [Game] [Target Site] [Category ID]
            if (args.Length != 3)
            {
                Logger.Log("Invalid execution parameters.");
                Logger.Log("TeraCrawler.exe [Game] [Target Site] [Category ID]");
                Logger.Log("[Game] must be one of ({0})", string.Join(", ", Enum.GetValues(typeof (Games)).OfType<Games>()));
                Logger.Log("[Target Site] must be one of ({0})", string.Join(", ", Enum.GetValues(typeof (TargetSites)).OfType<TargetSites>()));

                return;
            }

            var game = (Games)Enum.Parse(typeof (Games), args[0]);
            var targetSite = (TargetSites)Enum.Parse(typeof (TargetSites), args[1]);
            var categoryId = int.Parse(args[2]);

            var crawler = Crawler.Get(targetSite, categoryId);
            while(true)
            {
                crawler.CollectArticleList();
                crawler.CrawlArticles();

                Thread.Sleep(5000);
            }
        }
    }
}
