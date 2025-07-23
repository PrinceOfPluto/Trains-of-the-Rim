using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TrainsOfTheRim.Gizmos;
using Vehicles;
using Verse;

namespace TrainsOfTheRim.Patches
{
    [HarmonyPatch(typeof(VehiclePawn))]
    public class Patch_Gizmos
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehiclePawn.GetGizmos))]
        public static void GetTrainGizmos(ref IEnumerable<Gizmo> __result, VehiclePawn __instance)
        {
            TrainVehicleComp trainVehicleComp = __instance.TryGetComp<TrainVehicleComp>();
            if (trainVehicleComp == null)
            {
                return;
            }

            List<Gizmo> gizmos = __result.ToList();

            if (trainVehicleComp.CanCreateNewTrain())
            {
                gizmos.Add(new Gizmo_CreateNewTrain(__instance));
            }
            if (trainVehicleComp.HasCurrentTrain())
            {
                gizmos.Add(new Gizmo_AddToTrain(__instance));
            }
            if (trainVehicleComp.CanJoinTrain())
            {
                gizmos.Add(new Gizmo_JoinTrain(__instance));
            }
            if (trainVehicleComp.CanRemoveFromTrain())
            {
                gizmos.Add(new Gizmo_RemoveFromTrain(__instance));
            }
            if (trainVehicleComp.CanRecallToPosition())
            {
                gizmos.Add(new Gizmo_RecallTrain(__instance));
            }
            if (trainVehicleComp.CanSavePosition())
            {
                gizmos.Add(new Gizmo_SaveNewPosition(__instance));
            }
            if (trainVehicleComp.CanDisbandTrain())
            {
                gizmos.Add(new Gizmo_DisbandTrain(__instance));
            }
            if (trainVehicleComp.CanCycleTexture())
            {
                gizmos.Add(new Gizmo_CycleTrainTexture(__instance));
            }
            __result = gizmos;
        }
    }
}
