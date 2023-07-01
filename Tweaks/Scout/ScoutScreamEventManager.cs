using ExtraObjectiveSetup.BaseClasses;
using GameData;
using LevelGeneration;

namespace ExtraObjectiveSetup.Tweaks.Scout
{
    internal class ScoutScreamEventManager: ZoneDefinitionManager<EventsOnZoneScoutScream>
    {
        public static ScoutScreamEventManager Current = new();

        protected override string DEFINITION_NAME => "EventsOnScoutScream";

        private ScoutScreamEventManager() { }

        static ScoutScreamEventManager()
        {

        }
    }
}
