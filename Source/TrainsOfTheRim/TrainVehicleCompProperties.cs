using System;
using Verse;

namespace TrainsOfTheRim
{
    public class TrainVehicleCompProperties : CompProperties
    {
        public bool isLocomotive;
        public bool isRailroadVehicle;
        public bool hasRailAffordance = false;

        public TrainVehicleCompProperties()
        {
            this.compClass = typeof(TrainVehicleComp);
        }

        public TrainVehicleCompProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
}
