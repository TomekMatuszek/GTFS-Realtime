using Microsoft.SqlServer.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFS_Realtime
{
    public class DataHandler : IDataHandler
    {
        public DataTable VehicleData { get; set; }
        public DataTable TripsData { get; set; }
        private ITimeService _timeService;
        private IGeometrySnapper _geometrySnapper;
        public static Dictionary<string, Type> Columns = new Dictionary<string, Type>()
        {
            {"fid",         typeof(int)},
            {"trip_id",     typeof(string)},
            {"line",        typeof(string)},
            {"brigade" ,    typeof(string)},
            {"status" ,     typeof(string)},
            {"stop_seq" ,   typeof(string)},
            {"position_x" , typeof(double)},
            {"position_y" , typeof(double)},
            {"distance" ,   typeof(double)},
            {"speed" ,      typeof(double)},
            {"time_prev" ,  typeof(DateTime)},
            {"time_req" ,   typeof(DateTime)},
            {"time_org" ,   typeof(DateTime)},
            {"time" ,       typeof(DateTime)},
            {"timestamp" ,  typeof(int)},
            {"delay" ,      typeof(int)},
            {"delay_change" , typeof(int)},
            {"geometry" ,   typeof(SqlGeography)}
        };

        public DataHandler(ITimeService timeService, IGeometrySnapper geometrySnapper)
        {
            _timeService = timeService;
            _geometrySnapper = geometrySnapper;
            VehicleData = PrepareTable();
            TripsData = PrepareTable();
        }

        public void ClearDataTables()
        {
            VehicleData = PrepareTable();
            TripsData = PrepareTable();
        }

        public DataTable PrepareTable()
        {
            var table = new DataTable();
            Columns.ToList().ForEach(c => table.Columns.Add(c.Key, c.Value));
            return table;
        }

        public void FillTable(TransitRealtime.VehiclePosition obj)
        {
            var row = VehicleData.NewRow();
            row["fid"] = 0;
            row["trip_id"] = obj.Trip.TripId;
            row["line"] = obj.Trip.RouteId;
            row["brigade"] = obj.Vehicle.Label;
            row["status"] = obj.CurrentStatus;
            row["stop_seq"] = obj.CurrentStopSequence;
            row["position_x"] = obj.Position.Longitude;
            row["position_y"] = obj.Position.Latitude;
            row["speed"] = obj.Position.Speed * 3.6;
            row = _timeService.ResolveDates(row, obj.Timestamp);
            row["timestamp"] = obj.Timestamp + 7200;
            row["geometry"] = _geometrySnapper.SnapGeometryToNetwork(obj.Position.Longitude, obj.Position.Latitude);
            row["distance"] = 0;
            VehicleData.Rows.Add(row);
        }

        public void FillTable(TransitRealtime.VehiclePosition obj, DataRow prevRecord)
        {
            var row = VehicleData.NewRow();
            row["fid"] = 0;
            row["trip_id"] = obj.Trip.TripId;
            row["line"] = obj.Trip.RouteId;
            row["brigade"] = obj.Vehicle.Label;
            row["status"] = obj.CurrentStatus;
            row["stop_seq"] = obj.CurrentStopSequence;
            row["position_x"] = obj.Position.Longitude;
            row["position_y"] = obj.Position.Latitude;
            row["speed"] = obj.Position.Speed;
            row = _timeService.ResolveDates(row, prevRecord, obj.Timestamp);
            row["timestamp"] = obj.Timestamp + 7200;
            row["geometry"] = _geometrySnapper.SnapGeometryToNetwork(obj.Position.Longitude, obj.Position.Latitude);
            row["distance"] = double.Parse(
                    ((SqlGeography)row["geometry"]).STDistance((SqlGeography)prevRecord["geometry"]).ToString()
                );
            VehicleData.Rows.Add(row);
        }

        public void FillTable(TransitRealtime.TripUpdate obj)
        {
            var row = TripsData.NewRow();
            row["trip_id"] = obj.Trip.TripId;
            row["delay"] = obj.StopTimeUpdate[0].Arrival.Delay;
            row["delay_change"] = 0;
            TripsData.Rows.Add(row);
        }

        public void FillTable(TransitRealtime.TripUpdate obj, DataRow prevRecord)
        {
            var row = TripsData.NewRow();
            row["trip_id"] = obj.Trip.TripId;
            row["delay"] = obj.StopTimeUpdate[0].Arrival.Delay;
            try
            {
                row["delay_change"] = obj.StopTimeUpdate[0].Arrival.Delay - (int)prevRecord["delay"];
            }
            catch (InvalidCastException)
            {
                row["delay_change"] = DBNull.Value;
            }
            TripsData.Rows.Add(row);
        }
    }
}
