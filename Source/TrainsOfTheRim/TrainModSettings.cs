using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TrainsOfTheRim
{
    internal class TrainModSettings : ModSettings
    {
        public bool useRails = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref useRails, "useRails");
            base.ExposeData();
        }
    }
}
