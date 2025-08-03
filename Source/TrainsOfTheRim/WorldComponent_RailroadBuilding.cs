using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vehicles.World;
using Verse;

namespace TrainsOfTheRim
{
    [HarmonyPatch]
    internal class WorldComponent_RailroadBuilding : WorldComponent
    {
        public static WorldComponent_RailroadBuilding Instance { get; private set; }
        private List<Caravan> keys;
        private List<WorkInfo> values;
        public Dictionary<Caravan, WorkInfo> RailWorkInfos = new();

        public WorldComponent_RailroadBuilding(World world) : base(world)
        {
            Instance = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref RailWorkInfos, "railroadWorkInfos", LookMode.Reference, LookMode.Deep, ref keys, ref values);
        }

        [HarmonyPatch(typeof(Caravan), nameof(Caravan.GetInspectString))]
        [HarmonyPostfix]
        public static void AddToString(Caravan __instance, ref string __result)
        {
            if (Instance.RailWorkInfos.TryGetValue(__instance, out var workInfo))
                __result += "\n" + "TOTR.BuildingRoad".Translate(workInfo.ToBuild.LabelCap, (workInfo.WorkDone / (float)workInfo.WorkTotal).ToStringPercent(), workInfo.WorkDone,
                    workInfo.WorkTotal);
        }

        [HarmonyPatch(typeof(Caravan), "TickInterval")]
        [HarmonyPostfix]
        public static void PostTick(Caravan __instance)
        {
            if (IsVFECEnabled())
            {
                return;
            }

            if (__instance.IsHashIntervalTick(250) && Instance.RailWorkInfos.TryGetValue(__instance, out var workInfo))
            {
                workInfo.WorkDone += __instance.PawnsListForReading.Count(p => p.IsFreeColonist && !p.Dead && !p.Downed && !p.InMentalState);
                Instance.RailWorkInfos[__instance] = workInfo;
                if (__instance.Tile != workInfo.Tile || __instance.pather.MovingNow)
                    Instance.RailWorkInfos.Remove(__instance);
                else if (workInfo.WorkDone >= workInfo.WorkTotal)
                {
                    FinishRoad(__instance, workInfo);
                }
            }
        }

        [HarmonyPatch]
        [HarmonyReversePatch]
        [HarmonyPatch(typeof(WorldVehiclePathGrid), "RecalculatePathGrid")]
        internal static void RegenWorldGridForRoads(object instance)
        {
            
        }

        private static void FinishRoad(Caravan __instance, WorkInfo workInfo)
        {
            Find.WorldGrid.OverlayRoad(workInfo.Tile, workInfo.To, workInfo.ToBuild);
            Find.World.renderer.SetAllLayersDirty();
            Instance.RailWorkInfos.Remove(__instance);
            __instance.pather.StartPath(workInfo.To, null, true);
            Messages.Message("TOTR.RoadFinished".Translate(__instance.Name, workInfo.ToBuild.label), __instance, MessageTypeDefOf.TaskCompletion);
            RegenWorldGridForRoads(WorldVehiclePathGrid.Instance);
        }

        private static bool IsVFECEnabled()
        {
            return LoadedModManager.RunningModsListForReading.Any(x => x.PackageIdPlayerFacing == "OskarPotocki.VFE.Classical");
        }

        [HarmonyPatch(typeof(Caravan), nameof(Caravan.GetGizmos))]
        [HarmonyPostfix]
        public static IEnumerable<Gizmo> AddRoadGizmos(IEnumerable<Gizmo> gizmos, Caravan __instance)
        {
            foreach (var gizmo in gizmos) yield return gizmo;

            if(LoadedModManager.GetMod<TrainMod>().GetSettings<TrainModSettings>().hideRoadBuildingFeature)
            {
                yield break;
            }

            if (IsVFECEnabled())
            {
                yield break;
            }

            if (Instance.RailWorkInfos.ContainsKey(__instance))
            {
                if (DebugSettings.godMode)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "TOTR.DebugFinishRoad".Translate(),
                        action = delegate
                        {
                            if (Instance.RailWorkInfos.TryGetValue(__instance, out var workInfo))
                            {
                                FinishRoad(__instance, workInfo);
                            }
                        }
                    };
                }
                else
                {
                    yield break;
                }
            }

            if (__instance.pather.MovingNow) yield break;

            var allRoads = DefDatabase<RoadBuildingDef>.AllDefs.ToList();
            var knownRoads = allRoads.FindAll(r => r.researchPrerequisites.All(research => research.IsFinished));

            foreach (var def in knownRoads)
                yield return new Command_Action
                {
                    defaultLabel = def.label,
                    defaultDesc = def.description,
                    icon = def.Icon,
                    action = delegate
                    {
                        Find.WorldTargeter.BeginTargeting(target =>
                        {
                            Instance.RailWorkInfos.Add(__instance, new WorkInfo
                            {
                                Caravan = __instance,
                                ToBuild = def.road,
                                WorkDone = 0,
                                WorkTotal = def.workRequired,
                                Tile = __instance.Tile,
                                To = target.Tile
                            });
                            return true;
                        }, true, canSelectTarget: target => Find.WorldGrid.IsNeighbor(__instance.Tile, target.Tile));
                    }
                };
        }
    }

    public class WorkInfo : IExposable
    {
        public Caravan Caravan;
        public int Tile;
        public int To;
        public RoadDef ToBuild;
        public int WorkDone;
        public int WorkTotal;

        public void ExposeData()
        {
            Scribe_Values.Look(ref WorkDone, "workDone");
            Scribe_Values.Look(ref WorkTotal, "workTotal");
            Scribe_References.Look(ref Caravan, "caravan");
            Scribe_Defs.Look(ref ToBuild, "toBuild");
            Scribe_Values.Look(ref Tile, "tile", -1);
            Scribe_Values.Look(ref To, "to", -1);
        }
    }

    public class RoadBuildingDef : Def
    {
        public Texture2D Icon;
        public string iconPath;
        public RoadDef road;
        public int workRequired;
        public List<ResearchProjectDef> researchPrerequisites;

        public override void PostLoad()
        {
            base.PostLoad();
            LongEventHandler.ExecuteWhenFinished(() => { Icon = ContentFinder<Texture2D>.Get(iconPath); });
        }
    }
}
