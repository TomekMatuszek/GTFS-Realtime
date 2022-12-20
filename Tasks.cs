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
        DataTable OldData;

        public Tasks(DataTable oldData)
        {
            OldData = oldData;
        }
        
        public TransitRealtime.FeedMessage DownloadGTFS(string type)
        {
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

        public DataTable PrepareData(TransitRealtime.FeedMessage vehiclePositions, TransitRealtime.FeedMessage tripUpdates = null)
        {
            var handler = new DataHandler();
            var data1 = new DataTable();
            var data2 = new DataTable();
            for (int i = 0; i < vehiclePositions.Entity.Count; i++)
            {
                try
                {
                    data1 = handler.FillTable(vehiclePositions.Entity[i].Vehicle, OldData.Select($"trip_id = '{vehiclePositions.Entity[i].Vehicle.Trip.TripId}'")[0]);
                }
                catch (System.Data.EvaluateException)
                {
                    data1 = handler.FillTable(vehiclePositions.Entity[i].Vehicle);
                }
                catch (System.IndexOutOfRangeException)
                {
                    data1 = handler.FillTable(vehiclePositions.Entity[i].Vehicle);
                }
            }

            DataTable dataMerged;
            if (tripUpdates != null)
            {
                for (int j = 0; j < tripUpdates.Entity.Count; j++)
                {
                    data2 = handler.FillTable(tripUpdates.Entity[j].TripUpdate);
                }
                dataMerged = handler.PrepareTable();

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
                                d1.Field<DateTime>("time_prev"),
                                d1.Field<DateTime>("time_req"),
                                d1.Field<DateTime>("time_org"),
                                d1.Field<DateTime>("time"),
                                d1.Field<int>("timestamp"),
                                d2.Field<int>("delay"),
                                d1.Field<SqlGeography>("geometry")
                               }, false)).CopyToDataTable();
            }
            else
            {
                dataMerged = data1;
            }
 
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
