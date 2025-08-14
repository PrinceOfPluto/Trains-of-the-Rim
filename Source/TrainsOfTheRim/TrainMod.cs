using HarmonyLib;
using SmashTools;
using UnityEngine;
using Verse;

namespace TrainsOfTheRim
{
    internal class TrainMod : Mod
    {
        public static Harmony Harm;
        TrainModSettings settings;
        public TrainMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<TrainModSettings>();
            Harm = new Harmony("TrainsOfTheRim");
            Harm.PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.CheckboxLabeled("TOTR.RequireRailTerrainLabel".Translate(), ref settings.requireRailroadTerrain, "TOTR.RequireRailTerrainDesc".Translate());
            listingStandard.CheckboxLabeled("TOTR.HideRoadBuildingFeatureLabel".Translate(), ref settings.hideRoadBuildingFeature, "TOTR.HideRoadBuildingFeatureDesc".Translate());
            listingStandard.SliderLabeled("TOTR.RoadBuildingSpeedMultiplierLabel".Translate(), "TOTR.RoadBuildingSpeedMultiplierDesc".Translate(), endSymbol: "x", ref settings.roadBuildingSpeedMultiplier, 1f, 10f, decimalPlaces: 1);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "TOTR.SettingsCategoryName".Translate();
        }
    }
}
