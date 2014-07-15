using DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeraCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new TeraDataContext())
            {
                var articleList = context.Articles.ToList();
                context.Articles.InsertOnSubmit(
                    new Article
                    {
                        CrawledTime = DateTime.Now,
                        Author = "",
                        Title = "",
                        RawHtml = "",
                        ContentHtml = "",
                        ArticleWrittenTime = DateTime.Now.ToString(),
                    });

                context.SubmitChanges();
            }
        }
    }
}
