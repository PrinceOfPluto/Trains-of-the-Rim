using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TrainsOfTheRim
{
    public class TrainVehiclePosition : IExposable
    {
        public IntVec3 position;
        public Rot4 rotation;

        public TrainVehiclePosition() { }

        public TrainVehiclePosition(IntVec3 position, Rot4 rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref position, "position");
            Scribe_Values.Look(ref rotation, "rotation");
        }
    }
}
