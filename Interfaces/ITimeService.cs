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
        DataRow ResolveDates(DataRow row, DataRow prevRecord, ulong timestamp);
        DataRow ResolveDates(DataRow row, ulong timestamp);
        DateTime RoundTime(DateTime dt, TimeSpan d, DateTime prev);
        DateTime RoundTime(DateTime dt, TimeSpan d);
    }
}
