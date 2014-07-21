using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataContext;

namespace TeraCrawler.TargetCrawler
{
    public class InvenCrawler : Crawler
    {
        public override Article ParseArticlePage(string rawHtml)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Article> ParsePagingPage(string rawHtml)
        {
            throw new NotImplementedException();
        }

        protected override string MakePagingPageAddress(int pageNo)
        {
            throw new NotImplementedException();
        }

        protected override string MakeArticlePageAddress(int articleId)
        {
            throw new NotImplementedException();
        }
    }
}
