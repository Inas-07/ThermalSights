using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.Tweaks.Scout;
using GameData;
using GTFO.API.Utilities;
using LevelGeneration;

namespace ExtraObjectiveSetup.Tweaks.BossEvents
{
    internal class BossDeathEventManager : ZoneDefinitionManager<EventsOnZoneBossDeath>
    {
        public static BossDeathEventManager Current = new();

        protected override string DEFINITION_NAME => "EventsOnBossDeath";

        protected override void FileChanged(LiveEditEventArgs e)
        {
            base.FileChanged(e);

        }

        private BossDeathEventManager() { }

        static BossDeathEventManager()
        {

        }
    }
}
