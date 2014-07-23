using DataContext;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace TeraCrawler
{
    public static class Logger
    {
        public static void Log(string format, params object[] args)
        {
            var message = string.Format(format, args);
            Log(LogType.Info, message);
        }

        public static void Log(Exception ex)
        {
            Log(LogType.Exception, ex.Message);
            Log(LogType.Exception, ex.StackTrace);
            Log(LogType.Exception, ex.Source);

            if (ex.InnerException != null) Log(ex.InnerException);
        }

        private static void Log(LogType logType, string message)
        {
            if (message == null) return;

            var timeStamp = DateTime.Now;
            var formattedMessage = string.Format("[{0}] {1}", timeStamp.ToString("HH:mm:ss"), message);
            Console.WriteLine(formattedMessage);
            using (var context = new TeraDataContext())
            {
                context.Logs.InsertOnSubmit(new Log
                {
                    TimeStamp = timeStamp,
                    LogType = logType,
                    Message = message,
                });
                context.SubmitChanges();
            }
        }
    }
}
