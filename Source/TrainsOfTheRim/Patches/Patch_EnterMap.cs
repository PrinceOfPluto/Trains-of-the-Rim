using HarmonyLib;
using System.Collections.Generic;
using Vehicles.World;
using Verse;

namespace TrainsOfTheRim.Patches
{
    [HarmonyPatch(typeof(EnterMapUtilityVehicles))]
    internal class Patch_EnterMap
    {
        [HarmonyPostfix]
        [HarmonyPatch("SpawnVehicles")]
        public static void RecallCaravanToTrainPosition(VehicleCaravan caravan, List<Pawn> pawns, Map map,
    IntVec3 enterCell, Rot4 edge, bool draftColonists)
        {
            foreach (Pawn pawn in pawns)
            {
                TrainVehicleComp trainVehicleComp = pawn.TryGetComp<TrainVehicleComp>();
                if (trainVehicleComp != null && trainVehicleComp.HasCurrentTrain())
                {
                    trainVehicleComp.Vehicle.ignition.Drafted = true;
                    trainVehicleComp.RecallToCurrentMapSavedPosition();
                }
            }
        }
    }
}
