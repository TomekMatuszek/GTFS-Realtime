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

            var oldResults = new DataTable();
            for (int i = 0; i < 4; i++)
            {
                oldResults = Run(oldResults);
                if (i < 3)
                {
                    Thread.Sleep(15000);
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
