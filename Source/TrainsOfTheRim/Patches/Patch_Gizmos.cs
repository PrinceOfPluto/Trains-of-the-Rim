using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrainsOfTheRim.Gizmos;
using Vehicles;
using Verse;

namespace TrainsOfTheRim.Patches
{
    public class Patch_Gizmos
    {
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
            if (trainVehicleComp.CanAddToTrain())
            {
                gizmos.Add(new Gizmo_AddToTrain(__instance));
            }
            if (trainVehicleComp.HasCurrentTrain())
            {
                gizmos.Add(new Gizmo_RemoveFromTrain(__instance));
            }
            if(trainVehicleComp.CanRecallToPosition())
            {
                gizmos.Add(new Gizmo_RecallTrain(__instance));
            }
            __result = gizmos;
        }
    }
}
