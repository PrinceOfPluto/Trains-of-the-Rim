using HarmonyLib;
using RimWorld;
using SmashTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TrainsOfTheRim.Patches;
using UnityEngine;
using Vehicles;
using Verse;
using Verse.AI;

namespace TrainsOfTheRim
{
    public class TrainConsist : IExposable
    {
        private List<VehiclePawn> members;
        public List<VehiclePawn> Members
        {
            get { return members; }
        }

        // Locomotive position being at 0,0,0
        public Dictionary<VehiclePawn, TrainVehiclePosition> relativePositions;
        private List<VehiclePawn> tempVehiclePawns;
        private List<TrainVehiclePosition> tempVehiclePositions;

        public IEnumerable<VehiclePawn> ValidMembers
        {
            get { return Members.Where(IsEligibleForMembership); }
        }

        public bool ShouldBeSaved
        {
            get { return ValidMembers.Any(); }
        }

        public TrainConsist() { }

        public TrainConsist(VehiclePawn vehicle)
        {
            TrainVehicleComp trainVehicleComp = vehicle.TryGetComp<TrainVehicleComp>();
            if (trainVehicleComp == null || !trainVehicleComp.Props.isLocomotive)
            {
                throw new ArgumentException($"Cannot create new train. {vehicle.Name} is not a locomotive.");
            }
            EnsureMemberPositionsInitialized();
            AddToConsist(vehicle);
            SetRelativePositionOfLocomotive(vehicle);
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                RemoveInvalidMembers();
            }
            Scribe_Collections.Look(ref members, "members", LookMode.Reference);
            Scribe_Collections.Look(ref relativePositions, "relativePositions", LookMode.Reference, LookMode.Deep, ref tempVehiclePawns, ref tempVehiclePositions);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                EnsureMemberPositionsInitialized();
                RemoveInvalidMembers();
            }
        }

        public void AddToConsist(VehiclePawn p)
        {
            TrainVehicleComp trainVehicleComp = p.TryGetComp<TrainVehicleComp>();
            if (trainVehicleComp == null || !trainVehicleComp.Props.isRailroadVehicle)
            {
                throw new ArgumentException($"Cannot add non-railroad vehicle {p.Name} to train consist.");
            }
            if (members.Contains(p))
            {
                throw new ArgumentException($"VehiclePawn {p.Name} is already part of train consist");
            }
            members.Add(p);
            trainVehicleComp.currentTrain = this;
        }

        public bool RemoveFromConsist(VehiclePawn p)
        {
            bool result = false;
            if (members.Contains(p))
            {
                members.Remove(p);
                result = true;
            }
            TrainVehicleComp trainVehicleComp = p.TryGetComp<TrainVehicleComp>();
            if (trainVehicleComp != null)
            {
                trainVehicleComp.ClearSavedPositions();
                trainVehicleComp.currentTrain = null;
            }
            return result;
        }

        private void SetRelativePositionOfLocomotive(VehiclePawn locomotive)
        {
            relativePositions.SetOrAdd(locomotive, new TrainVehiclePosition(new IntVec3(0, 0, 0), locomotive.Rotation));
        }

        public void RecallConsistToPosition()
        {
            foreach (VehiclePawn v in members)
            {
                TrainVehicleComp trainVehicleComp = v.TryGetComp<TrainVehicleComp>();
                if (trainVehicleComp.CanRecallToPosition())
                {
                    trainVehicleComp.RecallToCurrentMapSavedPosition();
                }
            }
        }

        public void SaveRelativeMemberPosition(VehiclePawn p)
        {
            EnsureMemberPositionsInitialized();
            if (relativePositions.Count > 0)
            {
                IntVec3 currentLocomotivePosition = members.First().Position;
                int xOffset = p.Position.x - currentLocomotivePosition.x;
                int yOffset = p.Position.y - currentLocomotivePosition.y;
                int zOffset = p.Position.z - currentLocomotivePosition.z;
                relativePositions.SetOrAdd(p, new TrainVehiclePosition(new IntVec3(xOffset, yOffset, zOffset), p.Rotation));
            }
            else
            {
                TrainVehicleComp trainVehicleComp = p.TryGetComp<TrainVehicleComp>();
                if (trainVehicleComp.Props.isLocomotive)
                {
                    SetRelativePositionOfLocomotive(p);
                }
                else
                {
                    throw new Exception($"Cannot save relative member position for {p.VehicleDef.defName} id {p.ThingID} because it is not a locomotive");
                }
            }

        }

        public void OrderConsistTo(TrainVehicleComp locomotive, IntVec3 clickCell, IntVec3 gotoLoc, Rot8 rot)
        {
            foreach (VehiclePawn vehicle in members.FindAll(v => v != locomotive.Vehicle))
            {
                TrainVehicleComp trainVehicleComp = vehicle.TryGetComp<TrainVehicleComp>();
                if (trainVehicleComp == null)
                {
                    Log.Error($"Member of the train {vehicle.VehicleDef.defName} {vehicle.ThingID} is somehow not a train vehicle");
                    return;
                }

                TrainVehiclePosition savedPositionOfLocomotive = relativePositions.TryGetValue(members[0]);
                if (savedPositionOfLocomotive == null)
                {
                    Log.Error("Saved position of locomotive was null");
                    return;
                }

                Rot4 savedRotationOfLocomotive = savedPositionOfLocomotive.rotation;
                if (!relativePositions.ContainsKey(vehicle))
                {
                    Log.Error($"Vehicle {vehicle.VehicleDef.defName} was in the train, but does not have any relative positions stored");
                    return;
                }

                TrainVehiclePosition relativeFollowerPosition = relativePositions.TryGetValue(vehicle);
                if (relativeFollowerPosition == null)
                {
                    Log.Error($"Relative follower position for {vehicle.VehicleDef.defName} was null");
                    return;
                }

                Rot8 locomotiveEndRotation = savedRotationOfLocomotive;
                if(!rot.IsValid)
                {
                    // default to saved rotation
                    locomotive.Vehicle.vehiclePather.SetEndRotation(savedRotationOfLocomotive);
                } else
                {
                    locomotiveEndRotation = rot;
                }

                RotationDirection rotationDirection =  Rot4.GetRelativeRotation(savedRotationOfLocomotive, Rot8.ToRot4(locomotiveEndRotation));
                Rot4 targetRotation = relativeFollowerPosition.rotation.Rotated(rotationDirection);

                int targetX;
                int targetY;
                int targetZ;
                switch (rotationDirection)
                {
                    case RotationDirection.Clockwise:
                        targetX = relativeFollowerPosition.position.z + gotoLoc.z;
                        targetY = relativeFollowerPosition.position.y;
                        targetZ = -relativeFollowerPosition.position.x + gotoLoc.x;
                        break;
                    case RotationDirection.Opposite:
                        targetX = -relativeFollowerPosition.position.x + gotoLoc.x;
                        targetY = relativeFollowerPosition.position.y;
                        targetZ = -relativeFollowerPosition.position.z + gotoLoc.z;
                        break;
                    case RotationDirection.Counterclockwise:
                        targetX = -relativeFollowerPosition.position.z + gotoLoc.x;
                        targetY = relativeFollowerPosition.position.y;
                        targetZ = relativeFollowerPosition.position.x + gotoLoc.z;
                        break;
                    case RotationDirection.None:
                    default:
                        targetX = relativeFollowerPosition.position.x + gotoLoc.x;
                        targetY = relativeFollowerPosition.position.y;
                        targetZ = relativeFollowerPosition.position.z + gotoLoc.z;
                        break;
                }

                IntVec3 targetPosition = new(targetX, targetY, targetZ);
                trainVehicleComp.Vehicle.ignition.Drafted = true;
                trainVehicleComp.OrderToPosition(targetPosition, targetRotation);
            }
        }

        private static bool IsEligibleForMembership(VehiclePawn v)
        {
            TrainVehicleComp trainVehicleComp = v.TryGetComp<TrainVehicleComp>();
            if (trainVehicleComp == null || !trainVehicleComp.Props.isRailroadVehicle)
            {
                return false;
            }
            return v != null && !v.Destroyed && v.Faction != null && v.Faction.IsPlayer;
        }

        private void EnsureMemberPositionsInitialized()
        {
            if (members == null) members = new List<VehiclePawn>();
            if (relativePositions == null) relativePositions = new Dictionary<VehiclePawn, TrainVehiclePosition>();
        }

        private void RemoveInvalidMembers()
        {
            members.RemoveAll(v => !IsEligibleForMembership(v));
        }
    }
}
