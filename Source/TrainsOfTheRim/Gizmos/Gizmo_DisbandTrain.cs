using System;
using UnityEngine;
using Vehicles;
using Verse;

namespace TrainsOfTheRim.Gizmos
{
    internal class Gizmo_DisbandTrain : Command_Action
    {
        public VehiclePawn owner;
        public TrainVehicleComp OwnerTrainVehicleComp
        {
            get
            {
                var trainVehicleComp = owner.TryGetComp<TrainVehicleComp>();
                if (trainVehicleComp == null)
                {
                    throw new Exception($"TrainVehicleComp was null for vehicle {owner.Name}");
                }
                return trainVehicleComp;
            }
        }

        public Gizmo_DisbandTrain(VehiclePawn vehiclePawn)
        {
            owner = vehiclePawn;
            defaultLabel = "TOTR.DisbandTrainLabel".Translate();
            defaultDesc = "TOTR.DisbandTrainDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Gizmos/DisbandTrain");
        }

        public override void ProcessInput(Event ev)
        {
            WorldComponent_Trains.Instance.DisbandTrain(OwnerTrainVehicleComp.currentTrain);
        }
    }
}
