using Verse;

namespace TrainsOfTheRim
{
    internal class TrainModSettings : ModSettings
    {
        public bool requireRailroadTerrain = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref requireRailroadTerrain, "requireRailroadTerrain");
            base.ExposeData();
        }
    }
}
