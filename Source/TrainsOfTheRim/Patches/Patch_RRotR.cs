using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles.World;
using Verse;

namespace TrainsOfTheRim.Patches
{
    [StaticConstructorOnStartup]
    internal static class Patch_RRotR
    {
        static Patch_RRotR()
        {
            try
            {
                ((Action)(() =>
                {
                    if (LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing == "Mlie.RailsAndRoadsOfTheRim"))
                    {
                        Harmony harmonyInstance = TrainMod.Harm;
                        var original = AccessTools.Method(typeof(RailsAndRoadsOfTheRim.WorldObjectComp_ConstructionSite), nameof(RailsAndRoadsOfTheRim.WorldObjectComp_ConstructionSite.FinishWork));
                        var postfix = AccessTools.Method(typeof(Patch_RRotR), nameof(PatchRROTRFinishWork));
                        if (harmonyInstance != null && original != null && postfix != null)
                        {
                            harmonyInstance.Patch(original, prefix: new HarmonyMethod(postfix));
                        }
                        else
                        {
                            Log.Error($"[TrainsOfTheRim] Error patching Rails And Roads Of The Rim");
                        }
                    }
                }))();
            }
            catch (TypeLoadException)
            {
            }
        }

        internal static void PatchRROTRFinishWork()
        {
            WorldComponent_RailroadBuilding.RegenWorldGridForRoads(WorldVehiclePathGrid.Instance);
        }
    }
}
