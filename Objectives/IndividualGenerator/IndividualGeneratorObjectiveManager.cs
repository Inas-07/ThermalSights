using ExtraObjectiveSetup.BaseClasses;

namespace ExtraObjectiveSetup.Objectives.IndividualGenerator
{
    internal sealed class IndividualGeneratorObjectiveManager : InstanceDefinitionManager<IndividualGenerator>
    {
        public static readonly IndividualGeneratorObjectiveManager Current = new();

        protected override string DEFINITION_NAME { get; } = "IndividualGenerator";

        private IndividualGeneratorObjectiveManager() : base() { }

        static IndividualGeneratorObjectiveManager() { }
    }
}
