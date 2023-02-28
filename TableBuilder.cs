using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFS_parser
{
    public class TableBuilder
    {
        Dictionary<string, Type> Columns = new Dictionary<string, Type>()
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
        
        public DataTable PrepareTable()
        {
            var table = new DataTable();
            Columns.ToList().ForEach(c => table.Columns.Add(c.Key, c.Value));
            return table;
        }
    }
}
