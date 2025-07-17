using System;
using UnityEngine;
using Vehicles;
using Verse;

namespace TrainsOfTheRim.Gizmos
{
    internal class Gizmo_RecallTrain : Command_Action
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

        public Gizmo_RecallTrain(VehiclePawn vehiclePawn)
        {
            defaultLabel = "TOTR.RecallTrainLabel".Translate();
            defaultDesc = "TOTR.RecallTrainDesc".Translate();
            owner = vehiclePawn;
            icon = ContentFinder<Texture2D>.Get("UI/Gizmos/RecallTrain");
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            var result = base.GizmoOnGUI(topLeft, maxWidth, parms);

            if (result.State == GizmoState.Mouseover)
            {
                //Log.Message("TOTR - Mouseover");
            }
            return result;
        }

        public override void ProcessInput(Event ev)
        {
            OwnerTrainVehicleComp.RecallConsistToPosition();
        }
    }
}
