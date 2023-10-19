using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFS_Realtime.Interfaces
{
    public interface IRealtimeTasks
    {
        public void AddPreviousResults(DataTable oldData);
        public TransitRealtime.FeedMessage? DownloadGTFS(string type);
        public void PrintData(DataTable data);
        public void PrepareData(out DataTable mergedResults, TransitRealtime.FeedMessage vehiclePositions, TransitRealtime.FeedMessage tripUpdates = null);
        public void UploadData(DataTable data, string table);
    }
}
