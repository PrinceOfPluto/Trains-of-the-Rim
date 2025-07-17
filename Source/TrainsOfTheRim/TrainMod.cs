using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            listingStandard.CheckboxLabeled("Experimental: Require rails", ref settings.useRails, "Controls whether train vehicles require rails to move on the colony map. This is different from the world map which requires trains to move along railroads.");
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "TrainsOfTheRim";
        }
    }
}
