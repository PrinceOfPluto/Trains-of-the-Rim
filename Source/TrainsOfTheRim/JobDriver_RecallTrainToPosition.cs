using RimWorld;
using System.Collections.Generic;
using Vehicles;
using Verse;
using Verse.AI;

namespace TrainsOfTheRim
{
    internal class JobDriver_RecallTrainToPosition : JobDriver
    {

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            AddFailCondition(() => !(pawn is VehiclePawn || pawn.Faction == Faction.OfPlayer || (pawn as VehiclePawn).CanMove) || pawn.Downed);

            yield return Toils_Goto.GotoCell(TargetA.Cell, PathEndMode.OnCell);
            var rotationToil = new Toil
            {
                initAction = () =>
                {
                    VehiclePawn vehicle = (VehiclePawn)pawn;
                    TrainVehicleComp trainVehicleComp = vehicle.TryGetComp<TrainVehicleComp>();
                    if (trainVehicleComp != null)
                    {
                        trainVehicleComp.RotateToEndRotation();
                    }
                }
            };
            yield return rotationToil;
        }
    }
}
