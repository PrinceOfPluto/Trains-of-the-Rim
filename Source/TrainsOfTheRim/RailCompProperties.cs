using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TrainsOfTheRim
{
    internal class RailCompProperties : CompProperties
    {
        public bool hasRailAffordance = true;

        public RailCompProperties() { this.compClass = typeof(RailComp); }

        public RailCompProperties(Type compClass) : base(compClass) { this.compClass = compClass; }
    }
}
