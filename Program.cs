using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using System.Threading;

namespace GTFS_parser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GTFS-RT data for public transport vehicles in Poznan ------------------------");
            SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            var minutes = int.Parse(args[0]);
            var seconds = int.Parse(args[1]);

            var oldResults = new DataTable();
            for (int i = 0; i < minutes; i++)
            {
                for (int j = 0; j < (60 / seconds); j++)
                {
                    var t1 = DateTime.Now;
                    oldResults = Run(oldResults);
                    var t2 = DateTime.Now;
                    if (i < (minutes - 1) | j < ((60 / seconds) - 1))
                    {
                        Thread.Sleep((seconds * 1000) - (t2 - t1).Milliseconds);
                    }
                }
            }
            
            //Console.ReadLine();
        }

        static DataTable Run(DataTable oldResults)
        {
            var tasks = new Tasks(oldResults);
            var vehiclePositions = tasks.DownloadGTFS("vehicle_positions");
            Console.WriteLine(vehiclePositions.Entity[0].ToString());
            var tripUpdates = tasks.DownloadGTFS("trip_updates");
            Console.WriteLine(tripUpdates.Entity[0].ToString());

            var results = tasks.PrepareData(vehiclePositions, tripUpdates);
            Console.WriteLine(results.Rows.Count);

            tasks.UploadData(results);
            tasks.PrintData(results);

            return results;
        }
    }
}
