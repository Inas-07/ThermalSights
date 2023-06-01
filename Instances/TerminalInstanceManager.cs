using ExtraObjectiveSetup.BaseClasses;
using GameData;
using GTFO.API;
using LevelGeneration;
using System;
using System.Collections.Generic;

namespace ExtraObjectiveSetup.Instances
{
    public sealed class TerminalInstanceManager: InstanceManager<LG_ComputerTerminal>
    {
        public static readonly TerminalInstanceManager Current = new();

        private List<LG_ComputerTerminal> ReactorTerminals = new();

        public override (eDimensionIndex, LG_LayerType, eLocalZoneIndex) GetGlobalZoneIndex(LG_ComputerTerminal instance)
        {
            if(instance.SpawnNode == null)
            {
                if(instance.ConnectedReactor == null)
                {
                    throw new ArgumentException("LG_ComputerTerminal instance SpawnNode is null! This only happen if the instance is a reactor terminal and is yet setup-ed.\nRegister this LG_ComputerTerminal instance OnBuildDone instead!");
                }
                else
                {
                    var node = instance.ConnectedReactor.SpawnNode;
                    return (node.m_dimension.DimensionIndex, node.LayerType, node.m_zone.LocalIndex);
                }
            }

            return (instance.SpawnNode.m_dimension.DimensionIndex, instance.SpawnNode.LayerType, instance.SpawnNode.m_zone.LocalIndex);
        }

        public override uint Register(LG_ComputerTerminal instance)
        {
            if (instance.SpawnNode == null)
            {
                ReactorTerminals.Add(instance);
                return INVALID_INSTANCE_INDEX;
            }

            else return Register(GetGlobalZoneIndex(instance), instance);
        }

        private void RegisterReactorTerminals()
        {
            ReactorTerminals.ForEach(t => Register(GetGlobalZoneIndex(t), t));
        }

        private void Clear()
        {
            ReactorTerminals.Clear();
        }

        private TerminalInstanceManager() 
        {
            LevelAPI.OnBuildDone += RegisterReactorTerminals;
            LevelAPI.OnLevelCleanup += Clear;
        }
    
        static TerminalInstanceManager() { }
    }
}
