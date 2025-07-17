using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles.World;
using Vehicles;
using Verse;
using HarmonyLib;

namespace TrainsOfTheRim.Patches
{
    [HarmonyPatch(typeof(CaravanFormation))]
    internal class Patch_CaravanFormation
    {
        [HarmonyPostfix]
        [HarmonyPatch("CheckForErrors")]
        public static void CheckForTrainCaravanErrors(FormationInfo ___formation, ref bool __result)
        {
            if (___formation == null || ___formation.vehicles.NullOrEmpty())
            {
                return;
            }

            List<VehiclePawn> vehicles = ___formation.vehicles;

            bool isTrainCaravan = false;
            bool locomotivePresent = false;

            foreach (VehiclePawn vehicle in vehicles)
            {
                TrainVehicleComp trainComp = vehicle.GetComp<TrainVehicleComp>();
                if (trainComp != null)
                {
                    isTrainCaravan = true;
                    TrainVehicleCompProperties props = (TrainVehicleCompProperties)trainComp.props;
                    if (props.isLocomotive)
                    {
                        locomotivePresent = true;
                        // Since locomotive is present, we can return early
                        return;
                    }
                }
            }

            if (isTrainCaravan && !locomotivePresent)
            {
                //TOTR_TrainCaravanMustHaveAtLeastOneLocomotive
                Messages.Message("A train caravan must have at least one locomotive.", MessageTypeDefOf.RejectInput, false);
                __result = false;
                return;
            }
        }
    }
}
