using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFS_Realtime
{
    public interface ITimeService
    {
        public DataRow ResolveDates(DataRow row, DataRow prevRecord, ulong timestamp);
        public DataRow ResolveDates(DataRow row, ulong timestamp);
        public DateTime RoundTime(DateTime dt, TimeSpan d, DateTime prev);
        public DateTime RoundTime(DateTime dt, TimeSpan d);
    }
}
