using Microsoft.SqlServer.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFS_Realtime
{
    public interface IGeometrySnapper
    {
        SqlGeography SnapGeometryToNetwork(float longitude, float latitude, int tolerance = 50);
    }
}
