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
                            RailComp railComp = thing.TryGetComp<RailComp>();
                            if (railComp != null && railComp.Props != null && railComp.Props.hasRailAffordance)
                            {
                                Log.Message($"{thing.def.defName} has rail affordance");
                                // Has rail affordance so don't change whatever the original result was
                                __result = 0;
                                return;
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
