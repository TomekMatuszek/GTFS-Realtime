using NLog;
using NLog.Web;

namespace GTFS_Realtime
{
    public class NLogger : ILogger
    {
        public Logger _logger;

        public NLogger()
        {
            _logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
        }

        public void Log(string message)
        {
            _logger.Info(message);
        }

        public void LogError(string message)
        {
            _logger.Error(message);
        }
    }
}
