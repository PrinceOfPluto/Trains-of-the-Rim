using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Vehicles;
using Verse;

namespace VehiclesTrains
{
    [StaticConstructorOnStartup]
    public static class VehiclesTrainsHarmonyPatches
    {
        static VehiclesTrainsHarmonyPatches()
        {
            var harmony = new Harmony("VehiclesTrains");
            Harmony.DEBUG = true;
            MethodInfo vehiclesMethod = AccessTools.Method(typeof(WorldVehiclePathGrid), "CalculatedMovementDifficultyAt", new[] { typeof(int), typeof(VehicleDef), typeof(int), typeof(StringBuilder), typeof(bool) });
            MethodInfo trainsMethod = typeof(VehiclesTrainsHarmonyPatches).GetMethod("CalculatedMovementDifficultyAtPatchTrains");

            if (vehiclesMethod != null && trainsMethod != null)
            {
                var harmonyMethod = new HarmonyMethod(trainsMethod);
                harmony.Patch(vehiclesMethod, null, harmonyMethod);
                Log.Message("VehiclesTrains: Patched CalculatedMovementDifficultyAt()");
            }
            else
            {
                Log.Error("VehiclesTrains: Unable to patch CalculatedMovementDifficultyAt()");
            }
        }

        /// <summary>
        /// Checks whether vehicle has road restrictions and applies road cost if passable roads are available on tile
        /// </summary>
        /// <param name="tile">integer id of world tile</param>
        /// <param name="vehicleDef">vehicleDef whose movement difficulty is being calcuated</param>
        /// <param name="__result">result from the original CalculatedMovementDifficultyAt method in Vehicle Framework</param>
        public static void CalculatedMovementDifficultyAtPatchTrains(int tile, VehicleDef vehicleDef, ref float __result)
        {
            if (__result == WorldVehiclePathGrid.ImpassableMovementDifficulty)
            {
                // Tile is already impassable for vehicle, likely biome or hilliness impassability
                return;
            }
            float minRoadCost = 0f;
            // Preferable to create a new VehicleProperties field named defaultOffroadImpassable
            if (vehicleDef.buildDef.GetTerrainAffordanceNeed().defName == "RailAffordance" && !vehicleDef.properties.customRoadCosts.NullOrEmpty())
            {
                Dictionary<RoadDef, float> passableRoads = vehicleDef.properties.customRoadCosts
                    .Where(x => x.Value >= 0 && !WorldVehiclePathGrid.ImpassableCost(x.Value))
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    
                if (passableRoads.Count == 0)
                {
                    Log.Warning($"{vehicleDef.defName} is defaultOffroadImpassable but has no passable roads defined in customRoadCosts.");
                    __result = WorldVehiclePathGrid.ImpassableMovementDifficulty;
                    return;
                }
                List<Tile.RoadLink> roadLinks = Find.WorldGrid.tiles[tile].Roads;
                if (roadLinks.NullOrEmpty())
                {
                    // No road links on tile
                    __result = WorldVehiclePathGrid.ImpassableMovementDifficulty;
                    return;
                }
                List<RoadDef> tileRoads = roadLinks.Select(x => x.road).ToList();
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
            }
            __result = __result + minRoadCost;
            return;
        }
    }
}
