using NLog;
using NLog.Web;

namespace GTFS_Realtime
{
    public static class NLogger
    {
        public static Logger Log = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
    }
}
