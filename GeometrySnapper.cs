using Microsoft.SqlServer.Types;
using System;
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
    public class GeometrySnapper : IGeometrySnapper
    {
        public SqlGeography RouteNetwork;

        public GeometrySnapper()
        {
            var query = "select network from SIEC";
            var routeNetwork = new DataTable();
            using (var cnn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString))
            using (var cmd = new SqlCommand(query, cnn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cnn.Open();
                da.Fill(routeNetwork);
                cnn.Close();
            }
            RouteNetwork = (SqlGeography)routeNetwork.Rows[0]["network"];
        }

        public SqlGeography SnapGeometryToNetwork(float longitude, float latitude, int tolerance = 50)
        {
            var wkt = $"POINT({longitude} {latitude})";
            var wktGeom = SqlGeography.STGeomFromText(new SqlChars(wkt.Replace(",", ".")), 4326);
            var matchedPoint = wktGeom.ShortestLineTo(RouteNetwork);
            matchedPoint = matchedPoint.STLength() < tolerance ? matchedPoint.STEndPoint() : wktGeom;
            return matchedPoint;
        }
    }
}
