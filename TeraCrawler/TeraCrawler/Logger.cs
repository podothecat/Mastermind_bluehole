using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TeraCrawler
{
    public static class Logger
    {
        private enum LogType
        {
            Information = 100,
            Exception = 200,
        }

        private static object _logLock = new object();

        public static void Log(string format, params object[] args)
        {
            var message = string.Format(format, args);
            Log(LogType.Information, message);
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
            var timeStamp = DateTime.Now.ToString("HH:mm:ss");
            var formattedMessage = string.Format("[{0}] {1}", timeStamp, message);
            var logFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "LOG");
            var filePath = Path.Combine(logFolder, logType.ToString() + ".log");
            lock (_logLock)
            {
                if (!Directory.Exists(logFolder)) Directory.CreateDirectory(logFolder);
                Console.WriteLine(formattedMessage);
                using (var writer = new StreamWriter(filePath, true))
                    writer.WriteLine(formattedMessage);
            }
        }
    }
}
