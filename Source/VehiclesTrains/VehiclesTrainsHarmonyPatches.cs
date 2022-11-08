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
            MethodInfo trainsMethod = typeof(VehiclesTrainsHarmonyPatches).GetMethod("CalculatedMovementDifficultyAtPatch");

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

        public static void CalculatedMovementDifficultyAtPatch(int tile, VehicleDef vehicleDef, ref float __result)
        {
            if (__result == WorldVehiclePathGrid.ImpassableMovementDifficulty)
            {
                return;
            }
            float minRoadCost = 0f;
            if (vehicleDef.buildDef.GetTerrainAffordanceNeed().defName == "RailAffordance" && !vehicleDef.properties.customRoadCosts.NullOrEmpty())
            {
                Dictionary<RoadDef, float> passableRoads = vehicleDef.properties.customRoadCosts.Where(x => x.Value >= 0 && !WorldVehiclePathGrid.ImpassableCost(x.Value)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                if (passableRoads.Count == 0)
                {
                    Log.Warning($"{vehicleDef.defName} is defaultOffroadImpassable but has no passable roads defined in customRoadCosts.");
                    __result = WorldVehiclePathGrid.ImpassableMovementDifficulty;
                    return;
                }
                List<Tile.RoadLink> roadLinks = Find.WorldGrid.tiles[tile].Roads;
                if (roadLinks.NullOrEmpty())
                {
                    __result = WorldVehiclePathGrid.ImpassableMovementDifficulty;
                    return;
                }
                List<RoadDef> tileRoads = roadLinks.Select(x => x.road).ToList();
                foreach (RoadDef road in tileRoads)
                {
                    if (passableRoads.ContainsKey(road))
                    {
                        minRoadCost = minRoadCost == 0f ? passableRoads[road] : Math.Min(minRoadCost, passableRoads[road]);
                    }
                }
                if (minRoadCost == 0f)
                {
                    __result = WorldVehiclePathGrid.ImpassableMovementDifficulty;
                    return;
                }
            }
            __result = __result + minRoadCost;
            return;
        }
    }
}
