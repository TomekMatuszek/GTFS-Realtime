using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Types;
using System.Configuration;

namespace GTFS_parser
{
    public class Tasks
    {
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

        public void PrintData(DataTable data)
        {
            foreach (DataRow row in data.Rows)
            {
                Console.WriteLine($"Line: {row["brigade"]} | Position: {row["geometry"]} | Speed: {row["speed"]} | {row["time"]} | Delay: {row["delay"]}");
            }
        }

        public DataTable PrepareData(TransitRealtime.FeedMessage vehicle_positions, TransitRealtime.FeedMessage trip_updates)
        {
            var handler = new DataHandler();
            var data1 = new DataTable();
            var data2 = new DataTable();
            for (int i = 0; i < vehicle_positions.Entity.Count; i++)
            {
                data1 = handler.FillTable(vehicle_positions.Entity[i].Vehicle);
            }
            for (int j = 0; j < trip_updates.Entity.Count; j++)
            {
                data2 = handler.FillTable(trip_updates.Entity[j].TripUpdate);
            }
            var dataMerged = handler.PrepareTable();
            
            var results = (from d1 in data1.AsEnumerable()
                           join d2 in data2.AsEnumerable() on d1.Field<string>("trip_id") equals d2.Field<string>("trip_id")
                           select dataMerged.LoadDataRow(new object[]
                           {
                                d1.Field<int>("fid"),
                                d1.Field<string>("trip_id"),
                                d1.Field<string>("line"),
                                d1.Field<string>("brigade"),
                                d1.Field<double>("position_x"),
                                d1.Field<double>("position_y"),
                                d1.Field<double>("speed"),
                                d1.Field<DateTime>("time"),
                                d1.Field<int>("timestamp"),
                                d2.Field<int>("delay"),
                                d1.Field<SqlGeography>("geometry")
                           }, false)).CopyToDataTable();
            
            return dataMerged;
        }

        public void UploadData(DataTable data)
        {
            using (var cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                cnn.Open();
                using (var bulkcopy = new SqlBulkCopy(cnn))
                {
                    bulkcopy.DestinationTableName = "records";
                    bulkcopy.WriteToServer(data);
                    bulkcopy.Close();
                }
                cnn.Close();
            }
        }
    }
}
