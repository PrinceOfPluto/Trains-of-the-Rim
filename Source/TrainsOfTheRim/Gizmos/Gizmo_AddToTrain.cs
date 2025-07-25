﻿using RimWorld;
using System;
using UnityEngine;
using Vehicles;
using Verse;

namespace TrainsOfTheRim.Gizmos
{
    internal class Gizmo_AddToTrain : Command_Target
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

        public Gizmo_AddToTrain(VehiclePawn vehiclePawn)
        {
            owner = vehiclePawn;
            defaultLabel = "TOTR.CoupleToTrainLabel".Translate();
            action = Action;
            targetingParams = TargetingParameters.ForPawns();
            targetingParams.validator = CanAddToTrain;
            icon = ContentFinder<Texture2D>.Get("UI/Gizmos/Couple");
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

        private static bool CanAddToTrain(TargetInfo target)
        {
            TrainVehicleComp trainVehicleComp = target.Thing.TryGetComp<TrainVehicleComp>();
            return trainVehicleComp != null && trainVehicleComp.CanJoinTrain();
        }

        private void Action(LocalTargetInfo target)
        {
            if (!OwnerTrainVehicleComp.HasCurrentTrain())
            {
                throw new Exception($"Cannot add to train. {owner.Name} is not part of a train.");
            }

            TrainVehicleComp targetTrainComp = target.Thing.TryGetComp<TrainVehicleComp>();
            if (targetTrainComp?.currentTrain == null)
            {
                targetTrainComp.TryJoinTrain(OwnerTrainVehicleComp.currentTrain);
            } else
            {
                throw new Exception($"Cannot add to train. {targetTrainComp.Vehicle.Name} is already part of a train.");
            }
        }
    }
}
