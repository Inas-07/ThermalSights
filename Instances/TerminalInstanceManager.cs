using ExtraObjectiveSetup.BaseClasses;
using GameData;
using LevelGeneration;

namespace ExtraObjectiveSetup.Instances
{
    public sealed class TerminalInstanceManager: InstanceManager<LG_ComputerTerminal>
    {
        public static readonly TerminalInstanceManager Current = new();

        public override (eDimensionIndex, LG_LayerType, eLocalZoneIndex) GetGlobalZoneIndex(LG_ComputerTerminal instance) => (instance.SpawnNode.m_dimension.DimensionIndex, instance.SpawnNode.LayerType, instance.SpawnNode.m_zone.LocalIndex);
        
        private TerminalInstanceManager() {}
    
        static TerminalInstanceManager() { }
    }
}
