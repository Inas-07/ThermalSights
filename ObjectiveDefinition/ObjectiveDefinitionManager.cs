using System;
using System.IO;
using GTFO.API.Utilities;
using System.Collections.Generic;
using MTFO.API;
using ExtraObjectiveSetup.Utils;
using LevelGeneration;
using GameData;
using System.Linq;
using ExtraObjectiveSetup.ObjectiveDefinition.TerminalUplink;
using ExtraObjectiveSetup.JSON;

namespace ExtraObjectiveSetup.ObjectiveDefinition
{
    public abstract class ObjectiveDefinitionManager<T> where T : BaseDefinition, new()
    {
        public static string PLUGIN_CUSTOM_FOLDER { get; private set; } = Path.Combine(MTFOPathAPI.CustomPath, "ExtraObjectiveSetup");

        protected Dictionary<uint, ObjectiveDefinitionsForLevel<T>> definitions = new();

        protected readonly LiveEditListener liveEditListener;

        protected abstract string DEFINITION_PATH { get; }

        /// <summary>
        /// Sort definitions by dimension index, layer type, local index and instance index.
        /// </summary>
        protected void Sort(ObjectiveDefinitionsForLevel<T> levelDefs)
        {
            levelDefs.Definitions.Sort((u1, u2) =>
            {
                if (u1.DimensionIndex != u2.DimensionIndex) return (int)u1.DimensionIndex < (int)u2.DimensionIndex ? -1 : 1;
                if (u1.LayerType != u2.LayerType) return (int)u1.LayerType < (int)u2.LayerType ? -1 : 1;
                if (u1.LocalIndex != u2.LocalIndex) return (int)u1.LocalIndex < (int)u2.LocalIndex ? -1 : 1;
                if (u1.InstanceIndex != u2.InstanceIndex) return u1.InstanceIndex < u2.InstanceIndex ? -1 : -1;
                return 0;
            });
        }

        /// <summary>
        /// Add objective definitions for a level.
        /// </summary>
        protected virtual void AddDefinitions(ObjectiveDefinitionsForLevel<T> definitions)
        {
            if (definitions == null) return;

            if (this.definitions.ContainsKey(definitions.MainLevelLayout))
            {
                EOSLogger.Warning("Replaced MainLevelLayout {0}", definitions.MainLevelLayout);
            }
            this.definitions[definitions.MainLevelLayout] = definitions;
        }

        private void FileChanged(LiveEditEventArgs e)
        {
            EOSLogger.Warning($"LiveEdit File Changed: {e.FullPath}");
            LiveEdit.TryReadFileContent(e.FullPath, (content) =>
            {
                ObjectiveDefinitionsForLevel<T> conf = Json.Deserialize<ObjectiveDefinitionsForLevel<T>>(content);
                AddDefinitions(conf);
            });
        }

        public virtual List<T> GetDefinitionsForLevel(uint MainLevelLayout) => definitions.ContainsKey(MainLevelLayout) ? definitions[MainLevelLayout].Definitions : null;

        public T GetDefinition((eDimensionIndex, LG_LayerType, eLocalZoneIndex) globalIndex, uint instanceIndex) => GetDefinition(globalIndex.Item1, globalIndex.Item2, globalIndex.Item3, instanceIndex);

        public virtual T GetDefinition(eDimensionIndex dimensionIndex, LG_LayerType layerType, eLocalZoneIndex localIndex, uint instanceIndex)
        {
            var definitionsForLevel = definitions[RundownManager.ActiveExpedition.LevelLayoutData];

            return definitionsForLevel.Definitions.First(def => def.DimensionIndex == dimensionIndex && def.LayerType == layerType && def.LocalIndex == localIndex && def.InstanceIndex == instanceIndex);
        }

        public virtual void Init() { }

        protected ObjectiveDefinitionManager()
        {
            if (!Directory.Exists(DEFINITION_PATH))
            {
                Directory.CreateDirectory(DEFINITION_PATH);
                var file = File.CreateText(Path.Combine(DEFINITION_PATH, "Template.json"));
                file.WriteLine(Json.Serialize(new ObjectiveDefinitionsForLevel<T>()));
                file.Flush();
                file.Close();
            }

            foreach (string confFile in Directory.EnumerateFiles(DEFINITION_PATH, "*.json", SearchOption.AllDirectories))
            {
                ObjectiveDefinitionsForLevel<T> conf;
                Json.Load(confFile, out conf);

                AddDefinitions(conf);
            }

            liveEditListener = LiveEdit.CreateListener(DEFINITION_PATH, "*.json", true);
            liveEditListener.FileChanged += FileChanged;
        }

        internal static void Initialize()
        {
            if (!Directory.Exists(PLUGIN_CUSTOM_FOLDER))
            {
                Directory.CreateDirectory(PLUGIN_CUSTOM_FOLDER);
            }

            // explicitly call to all inherited classes, which defines chained puzzle creation order if any
            UplinkManager.Current.Init();
        }

        static ObjectiveDefinitionManager() {}
    }
}
