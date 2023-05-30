using GameData;
using LevelGeneration;

namespace ExtraObjectiveSetup.ObjectiveInstance
{
    public sealed class TerminalManager: InstanceManager<LG_ComputerTerminal>
    {
        public static readonly TerminalManager Current = new TerminalManager();

        public uint Register(LG_ComputerTerminal terminal)
        {
            if (terminal == null) return INVALID_INSTANCE_INDEX;
            return this.Register(GetGlobalIndex(terminal), terminal);
        }

        public override (eDimensionIndex, LG_LayerType, eLocalZoneIndex) GetGlobalIndex(LG_ComputerTerminal instance) => (instance.SpawnNode.m_dimension.DimensionIndex, instance.SpawnNode.LayerType, instance.SpawnNode.m_zone.LocalIndex);
        
        private TerminalManager() {}
    
        static TerminalManager() { }
    }
}
