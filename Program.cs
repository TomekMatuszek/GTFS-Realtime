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
            IKernel kernel = new StandardKernel(new GTFSRealtimeBindings());

            Parameters.Minutes = int.Parse(args[0]);
            Parameters.Seconds = int.Parse(args[1]);

            var realtimeService = kernel.Get<GTFSRealtimeService>();
            realtimeService.Run();
        }
    }
}
