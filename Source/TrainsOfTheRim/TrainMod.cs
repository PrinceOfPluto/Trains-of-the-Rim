using HarmonyLib;
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
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "TOTR.SettingsCategoryName".Translate();
        }
    }
}
