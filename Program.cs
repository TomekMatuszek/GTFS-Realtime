using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;
using Ninject;

namespace GTFS_Realtime
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GTFS-Realtime data for public transport vehicles in Poznan ------------------------------------------------");
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            Parameters.Minutes = int.Parse(args[0]);
            Parameters.Seconds = int.Parse(args[1]);
            NLogger.Log.Info($"START for {Parameters.Minutes} minutes, every {Parameters.Seconds} seconds");

            var oldResults = new DataTable();
            for (int i = 0; i < Parameters.Minutes; i++)
            {
                for (int j = 0; j < (60 / Parameters.Seconds); j++)
                {
                    var t1 = DateTime.Now;
                    oldResults = Run(oldResults);
                    Console.WriteLine($"Phase || {i:00}:{(j * Parameters.Seconds):00}");
                    var t2 = DateTime.Now;
                    if (i < (Parameters.Minutes - 1) | j < ((60 / Parameters.Seconds) - 1))
                    {
                        Thread.Sleep((Parameters.Seconds * 1000) - (t2 - t1).Milliseconds);
                    }
                }
            }
            NLogger.Log.Info("DONE");
        }

        static DataTable Run(DataTable oldResults)
        {
            IKernel kernel = new StandardKernel(new GTFSRealtimeModule());
            var tasks = kernel.Get<Tasks>();

            tasks.AddPreviousResults(oldResults);
            var vehiclePositions = tasks.DownloadGTFS("vehicle_positions");
            var tripUpdates = tasks.DownloadGTFS("trip_updates");

            DataTable results;
            if (vehiclePositions != null)
            {
                try
                {
                    tasks.PrepareData(out results, vehiclePositions, tripUpdates);
                    Console.WriteLine($"Vehicles: {results.Rows.Count}");

                    tasks.PrintData(results);
                    tasks.UploadData(results, "realtime");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Empty DataTable");
                    results = oldResults;
                    NLogger.Log.Error($"{ex.GetType()} | {ex}");
                }

                return results;
            }
            else
            {
                NLogger.Log.Info($"Empty vehicles feed");
                return oldResults;
            }
        }
    }
}
