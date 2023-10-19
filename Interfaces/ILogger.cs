using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFS_Realtime
{
    public interface ILogger
    {
        public void Log(string message);
        public void LogError(string message);
    }
}
