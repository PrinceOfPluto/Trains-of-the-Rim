using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vehicles;
using Verse;

namespace TrainsOfTheRim.Patches
{
    [HarmonyPatch(typeof(VehiclePathGrid))]
    internal class Patch_VehiclePathing
    {
        //[HarmonyPostfix]
        //[HarmonyPatch(nameof(VehiclePathGrid.CalculatePathCostFor))]
        public static void CalculatePathCostForTrain(VehicleDef vehicleDef, Map map, IntVec3 cell, StringBuilder stringBuilder, ref int __result)
        {
            TrainVehicleCompProperties trainCompProps = (TrainVehicleCompProperties)vehicleDef.comps.Find(x => x is TrainVehicleCompProperties);
            if (trainCompProps == null)
            {
                // Not a railroad vehicle
                return;
            }

            if (!map.IsPlayerHome)
            {
                // Only require rails at player home
                return;
            }

            stringBuilder?.AppendLine($"Starting patched calculation for railroad vehicle {vehicleDef} at {cell}.");
            try
            {
                ThingGrid thingGrid = map.thingGrid;
                Monitor.Enter(thingGrid);
                try
                {
                    List<Thing> thingList = thingGrid.ThingsListAt(cell);
                    stringBuilder?.AppendLine("Starting ThingList check.");
                    if (thingList.NullOrEmpty())
                    {
                        __result = VehiclePathGrid.ImpassableCost;
                    }
                    else
                    {
                        foreach (Thing thing in thingList)
                        {
                            if (thing.def.defName == "TOTR_Rail")
                            {
                                Log.Message($"Found {thing.def.defName} at cell {cell}");
                                TrainVehicleComp thingTrainComp = thing.TryGetComp<TrainVehicleComp>();
                                if (thingTrainComp != null)
                                {
                                    TrainVehicleCompProperties thingTrainCompProps = (TrainVehicleCompProperties)thingTrainComp.props;
                                    if (thingTrainCompProps != null && thingTrainCompProps.hasRailAffordance)
                                    {
                                        Log.Message($"{thing.def.defName} has rail affordance");
                                        // Has rail affordance so don't change whatever the original result was
                                        __result = 0;
                                        return;
                                    }
                                }
                                else
                                {
                                    Log.Message($"{thing.def.defName} had null thingTrainComp");
                                }
                            }

                        }
                    }
                }
                finally
                {
                    Monitor.Exit(thingGrid);
                }
                __result = VehiclePathGrid.ImpassableCost;
            }
            catch (Exception ex)
            {
                Log.Error(
                  $"Exception thrown while recalculating cost for {vehicleDef} at {cell}.\nException={ex}");
            }
        }
    }
}
