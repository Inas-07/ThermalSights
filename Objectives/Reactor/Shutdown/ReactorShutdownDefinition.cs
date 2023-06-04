using ChainedPuzzles;
using ExtraObjectiveSetup.BaseClasses;
using GameData;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExtraObjectiveSetup.Objectives.Reactor.Shutdown
{
    public class ReactorShutdownDefinition: BaseReactorDefinition
    {
        public bool PutVerificationCodeOnTerminal { get; set; } = false;

        public BaseInstanceDefinition VerificationCodeTerminal { get; set; } = new();

        public uint ChainedPuzzleOnVerification { get; set; } = 0u;

        [JsonIgnore]
        public ChainedPuzzleInstance ChainedPuzzleOnVerificationInstance { get; set; } = null;

        public List<WardenObjectiveEventData> EventsOnVerification { get; set; } = new();  

        public List<WardenObjectiveEventData> EventsOnComplete { get; set; } = new();
    }
}
