using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ExtraObjectiveSetup.ObjectiveDefinition;
using ExtraObjectiveSetup.JSON;

namespace ExtraObjectiveSetup
{
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(MTFOUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(MTFOPartialDataUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(AUTHOR + "." + PLUGIN_NAME, PLUGIN_NAME, VERSION)]
    
    public class EntryPoint: BasePlugin
    {
        public const string AUTHOR = "Inas07";
        public const string PLUGIN_NAME = "ExtraObjectiveSetup";
        public const string VERSION = "1.0.0";

        private Harmony m_Harmony;
        
        public override void Load()
        {
            m_Harmony = new Harmony("ExtraObjectiveSetup");
            m_Harmony.PatchAll();

            ObjectiveDefinitionManager<BaseDefinition>.Initialize();
        }
    }
}

