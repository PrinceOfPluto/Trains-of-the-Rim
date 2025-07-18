using RimWorld;
using System;
using UnityEngine;
using Vehicles;
using Verse;

namespace TrainsOfTheRim.Gizmos
{
    internal class Gizmo_SaveNewPosition : Command_Action
    {
        private VehiclePawn owner;
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

        public Gizmo_SaveNewPosition(VehiclePawn vehiclePawn)
        {
            owner = vehiclePawn;
            defaultLabel = "TOTR.SaveNewPositionLabel".Translate();
            defaultDesc = "TOTR.SaveNewPositionDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Gizmos/SaveTrainPosition");
        }

        public override void ProcessInput(Event ev)
        {
            OwnerTrainVehicleComp.SavePosition();
        }
    }
}
