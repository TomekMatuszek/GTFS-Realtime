using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFS_Realtime
{
    public interface IDataHandler
    {
        DataTable VehicleData { get; set; }
        DataTable TripsData { get; set; }
        DataTable PrepareTable();
        void FillTable(TransitRealtime.VehiclePosition obj);
        void FillTable(TransitRealtime.VehiclePosition obj, DataRow prevRecord);
        void FillTable(TransitRealtime.TripUpdate obj);
        void FillTable(TransitRealtime.TripUpdate obj, DataRow prevRecord);
    }
}
