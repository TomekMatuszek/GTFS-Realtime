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
        Dictionary<string, string> Columns = new Dictionary<string, string>(){
            {"fid", "System.Int32"},
            {"trip_id", "System.String"},
            {"line", "System.String"},
            {"brigade" , "System.String"},
            {"status" , "System.String"},
            {"stop_seq" , "System.String"},
            {"position_x" , "System.Double"},
            {"position_y" , "System.Double"},
            {"distance" , "System.Double"},
            {"speed" , "System.Double"},
            {"time_prev" , "System.DateTime"},
            {"time_req" , "System.DateTime"},
            {"time_org" , "System.DateTime"},
            {"time" , "System.DateTime"},
            {"timestamp" , "System.Int32"},
            {"delay" , "System.Int32"},
            {"delay_change" , "System.Int32"},
            {"geometry" , "SqlGeography"}
        };
        
        public DataTable PrepareTable()
        {
            var table = new DataTable();
            foreach (var column in Columns)
            {
                table = AddColumn(table, column.Value, column.Key);
            }
            return table;
        }

        private DataTable AddColumn(DataTable table, string type, string name)
        {
            DataColumn column;
            if (type == "SqlGeography")
            {
                column = new DataColumn(dataType: typeof(SqlGeography), columnName: name);
            }
            else
            {
                column = new DataColumn(dataType: Type.GetType(type), columnName: name);
            }
            table.Columns.Add(column);
            return table;
        }
    }
}
