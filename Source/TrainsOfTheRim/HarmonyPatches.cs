using HarmonyLib;
using Verse;

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
