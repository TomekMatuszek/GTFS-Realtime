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
        DataTable Data1 = new DataTable();
        DataTable Data2 = new DataTable();

        public DataHandler()
        {
            Data1 = PrepareTable();
            Data2 = PrepareTable();
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
            var row = Data1.NewRow();
            row["fid"] = 0;
            row["trip_id"] = obj.Trip.TripId;
            row["line"] = obj.Trip.RouteId;
            row["brigade"] = obj.Vehicle.Label;
            row["position_x"] = obj.Position.Longitude;
            row["position_y"] = obj.Position.Latitude;
            row["speed"] = obj.Position.Speed;
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime().AddSeconds(obj.Timestamp);
            date = RoundUp(date, TimeSpan.FromMinutes(1));
            row["time"] = date;
            row["timestamp"] = obj.Timestamp;
            //row["delay"] = 0;
            var wkt = $"POINT({obj.Position.Longitude} {obj.Position.Latitude})";
            row["geometry"] = SqlGeography.STGeomFromText(new SqlChars(wkt.Replace(",", ".")), 4326);
            Data1.Rows.Add(row);
            return Data1;
        }

        public DataTable FillTable(TransitRealtime.TripUpdate obj)
        {
            var row = Data2.NewRow();
            row["trip_id"] = obj.Trip.TripId;
            row["delay"] = obj.StopTimeUpdate[0].Arrival.Delay;
            Data2.Rows.Add(row);
            return Data2;
        }

        private DateTime RoundUp(DateTime dt, TimeSpan d)
        {
            return dt.AddTicks(-(dt.Ticks % d.Ticks));
            //return new DateTime((dt.Ticks + d.Ticks - 1) / d.Ticks * d.Ticks, dt.Kind);
        }
    }
}
