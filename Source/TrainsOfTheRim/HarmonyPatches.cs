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
using TrainsOfTheRim.Patches;
using Vehicles;
using Vehicles.World;
using Verse;

namespace TrainsOfTheRim
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("TrainsOfTheRim");
            Harmony.DEBUG = true;
            MethodInfo vehiclesMovementMethod = AccessTools.Method(typeof(WorldVehiclePathGrid), "CalculatedMovementDifficultyAt");
            MethodInfo trainsMovementMethod = typeof(HarmonyPatches).GetMethod("CalculatedMovementDifficultyAtPatchTrains");

            if (vehiclesMovementMethod != null && trainsMovementMethod != null)
            {
                harmony.Patch(vehiclesMovementMethod, postfix: new HarmonyMethod(trainsMovementMethod));
                Log.Message("TrainsOfTheRim: Patched WorldVehiclePathGrid.CalculatedMovementDifficultyAt()");
            }
            else
            {
                Log.Error($"TrainsOfTheRim: Unable to patch CalculatedMovementDifficultyAt(). Vehicle method ${vehiclesMovementMethod == null}, train method ${trainsMovementMethod == null}");
            }

            MethodInfo vehiclesCaravanErrorsMethod = AccessTools.Method(typeof(CaravanFormation), "CheckForErrors");
            MethodInfo trainsCaravanErrorsMethod = typeof(HarmonyPatches).GetMethod("CheckForTrainCaravanErrors");

            if (vehiclesCaravanErrorsMethod != null && trainsCaravanErrorsMethod != null)
            {
                harmony.Patch(vehiclesCaravanErrorsMethod, postfix: new HarmonyMethod(trainsCaravanErrorsMethod));
                Log.Message("TrainsOfTheRim: Patched Dialog_FormVehicleCaravan.CheckForErrors()");
            }
            else
            {
                Log.Error("TrainsOfTheRim: Unable to patch Dialog_FormVehicleCaravan.CheckForErrors()");
            }

            MethodInfo vehicleGizmoMethod = AccessTools.Method(typeof(VehiclePawn), nameof(VehiclePawn.GetGizmos));
            MethodInfo trainGizmoMethod = AccessTools.Method(typeof(Patch_Gizmos), nameof(Patch_Gizmos.GetTrainGizmos));
            if (vehicleGizmoMethod != null && trainGizmoMethod != null)
            {
                harmony.Patch(vehicleGizmoMethod, postfix: new HarmonyMethod(trainGizmoMethod));
                Log.Message("TrainsOfTheRim: Patched train gizmos");
            }
            else
            {
                Log.Error($"TrainsOfTheRim: Unable to patch train gizmos, {vehicleGizmoMethod == null}, {trainGizmoMethod == null}");
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

            TrainVehicleCompProperties trainCompProps = (TrainVehicleCompProperties)vehicleDef.comps.Find(x => x is TrainVehicleCompProperties);
            if (trainCompProps == null)
            {
                // Not a railroad vehicle
                return;
            }

            SurfaceTile surfaceTile = (SurfaceTile)Find.WorldGrid[tile];
            if (surfaceTile.Roads.NullOrEmpty() || !surfaceTile.Roads.Exists(PassableRoad))
            {
                // No road links on tile
                __result = WorldVehiclePathGrid.ImpassableMovementDifficulty;
                return;
            }

            bool PassableRoad(SurfaceTile.RoadLink roadLink)
            {
                return vehicleDef.properties.customRoadCosts.ContainsKey(roadLink.road);
            }
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
                            if(thing.def.defName == "TOTR_Rail")
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
