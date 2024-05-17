using BepInEx;
using BepInEx.Unity.IL2CPP;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.JSON;
using GTFO.API;
using HarmonyLib;

namespace EOSExt.SecDoor
{
    [BepInDependency("dev.gtfomodding.gtfo-api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("GTFO.FloLib", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("Inas.ExtraObjectiveSetup", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(MTFOPartialDataUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(InjectLibUtil.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(AUTHOR + "." + PLUGIN_NAME, PLUGIN_NAME, VERSION)]
    
    public class EntryPoint: BasePlugin
    {
        public const string AUTHOR = "Inas";
        public const string PLUGIN_NAME = "EOSExt.SecDoor";
        public const string VERSION = "1.0.0";

        private Harmony m_Harmony;

        public override void Load()
        {
            SetupManagers();

            m_Harmony = new Harmony("EOSExt.SecDoor");
            m_Harmony.PatchAll();

            EOSLogger.Log("ExtraObjectiveSetup.SecDoor loaded.");
        }

        /// <summary>
        /// Explicitly invoke Init() to all managers to eager-load, which in the meantime defines chained puzzle creation order if any
        /// </summary>
        private void SetupManagers()
        {
            SecDoorIntTextOverrideManager.Current.Init();
        }
    }
}

