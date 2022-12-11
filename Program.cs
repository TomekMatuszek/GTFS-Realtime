using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace poznan_GTFS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GTFS-RT data for public transport vehicles in Poznan ------------------------");

            //var token = "token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJ0ZXN0Mi56dG0ucG96bmFuLnBsIiwiY29kZSI6MSwibG9naW4iOiJtaFRvcm8iLCJ0aW1lc3RhbXAiOjE1MTM5NDQ4MTJ9.ND6_VN06FZxRfgVylJghAoKp4zZv6_yZVBu_1-yahlo&";
            var HttpRequest = (HttpWebRequest)WebRequest.Create($"https://www.ztm.poznan.pl/pl/dla-deweloperow/getGtfsRtFile/?file=trip_updates.pb");
            TransitRealtime.FeedMessage feed;
            using (var response = (HttpWebResponse)HttpRequest.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                feed = TransitRealtime.FeedMessage.Parser.ParseFrom(responseStream);
                response.Close();
                responseStream.Close();
                HttpRequest.Abort();
            }

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
