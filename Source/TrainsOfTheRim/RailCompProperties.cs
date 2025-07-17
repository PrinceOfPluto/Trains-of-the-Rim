using System;
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
