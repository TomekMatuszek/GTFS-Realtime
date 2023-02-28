using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFS_parser
{
    public class DataHandler : TableBuilder
    {
        DataTable VehicleData = new DataTable();
        DataTable TripsData = new DataTable();

        public DataHandler()
        {
            VehicleData = PrepareTable();
            TripsData = PrepareTable();
        }

        public DataTable FillTable(TransitRealtime.VehiclePosition obj)
        {
            var row = VehicleData.NewRow();
            row["fid"] = 0;
            row["trip_id"] = obj.Trip.TripId;
            row["line"] = obj.Trip.RouteId;
            row["brigade"] = obj.Vehicle.Label;
            row["status"] = obj.CurrentStatus;
            row["stop_seq"] = obj.CurrentStopSequence;
            row["position_x"] = obj.Position.Longitude;
            row["position_y"] = obj.Position.Latitude;
            row["distance"] = 0;
            row["speed"] = obj.Position.Speed;
            row["time_prev"] = DateTime.Now;
            row["time_req"] = DateTime.Now;
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime().AddSeconds(obj.Timestamp);
            row["time_org"] = date;
            row["time"] = RoundTime(date, TimeSpan.FromSeconds(Parameters.Seconds));
            row["timestamp"] = obj.Timestamp + 3600;
            var wkt = $"POINT({obj.Position.Longitude} {obj.Position.Latitude})";
            row["geometry"] = SqlGeography.STGeomFromText(new SqlChars(wkt.Replace(",", ".")), 4326);
            VehicleData.Rows.Add(row);
            return VehicleData;
        }

        public DataTable FillTable(TransitRealtime.VehiclePosition obj, DataRow prevRecord)
        {
            var row = VehicleData.NewRow();
            row["fid"] = 0;
            row["trip_id"] = obj.Trip.TripId;
            row["line"] = obj.Trip.RouteId;
            row["brigade"] = obj.Vehicle.Label;
            row["status"] = obj.CurrentStatus;
            row["stop_seq"] = obj.CurrentStopSequence;
            row["position_x"] = obj.Position.Longitude;
            row["position_y"] = obj.Position.Latitude;
            row["speed"] = obj.Position.Speed;
            row["time_prev"] = prevRecord["time"];
            row["time_req"] = DateTime.Now;
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime().AddSeconds(obj.Timestamp);
            row["time_org"] = date;
            row["time"] = RoundTime(date, TimeSpan.FromSeconds(Parameters.Seconds), DateTime.Parse(prevRecord["time"].ToString()));
            row["timestamp"] = obj.Timestamp + 3600;
            var wkt = $"POINT({obj.Position.Longitude} {obj.Position.Latitude})";
            row["geometry"] = SqlGeography.STGeomFromText(new SqlChars(wkt.Replace(",", ".")), 4326);
            row["distance"] = double.Parse(
                    ((SqlGeography)row["geometry"]).STDistance((SqlGeography)prevRecord["geometry"]).ToString()
                );
            VehicleData.Rows.Add(row);
            return VehicleData;
        }

        public DataTable FillTable(TransitRealtime.TripUpdate obj)
        {
            var row = TripsData.NewRow();
            row["trip_id"] = obj.Trip.TripId;
            row["delay"] = obj.StopTimeUpdate[0].Arrival.Delay;
            row["delay_change"] = 0;
            TripsData.Rows.Add(row);
            return TripsData;
        }

        public DataTable FillTable(TransitRealtime.TripUpdate obj, DataRow prevRecord)
        {
            var row = TripsData.NewRow();
            row["trip_id"] = obj.Trip.TripId;
            row["delay"] = obj.StopTimeUpdate[0].Arrival.Delay;
            row["delay_change"] = obj.StopTimeUpdate[0].Arrival.Delay - (int)prevRecord["delay"];
            TripsData.Rows.Add(row);
            return TripsData;
        }

        private DateTime RoundTime(DateTime dt, TimeSpan d, DateTime prev)
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

        private DateTime RoundTime(DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            bool roundUp = delta > d.Ticks / 2;
            var offset = roundUp ? d.Ticks : 0;
            return new DateTime(dt.Ticks + offset - delta, dt.Kind);
        }
    }
}
