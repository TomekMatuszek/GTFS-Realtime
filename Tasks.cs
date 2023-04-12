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
using System.Data.SqlTypes;
using System.Runtime.InteropServices;
using TransitRealtime;

namespace GTFS_Realtime
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
            HttpRequest.KeepAlive = false;
            using (var response = (HttpWebResponse)HttpRequest.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                FeedMessage feed;
                try
                {
                    feed = TransitRealtime.FeedMessage.Parser.ParseFrom(responseStream);
                }
                catch (Google.Protobuf.InvalidProtocolBufferException ex)
                {
                    feed = null;
                    NLogger.Log.Error($"{ex.GetType()} [{type}] | {ex}");
                }
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
                Console.WriteLine($@"Line: {row["brigade"]} | Position: {row["geometry"]} | Speed: {row["speed"]} | {row["time"]} | Delay: {row["delay"]} ({row["delay_change"]})");
            }
        }

        public DataTable PrepareData(TransitRealtime.FeedMessage vehiclePositions, TransitRealtime.FeedMessage tripUpdates = null)
        {
            var handler = new DataHandler();
            var data1 = new DataTable();
            var data2 = new DataTable();
            DataTable dataMerged;
            data1 = PrepareVehicles(vehiclePositions, handler);

            if (tripUpdates != null)
            {
                data2 = PrepareTrips(tripUpdates, handler);

                dataMerged = handler.PrepareTable();
                var results = (from d1 in data1.AsEnumerable()
                               join d2 in data2.AsEnumerable() on d1.Field<string>("trip_id") equals d2.Field<string>("trip_id")
                               select dataMerged.LoadDataRow(new object[]
                               {
                                d1.Field<int>("fid"),
                                d1.Field<string>("trip_id"),
                                d1.Field<string>("line"),
                                d1.Field<string>("brigade"),
                                d1.Field<string>("status"),
                                d1.Field<string>("stop_seq"),
                                d1.Field<double>("position_x"),
                                d1.Field<double>("position_y"),
                                d1.Field<double>("distance"),
                                d1.Field<double>("speed"),
                                d1.Field<DateTime>("time_prev"),
                                d1.Field<DateTime>("time_req"),
                                d1.Field<DateTime>("time_org"),
                                d1.Field<DateTime>("time"),
                                d1.Field<int>("timestamp"),
                                d2.Field<int>("delay"),
                                d2.Field<int?>("delay_change"),
                                d1.Field<SqlGeography>("geometry")
                               }, false)).CopyToDataTable();
            }
            else
            {
                dataMerged = data1;
            }
 
            return dataMerged;
        }

        public void UploadData(DataTable data, string table)
        {
            using (var cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            {
                cnn.Open();
                using (var bulkcopy = new SqlBulkCopy(cnn))
                {
                    bulkcopy.DestinationTableName = table;
                    bulkcopy.WriteToServer(data);
                    bulkcopy.Close();
                }
                cnn.Close();
            }
        }

        private DataTable PrepareVehicles(TransitRealtime.FeedMessage vehiclePositions, DataHandler handler)
        {
            var data = new DataTable();
            for (int i = 0; i < vehiclePositions.Entity.Count; i++)
            {
                try
                {
                    data = handler.FillTable(vehiclePositions.Entity[i].Vehicle, OldData.Select($"trip_id = '{vehiclePositions.Entity[i].Vehicle.Trip.TripId}'")[0]);
                }
                catch (EvaluateException)
                {
                    data = handler.FillTable(vehiclePositions.Entity[i].Vehicle);
                }
                catch (IndexOutOfRangeException)
                {
                    data = handler.FillTable(vehiclePositions.Entity[i].Vehicle);
                }
            }
            return data;
        }

        private DataTable PrepareTrips(TransitRealtime.FeedMessage tripUpdates, DataHandler handler)
        {
            var data = new DataTable();
            for (int j = 0; j < tripUpdates.Entity.Count; j++)
            {
                try
                {
                    data = handler.FillTable(tripUpdates.Entity[j].TripUpdate, OldData.Select($"trip_id = '{tripUpdates.Entity[j].TripUpdate.Trip.TripId}'")[0]);
                }
                catch (EvaluateException)
                {
                    data = handler.FillTable(tripUpdates.Entity[j].TripUpdate);
                }
                catch (IndexOutOfRangeException)
                {
                    data = handler.FillTable(tripUpdates.Entity[j].TripUpdate);
                }
            }
            return data;
        }
    }
}
