using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Data;
using System.Data.SqlClient;

namespace GTFS_parser
{
    public class Tasks
    {
        public TransitRealtime.FeedMessage DownloadGTFS(string type)
        {
            //var token = "token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJ0ZXN0Mi56dG0ucG96bmFuLnBsIiwiY29kZSI6MSwibG9naW4iOiJtaFRvcm8iLCJ0aW1lc3RhbXAiOjE1MTM5NDQ4MTJ9.ND6_VN06FZxRfgVylJghAoKp4zZv6_yZVBu_1-yahlo&";
            var HttpRequest = (HttpWebRequest)WebRequest.Create($"https://www.ztm.poznan.pl/pl/dla-deweloperow/getGtfsRtFile/?file={type}.pb");
            using (var response = (HttpWebResponse)HttpRequest.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                var feed = TransitRealtime.FeedMessage.Parser.ParseFrom(responseStream);
                response.Close();
                responseStream.Close();
                HttpRequest.Abort();
                Console.WriteLine(feed.Entity.Count);
                return feed;
            }
        }

        public void PrintGTFS(TransitRealtime.FeedMessage feed)
        {
            for (int i = 0; i < feed.Entity.Count; i++)
            {
                Console.WriteLine($"{feed.Entity[i].TripUpdate.Vehicle.Label} | {feed.Entity[i].TripUpdate.StopTimeUpdate[0].Arrival.Delay}");
            }
        }
    }
}
