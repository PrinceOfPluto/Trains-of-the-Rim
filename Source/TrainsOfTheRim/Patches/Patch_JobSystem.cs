using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;
using Verse.AI;

namespace TrainsOfTheRim.Patches
{
    [HarmonyPatch(typeof(Pawn_JobTracker))]
    internal class Patch_JobSystem
    {
        //[HarmonyPostfix]
        //[HarmonyPatch(nameof(Pawn_JobTracker.TryTakeOrderedJob))]
        public static void TrainFollowsLocomotive(ref bool __result, Job job, JobTag? tag, bool requestQueueing, Pawn_JobTracker __instance)
        {
            if (job.def != JobDefOf.Goto)
            {
                return;
            }
            Pawn vehicle = __instance.curDriver.pawn;
            if (!(vehicle is VehiclePawn))
            {
                return;
            }
            TrainVehicleComp trainVehicleComp = vehicle.TryGetComp<TrainVehicleComp>();
            if (trainVehicleComp != null && trainVehicleComp.HasCurrentTrain() && trainVehicleComp.Props.isLocomotive)
            {
                trainVehicleComp.currentTrain.OrderFollowVehicleAhead();
            }
        }
    }
}
