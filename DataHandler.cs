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
        DataTable Data = new DataTable();

        public DataHandler()
        {
            Data = PrepareTable();
        }

        private DataTable PrepareTable()
        {
            var table = Data;
            table = AddColumn(table, "System.String", "trip_id");
            table = AddColumn(table, "System.String", "line");
            table = AddColumn(table, "System.String", "brigade");
            table = AddColumn(table, "System.Double", "position_x");
            table = AddColumn(table, "System.Double", "position_y");
            table = AddColumn(table, "System.Double", "speed");
            table = AddColumn(table, "System.DateTime", "time");
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

        public DataTable FillTable(TransitRealtime.FeedEntity obj)
        {
            var row = Data.NewRow();
            row["trip_id"] = obj.Vehicle.Trip.TripId;
            row["line"] = obj.Vehicle.Trip.RouteId;
            row["brigade"] = obj.Vehicle.Vehicle.Label;
            row["position_x"] = obj.Vehicle.Position.Longitude;
            row["position_y"] = obj.Vehicle.Position.Latitude;
            row["speed"] = obj.Vehicle.Position.Speed;
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime();
            row["time"] = date.AddSeconds(obj.Vehicle.Timestamp);
            row["delay"] = 0;
            var wkt = $"POINT({obj.Vehicle.Position.Longitude} {obj.Vehicle.Position.Latitude})";
            row["geometry"] = SqlGeography.STGeomFromText(new SqlChars(wkt.Replace(",", ".")), 4326);
            Data.Rows.Add(row);
            return Data;
        }
    }
}
