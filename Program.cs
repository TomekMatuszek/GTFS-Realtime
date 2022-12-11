using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace GTFS_parser
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GTFS-RT data for public transport vehicles in Poznan ------------------------");

            var tasks = new Tasks();
            var feed = tasks.DownloadGTFS("trip_updates");
            tasks.PrintGTFS(feed);

            Console.WriteLine(feed.Entity.Count);
            Console.WriteLine(feed.Entity[0].ToString());
            Console.WriteLine(feed.Entity[0].TripUpdate.Vehicle.Label);
            Console.WriteLine(feed.Entity[0].TripUpdate.StopTimeUpdate[0].Arrival.Delay);

            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0).ToLocalTime();
            date = date.AddSeconds(feed.Entity[0].TripUpdate.Timestamp);
            Console.WriteLine(date);

            Console.ReadLine();
        }
    }
}
