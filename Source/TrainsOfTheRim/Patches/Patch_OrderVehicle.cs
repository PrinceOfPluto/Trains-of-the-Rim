using HarmonyLib;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;

namespace TrainsOfTheRim.Patches
{
    [HarmonyPatch(typeof(FloatMenuOptionProvider_OrderVehicle))]
    internal class Patch_OrderVehicle
    {
        [HarmonyPostfix]
        [HarmonyPatch("PawnGotoAction")]
        public static void PawnGotoActionTrain(IntVec3 clickCell, VehiclePawn vehicle, IntVec3 gotoLoc,
    Rot8 rot)
        {
            TrainVehicleComp trainVehicleComp = vehicle.TryGetComp<TrainVehicleComp>();
            if (trainVehicleComp == null || !trainVehicleComp.HasCurrentTrain() || !trainVehicleComp.LeadsCurrentTrain())
            {
                return;
            }
            trainVehicleComp.currentTrain.OrderConsistTo(trainVehicleComp, clickCell, gotoLoc, rot);
        }
    }
}
