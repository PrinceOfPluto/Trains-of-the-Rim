using UnityEngine;
using Verse;

namespace TrainsOfTheRim
{
    internal class TrainMod : Mod
    {
        TrainModSettings settings;
        public TrainMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<TrainModSettings>();
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
