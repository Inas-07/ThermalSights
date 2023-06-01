using GameData;
using System.Collections.Generic;
using ExtraObjectiveSetup.BaseClasses;

namespace ExtraObjectiveSetup.Objectives.GeneratorCluster
{
    public class GeneratorClusterDefinition : BaseInstanceDefinition
    {
        public uint NumberOfGenerators { get; set; } = 0;

        // OnActivateOnSolveItem is enabled by default
        public List<List<WardenObjectiveEventData>> EventsOnInsertCell { get; set; } = new() { new() };

        public uint EndSequenceChainedPuzzle { get; set; } = 0u;

        public List<WardenObjectiveEventData> EventsOnEndSequenceChainedPuzzleComplete { get; set; } = new() { };
    }
}
