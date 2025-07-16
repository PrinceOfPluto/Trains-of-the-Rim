using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicles;
using Verse;
using Verse.AI;
using Verse.Noise;

namespace TrainsOfTheRim
{
    public class TrainVehicleComp : VehicleComp
    {
        public TrainConsist currentTrain;
        public TrainVehicleCompProperties Props => (TrainVehicleCompProperties)this.props;

        public Dictionary<Map, TrainVehiclePosition> savedPositions = new Dictionary<Map, TrainVehiclePosition>();
        private List<Map> tempPositionsKeyList;
        private List<TrainVehiclePosition> tempPositionsValueList;

        public Trains_WorldComp worldComp;

        public TrainVehicleComp()
        {
            worldComp = Find.World.GetComponent<Trains_WorldComp>();
        }

        public bool HasCurrentTrain()
        {
            return currentTrain != null;
        }

        public bool CanAddToTrain()
        {
            return worldComp.trainConsists.Count > 0 && currentTrain == null;
        }

        public void TryAddToTrain(TrainConsist trainConsist)
        {
            if (!Props.isRailroadVehicle)
            {
                throw new ArgumentException($"Cannot add to train. {Vehicle.Name} is not a railroad vehicle.");
            }
            if (currentTrain != null)
            {
                throw new ArgumentException($"Cannot add to train. {Vehicle.Name} is already part of another train.");
            }
            worldComp.AddToTrain(Vehicle, trainConsist);
            SavePosition();
        }

        public bool CanCreateNewTrain()
        {
            return Props.isLocomotive && currentTrain == null;
        }

        public void TryCreateNewTrain()
        {
            if (!Props.isLocomotive)
            {
                throw new ArgumentException($"Cannot create new train. {Vehicle.Name} is not a locomotive.");
            }
            if (currentTrain != null)
            {
                throw new ArgumentException($"Cannot create new train. {Vehicle.Name} is already part of another train.");
            }
            currentTrain = worldComp.CreateNewTrain(Vehicle);
            SavePosition();
        }

        public void RemoveFromTrain()
        {
            if (currentTrain != null)
            {
                worldComp.RemoveFromTrain(Vehicle, currentTrain);
                currentTrain = null;
                savedPositions.Clear();
            }
        }

        public void SavePosition()
        {
            if(savedPositions ==  null)
            {
                savedPositions = new Dictionary<Map, TrainVehiclePosition>();
            }
            if (Vehicle.Map == null)
            {
                throw new Exception($"Cannot save train position. {Vehicle.ThingID} had null map.");
            }
            savedPositions.Add(Vehicle.Map, new TrainVehiclePosition(Vehicle.Position, Vehicle.Rotation));
        }

        public bool CanRecallToPosition()
        {
            return Vehicle.Map != null && savedPositions.ContainsKey(Vehicle.Map);
        }

        public void RecallToPosition()
        {
            if (Vehicle.Map == null)
            {
                throw new Exception($"Cannot recall to train position. {Vehicle.ThingID} had null map.");
            }
            if (!savedPositions.ContainsKey(Vehicle.Map))
            {
                throw new Exception($"Cannot recall to train position. {Vehicle.ThingID} has no saved position on its current map.");
            }
            IntVec3 targetPosition = savedPositions[Vehicle.Map].position;
            var job = JobMaker.MakeJob(Resources.Jobs.TOTR_RecallTrainToPosition, targetPosition);
            Vehicle.jobs.TryTakeOrderedJob(job, JobTag.DraftedOrder);
        }

        public void RecallToRotation()
        {
            if (savedPositions == null)
            {
                throw new Exception($"Cannot recall to train rotation. {Vehicle.ThingID} had null saved positions.");
            }
            if (!savedPositions.ContainsKey(Vehicle.Map))
            {
                throw new Exception($"Cannot recall to train rotation. {Vehicle.ThingID} has no saved position on its current map");
            }
            Vehicle.Rotation = savedPositions[Vehicle.Map].rotation;
        }

        public override void PostExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                tempPositionsKeyList = savedPositions.Keys.ToList();
                tempPositionsValueList = savedPositions.Values.ToList();
            }
            Scribe_Deep.Look(ref currentTrain, "currentTrain");
            Scribe_Collections.Look(ref tempPositionsKeyList, "savedPositionsKeys", LookMode.Reference);
            Scribe_Collections.Look(ref tempPositionsValueList, "savedPositionsvalues", LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                savedPositions = new Dictionary<Map, TrainVehiclePosition>();
                for(int i = 0; i < tempPositionsKeyList.Count; i++)
                {
                    savedPositions.Add(tempPositionsKeyList[i], tempPositionsValueList[i]);
                }
                tempPositionsKeyList = null;
                tempPositionsValueList = null;
            }
            base.PostExposeData();
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            RemoveFromTrain();
            base.PostDestroy(mode, previousMap);
        }
    }
}
