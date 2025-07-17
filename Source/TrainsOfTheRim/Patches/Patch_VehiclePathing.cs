using HarmonyLib;
using System;
using System.Text;
using Vehicles;
using Verse;

namespace TrainsOfTheRim.Patches
{
    [HarmonyPatch(typeof(VehiclePathGrid))]
    internal class Patch_VehiclePathing
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(VehiclePathGrid.CalculatePathCostFor))]
        public static void CalculatePathCostForTrain(VehicleDef vehicleDef, Map map, IntVec3 cell, StringBuilder stringBuilder, ref int __result)
        {
            if (__result == VehiclePathGrid.ImpassableCost)
            {
                return;
            }

            if (!LoadedModManager.GetMod<TrainMod>().GetSettings<TrainModSettings>().requireRailroadTerrain)
            {
                return;
            }

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

            if (cell.OnEdge(map) || cell.CloseToEdge(map,10) || map.exitMapGrid.IsExitCell(cell))
            {
                // allow non-rails at exit grid
                return;
            }
            

            stringBuilder?.AppendLine($"Starting patched calculation for railroad vehicle {vehicleDef} at {cell}.");
            try
            {
                TerrainDef terrainDef = map.terrainGrid.TerrainAt(cell);
                if (vehicleDef.properties.customTerrainCosts.TryGetValue(terrainDef, out int terrainCost))
                {
                    __result = terrainCost;
                    return;
                }
                if (terrainDef.HasTag("Rail"))
                {
                    __result = 1;
                    return;
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
