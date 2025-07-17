using System;
using System.Collections.Generic;
using System.Linq;
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
            if (trainVehicleComp == null || !trainVehicleComp.Props.isRailroadVehicle)
            {
                throw new ArgumentException($"Cannot create new train. {vehicle.Name} is not a locomotive.");
            }
            EnsureMemberPositionsInitialized();
            AddToConsist(vehicle);
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                RemoveInvalidMembers();
            }
            Scribe_Collections.Look(ref members, "members", LookMode.Reference);
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

        public void RecallConsistToPosition()
        {
            foreach (VehiclePawn v in members)
            {
                TrainVehicleComp trainVehicleComp = v.TryGetComp<TrainVehicleComp>();
                if(trainVehicleComp.CanRecallToPosition())
                {
                    trainVehicleComp.RecallToPosition();
                }
            }
        }

        public void OrderFollowVehicleAhead()
        {
            // start at 1 to skip the locomotive
            for (int i = 1; i < members.Count; i++)
            {
                VehiclePawn targetToFollow = members[i-1];
                VehiclePawn follower = members[i];

                follower.mindState.duty = new PawnDuty(DutyDefOf_Vehicles.FollowVehicle, targetToFollow, targetToFollow.VehicleDef.Size.z * 1.5f)
                {
                    locomotion = LocomotionUrgency.Sprint
                };
            }
        }

        private static bool IsEligibleForMembership(VehiclePawn v)
        {
            TrainVehicleComp trainVehicleComp = v.TryGetComp<TrainVehicleComp>();
            if (trainVehicleComp == null || !trainVehicleComp.Props.isRailroadVehicle) {
                return false;
            }
            return v != null && !v.Destroyed && v.Faction != null && v.Faction.IsPlayer;
        }

        private void EnsureMemberPositionsInitialized()
        {
            if (members == null) members = new List<VehiclePawn>();
        }

        private void RemoveInvalidMembers()
        {
            members.RemoveAll(v => !IsEligibleForMembership(v));
        }
    }
}
