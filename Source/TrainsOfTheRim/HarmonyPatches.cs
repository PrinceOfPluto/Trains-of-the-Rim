using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using TrainsOfTheRim.Patches;
using Vehicles;
using Vehicles.World;
using Verse;
using Verse.AI;

namespace TrainsOfTheRim
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("TrainsOfTheRim");
            Harmony.DEBUG = true;
            harmony.PatchAll();
        }
    }
}
