using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;

namespace GTFS_parser
{
    public class Tasks
    {
        DataTable Data = new DataTable();

        public TransitRealtime.FeedMessage DownloadGTFS(string type)
        {
            //var token = "token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJ0ZXN0Mi56dG0ucG96bmFuLnBsIiwiY29kZSI6MSwibG9naW4iOiJtaFRvcm8iLCJ0aW1lc3RhbXAiOjE1MTM5NDQ4MTJ9.ND6_VN06FZxRfgVylJghAoKp4zZv6_yZVBu_1-yahlo&";
            var HttpRequest = (HttpWebRequest)WebRequest.Create($"https://www.ztm.poznan.pl/pl/dla-deweloperow/getGtfsRtFile/?file={type}.pb");
            using (var response = (HttpWebResponse)HttpRequest.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                var feed = TransitRealtime.FeedMessage.Parser.ParseFrom(responseStream);
                response.Close();
                responseStream.Close();
                HttpRequest.Abort();
                return feed;
            }
        }

        public void PrintGTFS(TransitRealtime.FeedMessage feed)
        {
            for (int i = 0; i < feed.Entity.Count; i++)
            {
                Console.WriteLine($"{feed.Entity[i].TripUpdate.Vehicle.Label} | {feed.Entity[i].TripUpdate.StopTimeUpdate[0].Arrival.Delay}");
            }
        }

        public DataTable PrepareData(TransitRealtime.FeedMessage feed)
        {
            Data = PrepareTable();
            for (int i = 0; i < feed.Entity.Count; i++)
            {
                Data = FillTable(Data, feed.Entity[i]);
            }
            return Data;
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

        private DataTable FillTable(DataTable table, TransitRealtime.FeedEntity obj)
        {
            var row = table.NewRow();
            row["trip_id"] = obj.Vehicle.Trip.TripId;
            row["line"] = obj.Vehicle.Trip.RouteId;
            row["brigade"] = obj.Vehicle.Vehicle.Label;
            row["position_x"] = obj.Vehicle.Position.Longitude;
            row["position_y"] = obj.Vehicle.Position.Latitude;
            row["speed"] = obj.Vehicle.Position.Speed;
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime();
            row["time"] = date.AddSeconds(obj.Vehicle.Timestamp);
            table.Rows.Add(row);
            return table;
        }
    }
}
