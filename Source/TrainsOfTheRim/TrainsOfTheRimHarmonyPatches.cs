using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using SmashTools;
using Vehicles;
using Vehicles.World;
using Verse;

namespace TrainsOfTheRim
{
    [StaticConstructorOnStartup]
    public static class TrainsOfTheRimHarmonyPatches
    {
        static TrainsOfTheRimHarmonyPatches()
        {
            var harmony = new Harmony("VehiclesTrains");
            Harmony.DEBUG = true;
            MethodInfo vehiclesMovementMethod = AccessTools.Method(typeof(WorldVehiclePathGrid), "CalculatedMovementDifficultyAt", new[] { typeof(PlanetTile), typeof(VehicleDef), typeof(int), typeof(StringBuilder), typeof(bool) });
            MethodInfo trainsMovementMethod = typeof(TrainsOfTheRimHarmonyPatches).GetMethod("CalculatedMovementDifficultyAtPatchTrains");

            if (vehiclesMovementMethod != null && trainsMovementMethod != null)
            {
                harmony.Patch(vehiclesMovementMethod, postfix: new HarmonyMethod(trainsMovementMethod));
                Log.Message("VehiclesTrains: Patched WorldVehiclePathGrid.CalculatedMovementDifficultyAt()");
            }
            else
            {
                Log.Error("VehiclesTrains: Unable to patch CalculatedMovementDifficultyAt()");
            }

            MethodInfo vehiclesCaravanErrorsMethod = AccessTools.Method(typeof(CaravanFormation), "CheckForErrors");
            MethodInfo trainsCaravanErrorsMethod = typeof(TrainsOfTheRimHarmonyPatches).GetMethod("CheckForTrainCaravanErrors");

            if (vehiclesCaravanErrorsMethod != null && trainsCaravanErrorsMethod != null)
            {
                harmony.Patch(vehiclesCaravanErrorsMethod, postfix: new HarmonyMethod(trainsCaravanErrorsMethod));
                Log.Message("VehiclesTrains: Patched Dialog_FormVehicleCaravan.CheckForErrors()");
            }
            else
            {
                Log.Error("VehiclesTrains: Unable to patch Dialog_FormVehicleCaravan.CheckForErrors()");
            }

            //MethodInfo vehiclesPathMethod = AccessTools.Method(typeof(VehiclePathGrid), "CalculatePathCostFor", new[] { typeof(VehicleDef), typeof(Map), typeof(IntVec3), typeof(StringBuilder) });
            //MethodInfo trainsPathMethod = typeof(TrainsOfTheRimHarmonyPatches).GetMethod("CalculatePathCostForTrain");
            //if (vehiclesPathMethod != null && trainsPathMethod != null)
            //{
            //    harmony.Patch(vehiclesPathMethod, postfix: new HarmonyMethod(trainsPathMethod));
            //    Log.Message("VehiclesTrains: Patched VehiclePathGrid.CalculatePathCostFor()");
            //}
            //else
            //{
            //    Log.Error("VehiclesTrains: Unable to patch VehiclePathGrid.CalculatePathCostFor()");
            //}

        }

        /// <summary>
        /// Checks whether vehicle has road restrictions and applies road cost if passable roads are available on tile
        /// </summary>
        /// <param name="tile">integer id of world tile</param>
        /// <param name="vehicleDef">vehicleDef whose movement difficulty is being calcuated</param>
        /// <param name="__result">result from the original CalculatedMovementDifficultyAt method in Vehicle Framework</param>
        public static void CalculatedMovementDifficultyAtPatchTrains(PlanetTile tile, VehicleDef vehicleDef, ref float __result)
        {
            if (__result == WorldVehiclePathGrid.ImpassableMovementDifficulty)
            {
                // Tile is already impassable for vehicle
                return;
            }

            TrainCompProperties trainCompProps = (TrainCompProperties)vehicleDef.comps.Find(x => x is TrainCompProperties);
            if (trainCompProps == null)
            {
                // Not a railroad vehicle
                return;
            }

            if (!trainCompProps.isRailroadVehicle)
            {
                Log.WarningOnce($"{vehicleDef.defName} with TrainCompProperties is not marked as a railroad vehicle", 2025070801 + vehicleDef.DefIndex);
                return;
            }

            if (vehicleDef.properties.customRoadCosts.NullOrEmpty())
            {
                Log.ErrorOnce($"{vehicleDef.defName} has no custom road costs defined.", 2025070802 + vehicleDef.DefIndex);
                return;
            }

            Dictionary<RoadDef, float> passableRoads = vehicleDef.properties.customRoadCosts
                .Where(x => x.Value >= 0 && x.Value <= WorldVehiclePathGrid.ImpassableMovementDifficulty)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            if (passableRoads.Count == 0)
            {
                Log.WarningOnce($"{vehicleDef.defName} is recognized as a railroad vehicle but has no passable roads defined in customRoadCosts.", 2025070803 + vehicleDef.DefIndex);
                __result = WorldVehiclePathGrid.ImpassableMovementDifficulty;
                return;
            }

            SurfaceTile worldTile = (SurfaceTile)Find.WorldGrid[tile];
            List<SurfaceTile.RoadLink> roadLinks = worldTile.Roads;
            if (roadLinks.NullOrEmpty())
            {
                // No road links on tile
                __result = WorldVehiclePathGrid.ImpassableMovementDifficulty;
                return;
            }

            List<RoadDef> tileRoads = roadLinks.Select(x => x.road).ToList();
            float minRoadCost = 0f;
            foreach (RoadDef road in tileRoads)
            {
                if (passableRoads.ContainsKey(road))
                {
                    // If minRoadCost still initialized as 0f, assign current road's cost, otherwise take the least costly road
                    minRoadCost = minRoadCost == 0f ? passableRoads[road] : Math.Min(minRoadCost, passableRoads[road]);
                }
            }
            if (minRoadCost == 0f)
            {
                // Tile has no roads matching vehicle's passable roads in customRoadCosts
                __result = WorldVehiclePathGrid.ImpassableMovementDifficulty;
                return;
            }

            __result += minRoadCost;
            return;
        }

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
                TrainComp trainComp = vehicle.GetComp<TrainComp>();
                if (trainComp != null)
                {
                    isTrainCaravan = true;
                    TrainCompProperties props = (TrainCompProperties)trainComp.props;
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

        public static void CalculatePathCostForTrain(VehicleDef vehicleDef, Map map, IntVec3 cell, StringBuilder stringBuilder, ref int __result)
        {
            TrainCompProperties trainCompProps = (TrainCompProperties)vehicleDef.comps.Find(x => x is TrainCompProperties);
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
                            if(thing.def.defName == "TOTR_Rail")
                            {
                                Log.Message($"Found {thing.def.defName} at cell {cell}");
                                TrainComp thingTrainComp = thing.TryGetComp<TrainComp>();
                                if (thingTrainComp != null)
                                {
                                    TrainCompProperties thingTrainCompProps = (TrainCompProperties)thingTrainComp.props;
                                    if (thingTrainCompProps != null && thingTrainCompProps.hasRailAffordance)
                                    {
                                        Log.Message($"{thing.def.defName} has rail affordance");
                                        // Has rail affordance so don't change whatever the original result was
                                        __result = 0;
                                        return;
                                    }
                                } else
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
