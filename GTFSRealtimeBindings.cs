using GTFS_Realtime.Interfaces;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTFS_Realtime
{
    public class GTFSRealtimeBindings : NinjectModule
    {
        public override void Load()
        {
            Bind<GTFSRealtimeService>().ToSelf();
            Bind<IRealtimeTasks>().To<RealtimeTasks>();
            Bind<ILogger>().To<NLogger>();

            Bind<RealtimeTasks>().ToSelf();
            Bind<IDataHandler>().To<DataHandler>();

            Bind<DataHandler>().ToSelf();
            Bind<ITimeService>().To<TimeService>();
            Bind<IGeometrySnapper>().To<GeometrySnapper>();
        }
    }
}
