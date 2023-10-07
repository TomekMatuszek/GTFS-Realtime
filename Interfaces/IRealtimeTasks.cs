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
        void AddPreviousResults(DataTable oldData);
        TransitRealtime.FeedMessage DownloadGTFS(string type);
        void PrintData(DataTable data);
        void PrepareData(out DataTable mergedResults, TransitRealtime.FeedMessage vehiclePositions, TransitRealtime.FeedMessage tripUpdates = null);
        void UploadData(DataTable data, string table);
    }
}
