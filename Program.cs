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
            var feed = tasks.DownloadGTFS("vehicle_positions");

            var results = tasks.PrepareData(feed);
            Console.WriteLine(results.Rows.Count);
            Console.WriteLine(feed.Entity[0].ToString());
            tasks.PrintData(results);

            Console.ReadLine();
        }
    }
}
