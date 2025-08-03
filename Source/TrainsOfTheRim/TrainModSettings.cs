using Verse;

namespace TrainsOfTheRim
{
    internal class TrainModSettings : ModSettings
    {
        public bool requireRailroadTerrain = false;
        public bool hideRoadBuildingFeature = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref requireRailroadTerrain, "requireRailroadTerrain");
            Scribe_Values.Look(ref hideRoadBuildingFeature, "hideRoadBuildingFeature");
            base.ExposeData();
        }
    }
}
