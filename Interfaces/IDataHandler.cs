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
        public void ClearDataTables();
        DataTable PrepareTable();
        public void FillTable(TransitRealtime.VehiclePosition obj);
        public void FillTable(TransitRealtime.VehiclePosition obj, DataRow prevRecord);
        public void FillTable(TransitRealtime.TripUpdate obj);
        public void FillTable(TransitRealtime.TripUpdate obj, DataRow prevRecord);
    }
}
