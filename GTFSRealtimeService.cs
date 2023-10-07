using GTFS_Realtime.Interfaces;
using Ninject;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTFS_Realtime
{
    public class GTFSRealtimeService
    {
        private IRealtimeTasks _tasks;
        private ILogger _logger;

        public GTFSRealtimeService(IRealtimeTasks tasks, ILogger logger)
        {
            _tasks = tasks;
            _logger = logger;
        }

        public void Run()
        {
            _logger.Log($"START for {Parameters.Minutes} minutes, every {Parameters.Seconds} seconds");

            var oldResults = new DataTable();
            for (int i = 0; i < Parameters.Minutes; i++)
            {
                for (int j = 0; j < (60 / Parameters.Seconds); j++)
                {
                    var t1 = DateTime.Now;
                    oldResults = RunIteration(oldResults);
                    Console.WriteLine($"Phase || {i:00}:{(j * Parameters.Seconds):00}");
                    var t2 = DateTime.Now;
                    if (i < (Parameters.Minutes - 1) | j < ((60 / Parameters.Seconds) - 1))
                    {
                        Thread.Sleep((Parameters.Seconds * 1000) - (t2 - t1).Milliseconds);
                    }
                }
            }
            _logger.Log("DONE");
        }

        public DataTable RunIteration(DataTable oldResults)
        {
            _tasks.AddPreviousResults(oldResults);
            var vehiclePositions = _tasks.DownloadGTFS("vehicle_positions");
            var tripUpdates = _tasks.DownloadGTFS("trip_updates");

            DataTable results;
            if (vehiclePositions != null)
            {
                try
                {
                    _tasks.PrepareData(out results, vehiclePositions, tripUpdates);
                    Console.WriteLine($"Vehicles: {results.Rows.Count}");

                    _tasks.PrintData(results);
                    _tasks.UploadData(results, "realtime");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Empty DataTable");
                    results = oldResults;
                    _logger.LogError($"{ex.GetType()} | {ex}");
                }

                return results;
            }
            else
            {
                _logger.Log($"Empty vehicles feed");
                return oldResults;
            }
        }
    }
}
