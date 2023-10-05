using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFS_Realtime
{
    public class GTFSRealtimeModule : NinjectModule
    {
        public override void Load()
        {
            Bind<Tasks>().ToSelf();
            Bind<IDataHandler>().To<DataHandler>();

            Bind<DataHandler>().ToSelf();
            Bind<ITimeService>().To<TimeService>();
            Bind<IGeometrySnapper>().To<GeometrySnapper>();
        }
    }
}
