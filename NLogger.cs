using NLog;
using NLog.Web;

namespace GTFS_parser
{
    public static class NLogger
    {
        public static Logger Log = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
    }
}
