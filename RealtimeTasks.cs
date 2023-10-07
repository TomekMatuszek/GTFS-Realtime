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
using GTFS_Realtime.Interfaces;

namespace GTFS_Realtime
{
    public class RealtimeTasks : IRealtimeTasks
    {
        public DataTable OldData;
        private IDataHandler _dataHandler;
        private ILogger _logger;

        public RealtimeTasks(IDataHandler handler, ILogger logger)
        {
            _dataHandler = handler;
            _logger = logger;
        }

        public void AddPreviousResults(DataTable oldData)
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
                    _logger.LogError($"{ex.GetType()} [{type}] | {ex}");
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

        public void PrepareData(out DataTable mergedResults, TransitRealtime.FeedMessage vehiclePositions, TransitRealtime.FeedMessage tripUpdates = null)
        {
            DataTable dataMerged;
            PrepareVehicles(vehiclePositions, out DataTable data1);

            if (tripUpdates != null)
            {
                PrepareTrips(tripUpdates, out DataTable data2);

                dataMerged = _dataHandler.PrepareTable();
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
            mergedResults = dataMerged;
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

        private void PrepareVehicles(TransitRealtime.FeedMessage vehiclePositions, out DataTable vehicleData)
        {
            for (int i = 0; i < vehiclePositions.Entity.Count; i++)
            {
                try
                {
                    _dataHandler.FillTable(vehiclePositions.Entity[i].Vehicle, OldData.Select($"trip_id = '{vehiclePositions.Entity[i].Vehicle.Trip.TripId}'")[0]);
                }
                catch (EvaluateException)
                {
                    _dataHandler.FillTable(vehiclePositions.Entity[i].Vehicle);
                }
                catch (IndexOutOfRangeException)
                {
                    _dataHandler.FillTable(vehiclePositions.Entity[i].Vehicle);
                }
            }
            vehicleData = _dataHandler.VehicleData;
        }

        private void PrepareTrips(TransitRealtime.FeedMessage tripUpdates, out DataTable tripsData)
        {
            for (int j = 0; j < tripUpdates.Entity.Count; j++)
            {
                try
                {
                    _dataHandler.FillTable(tripUpdates.Entity[j].TripUpdate, OldData.Select($"trip_id = '{tripUpdates.Entity[j].TripUpdate.Trip.TripId}'")[0]);
                }
                catch (EvaluateException)
                {
                    _dataHandler.FillTable(tripUpdates.Entity[j].TripUpdate);
                }
                catch (IndexOutOfRangeException)
                {
                    _dataHandler.FillTable(tripUpdates.Entity[j].TripUpdate);
                }
            }
            tripsData = _dataHandler.TripsData;
        }
    }
}
