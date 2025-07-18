using System;
using UnityEngine;
using Vehicles;
using Verse;

namespace TrainsOfTheRim
{
    public class Gizmo_CycleTrainTexture : Command_Action
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

        public Gizmo_CycleTrainTexture(VehiclePawn vehiclePawn)
        {
            defaultLabel = "TOTR.CycleTextureLabel".Translate();
            defaultDesc = "TOTR.CycleTextureDesc".Translate();
            owner = vehiclePawn;
            icon = ContentFinder<Texture2D>.Get("UI/Gizmos/CycleTexture");
        }

        public override void ProcessInput(Event ev)
        {
            OwnerTrainVehicleComp.CycleTexture();
        }
    }
}
