using LudeonTK;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vehicles;
using Verse;

namespace TrainsOfTheRim
{
    public class WorldComponent_Trains : WorldComponent
    {
        public static WorldComponent_Trains Instance { get; private set; }

        public List<TrainConsist> trainConsists = new List<TrainConsist>();

        public WorldComponent_Trains(World world) : base(world)
        {
            Instance = this;
        }

        public void AddToTrain(VehiclePawn vehiclePawn, TrainConsist trainConsist)
        {
            trainConsist.AddToConsist(vehiclePawn);
            Log.Message($"Added {vehiclePawn.Name} to existing train consist");
        }

        public TrainConsist CreateNewTrain(VehiclePawn vehiclePawn)
        {
            TrainConsist consist = new TrainConsist(vehiclePawn);
            this.trainConsists.Add(consist);
            Log.Message($"Added {vehiclePawn.Name} to new train consist");
            return consist;
        }

        public void RemoveFromTrain(VehiclePawn vehiclePawn, TrainConsist trainConsist)
        {
            if (trainConsist.Members.Remove(vehiclePawn))
            {
                Log.Message($"Removed {vehiclePawn.Name} from train");
            };
            if (trainConsist.Members.NullOrEmpty())
            {
                Log.Message($"Removed empty train consist");
                trainConsists.Remove(trainConsist);
            }
        }

        [DebugAction("Trains", "List current trains", actionType = DebugActionType.Action)]
        public static void ListCurrentTrains()
        {
            WorldComponent_Trains worldComp = Find.World.GetComponent<WorldComponent_Trains>();
            if (worldComp.trainConsists.NullOrEmpty())
            {
                Log.Message($"No trains currently exist");
            }
            StringBuilder sb = new StringBuilder();
            int counter = 0;
            worldComp.trainConsists.ForEach(trainConsist =>
            {
                counter++;
                var members = trainConsist.Members;
                sb.AppendLine($"Train {counter} has {members.Count} members:");
                members.ForEach(member =>
                {
                    var trainVehicleComp = member.TryGetComp<TrainVehicleComp>();
                    sb.AppendLine($"Member {member.ThingID} has {trainVehicleComp?.savedPositions.Count} saved positions");
                    trainVehicleComp.savedPositions.Keys.ToList().ForEach(map =>
                    {
                        var position = trainVehicleComp.savedPositions[map].position;
                        var rotation = trainVehicleComp.savedPositions[map].rotation;
                        sb.AppendLine($"Member {member.ThingID} has saved position {position} and rotation {rotation} for map {map.Index}");
                    });

                });
            });
            Log.Message(sb.ToString());
        }


        public override void ExposeData()
        {
            var mode = Scribe.mode;
            if (mode == LoadSaveMode.Saving)
            {
                DiscardNonSaveWorthyTrains();
            }
            Scribe_Collections.Look(ref trainConsists, nameof(trainConsists), LookMode.Deep);
            if (mode == LoadSaveMode.PostLoadInit)
            {
                if (trainConsists == null) trainConsists = new List<TrainConsist>();
            }
        }

        private void DiscardNonSaveWorthyTrains()
        {
            trainConsists.RemoveAll(t => t == null || !t.ShouldBeSaved);
        }
    }
}
