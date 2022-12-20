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
    public class DataHandler
    {
        DataTable VehicleData = new DataTable();
        DataTable TripsData = new DataTable();

        public DataHandler()
        {
            VehicleData = PrepareTable();
            TripsData = PrepareTable();
        }

        public DataTable PrepareTable()
        {
            var table = new DataTable();
            table = AddColumn(table, "System.Int32", "fid");
            table = AddColumn(table, "System.String", "trip_id");
            table = AddColumn(table, "System.String", "line");
            table = AddColumn(table, "System.String", "brigade");
            table = AddColumn(table, "System.Double", "position_x");
            table = AddColumn(table, "System.Double", "position_y");
            table = AddColumn(table, "System.Double", "speed");
            table = AddColumn(table, "System.DateTime", "time_prev");
            table = AddColumn(table, "System.DateTime", "time_req");
            table = AddColumn(table, "System.DateTime", "time_org");
            table = AddColumn(table, "System.DateTime", "time");
            table = AddColumn(table, "System.Int32", "timestamp");
            table = AddColumn(table, "System.Int32", "delay");
            table = AddColumn(table, "SqlGeography", "geometry");
            return table;
        }

        private DataTable AddColumn(DataTable table, string type, string name)
        {
            DataColumn column;
            if (type == "SqlGeography")
            {
                column = new DataColumn(dataType: typeof(SqlGeography), columnName: name);
            }
            else
            {
                column = new DataColumn(dataType: Type.GetType(type), columnName: name);
            }
            table.Columns.Add(column);
            return table;
        }

        public DataTable FillTable(TransitRealtime.VehiclePosition obj)
        {
            var row = VehicleData.NewRow();
            row["fid"] = 0;
            row["trip_id"] = obj.Trip.TripId;
            row["line"] = obj.Trip.RouteId;
            row["brigade"] = obj.Vehicle.Label;
            row["position_x"] = obj.Position.Longitude;
            row["position_y"] = obj.Position.Latitude;
            row["speed"] = obj.Position.Speed;
            row["time_prev"] = DateTime.Now;
            row["time_req"] = DateTime.Now;
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime().AddSeconds(obj.Timestamp);
            row["time_org"] = date;
            date = RoundTime(date, TimeSpan.FromSeconds(15), date.AddSeconds(-15));
            row["time"] = date;
            row["timestamp"] = obj.Timestamp + 3600;
            row["delay"] = 0;
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
            row["position_x"] = obj.Position.Longitude;
            row["position_y"] = obj.Position.Latitude;
            row["speed"] = obj.Position.Speed;
            row["time_prev"] = prevRecord["time"];
            row["time_req"] = DateTime.Now;
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime().AddSeconds(obj.Timestamp);
            row["time_org"] = date;
            date = RoundTime(date, TimeSpan.FromSeconds(15), DateTime.Parse(prevRecord["time"].ToString()));
            row["time"] = date;
            row["timestamp"] = obj.Timestamp + 3600;
            row["delay"] = 0;
            var wkt = $"POINT({obj.Position.Longitude} {obj.Position.Latitude})";
            row["geometry"] = SqlGeography.STGeomFromText(new SqlChars(wkt.Replace(",", ".")), 4326);
            VehicleData.Rows.Add(row);
            return VehicleData;
        }

        public DataTable FillTable(TransitRealtime.TripUpdate obj)
        {
            var row = TripsData.NewRow();
            row["trip_id"] = obj.Trip.TripId;
            row["delay"] = obj.StopTimeUpdate[0].Arrival.Delay;
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
                rounded = rounded.AddSeconds(15);
            }
            else if (diff.Seconds > 15)
            {
                rounded = rounded.AddSeconds(-15);
            }

            return rounded;

            /*var delta = dt.Ticks % d.Ticks;
            bool roundUp = delta > d.Ticks / 2;
            var offset = roundUp ? d.Ticks : 0;
            return new DateTime(dt.Ticks + offset - delta, dt.Kind);*/
            
            //return dt.AddTicks(-(dt.Ticks % d.Ticks));
        }
    }
}
