using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Trustesse.Ivoluntia.Commons.Extensions.Logging
{
    public static class LogHandler
    {
        public static void WriteLog(string logpath, string mssg, LogType logType, [CallerMemberName] string? cmn = null)
        {
            var path = Path.Combine(logpath, $"Log_{logType}\\{logType.ToString().ToUpper()}-{DateTime.Now:yy-MM-dd}.txt");

            var logMssg = $"\r\nMethod: {cmn}\r\nMessage: {mssg}\r\n_______________________________________";

            using (var log = new LoggerConfiguration()
                .WriteTo.File(path, shared: true, rollOnFileSizeLimit: true, fileSizeLimitBytes: 15 * 1024 * 1024)
                .CreateLogger())
            {
                switch (logType)
                {
                    case LogType n when n.Equals(LogType.Information):
                        log.Information(logMssg);
                        break;
                    case LogType n when n.Equals(LogType.Reversal):
                        log.Information(logMssg);
                        break;
                    case LogType n when n.Equals(LogType.Threads):
                        log.Information(logMssg);
                        break;
                    case LogType n when n.Equals(LogType.Warning):
                        log.Warning(logMssg);
                        break;
                    case LogType n when n.Equals(LogType.Exception):
                        log.Error(logMssg);
                        break;
                    case LogType n when n.Equals(LogType.SqlException):
                        log.Error(logMssg);
                        break;
                    case LogType n when n.Equals(LogType.BackgroundService):
                        log.Information(logMssg);
                        break;
                    default:
                        log.Warning(logMssg);
                        break;
                };
            }
        }

    }
    public enum LogType
    {
        Information,
        Reversal,
        Threads,
        Warning,
        Exception,
        SqlException,
        BackgroundService
    }
}


