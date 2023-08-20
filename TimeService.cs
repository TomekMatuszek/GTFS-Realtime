using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFS_Realtime
{
    public class TimeService : ITimeService
    {
        public DataRow ResolveDates(DataRow row, DataRow prevRecord, ulong timestamp)
        {
            row["time_prev"] = prevRecord["time"];
            row["time_req"] = DateTime.Now;
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime().AddSeconds(timestamp + 3600);
            row["time_org"] = date;
            row["time"] = RoundTime(date, TimeSpan.FromSeconds(Parameters.Seconds), DateTime.Parse(prevRecord["time"].ToString()));
            return row;
        }

        public DataRow ResolveDates(DataRow row, ulong timestamp)
        {
            row["time_prev"] = DateTime.Now;
            row["time_req"] = DateTime.Now;
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime().AddSeconds(timestamp + 3600);
            row["time_org"] = date;
            row["time"] = RoundTime(date, TimeSpan.FromSeconds(Parameters.Seconds));
            return row;
        }

        public DateTime RoundTime(DateTime dt, TimeSpan d, DateTime prev)
        {
            var delta = dt.Ticks % d.Ticks;
            var rounded = new DateTime(dt.Ticks - delta, dt.Kind);
            TimeSpan diff = rounded - prev;
            if (rounded == prev)
            {
                rounded = rounded.AddSeconds(Parameters.Seconds);
            }
            else if (diff.Seconds > Parameters.Seconds)
            {
                rounded = rounded.AddSeconds(-(Parameters.Seconds));
            }

            return rounded;
            //return dt.AddTicks(-(dt.Ticks % d.Ticks));
        }

        public DateTime RoundTime(DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            bool roundUp = delta > d.Ticks / 2;
            var offset = roundUp ? d.Ticks : 0;
            return new DateTime(dt.Ticks + offset - delta, dt.Kind);
        }
    }
}
