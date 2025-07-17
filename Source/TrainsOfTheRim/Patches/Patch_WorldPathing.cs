using RimWorld.Planet;
using Vehicles.World;
using Vehicles;
using Verse;
using HarmonyLib;

namespace TrainsOfTheRim.Patches
{
    [HarmonyPatch(typeof(WorldVehiclePathGrid))]
    internal class Patch_WorldPathing
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(WorldVehiclePathGrid.CalculatedMovementDifficultyAt))]
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
    }
}
