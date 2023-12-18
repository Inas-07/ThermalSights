using System.Collections.Generic;
using ExtraObjectiveSetup.BaseClasses;
using LevelGeneration;
using ExtraObjectiveSetup;
using ExtraObjectiveSetup.Utils;
using GTFO.API;
using EOSExt.SecuritySensor.Definition;
using UnityEngine;
using EOSExt.SecuritySensor.Component;
using ChainedPuzzles;
using System;
using GameData;
using ExtraObjectiveSetup.ExtendedWardenEvents;
using GTFO.API.Utilities;
using UnityEngine.UI;
using TMPro;

namespace EOSExt.SecuritySensor
{
    public enum SensorEventType
    {
        ToggleSensorGroupState = 400,
    }

    public sealed class SecuritySensorManager : GenericExpeditionDefinitionManager<SensorGroupSettings>
    {
        public static SecuritySensorManager Current { get; private set; } = new();

        private List<(SensorGroupSettings settings, SensorGroup sensorGroup)> securitySensorGroups = new();

        private Dictionary<IntPtr, int> sensorGroupIndex = new();

        protected override string DEFINITION_NAME => "SecuritySensor";

        public void BuildSensorGroup(SensorGroupSettings sensorGroupSettings)
        {
            int groupIndex = securitySensorGroups.Count;
            var sg = SensorGroup.Instantiate(sensorGroupSettings, groupIndex);
            securitySensorGroups.Add((sensorGroupSettings, sg));
            
            foreach (var go in sg.SensorGOs)
            {
                sensorGroupIndex[go.Pointer] = groupIndex;
            }
        }

        internal void TriggerSensor(IntPtr pointer)
        {
            if(!sensorGroupIndex.ContainsKey(pointer))
            {
                EOSLogger.Error($"Triggering a sensor but doesn't find its corresponding sensor group! This should not happen!");
                return;
            }

            int groupIndex = sensorGroupIndex[pointer];
            if(groupIndex < 0 || groupIndex >= securitySensorGroups.Count)
            {
                EOSLogger.Error($"TriggerSensor: invalid SensorGroup index {groupIndex}");
                return;
            }

            securitySensorGroups[groupIndex]
                .settings
                .EventsOnTrigger
                .ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, e.Trigger, true));
            EOSLogger.Warning($"TriggerSensor: SensorGroup_{groupIndex} triggered");
        }

        private void BuildSecuritySensor()
        {
            if (!definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData)) return;
            definitions[RundownManager.ActiveExpedition.LevelLayoutData].Definitions.ForEach(BuildSensorGroup);
        }

        private void Clear()
        {
            securitySensorGroups.ForEach(builtSensorGroup => builtSensorGroup.sensorGroup.Destroy());

            securitySensorGroups.Clear();
            sensorGroupIndex.Clear();
        }

        private void ToggleSensorGroup(WardenObjectiveEventData e)
        {
            int groupIndex = e.Count;
            bool active = e.Enabled;
            if(groupIndex < 0 || groupIndex >= securitySensorGroups.Count)
            {
                EOSLogger.Error($"ToggleSensorGroup: invalid SensorGroup index {groupIndex}");
                return;
            }

            securitySensorGroups[groupIndex].sensorGroup.ChangeState(active ? ActiveState.ENABLED : ActiveState.DISABLED);
        }

        protected override void FileChanged(LiveEditEventArgs e)
        {
            base.FileChanged(e);

            //if (GameStateManager.CurrentStateName != eGameStateName.InLevel 
            //    || !definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData)) return;

            //var def = definitions[RundownManager.ActiveExpedition.LevelLayoutData].Definitions;
            //if (def.Count != securitySensors.Count) return;
            //for(int i = 0; i < securitySensors.Count; i++)
            //{
            //    var builtSettings = securitySensors[i].settings;
            //    if (def[i].SensorGroup.Count != builtSettings.SensorGroup.Count) return;   
            //}

            //for(int i = 0; i < securitySensors.Count; i++)
            //{
            //    var builtSensorGroup = securitySensors[i];
            //    builtSensorGroup.settings = def[i];
            //    for(int j = 0; j < builtSensorGroup.sensorGO.Count; j++)
            //    {
            //        var sensorSettings = builtSensorGroup.settings.SensorGroup[j];
            //        var position = sensorSettings.Position.ToVector3();
            //        if (position == Vector3.zeroVector) continue;

            //        var sensorGO = builtSensorGroup.sensorGO[j];

            //        CP_Bioscan_Graphics graphics = sensorGO.GetComponent<CP_Bioscan_Graphics>();
            //        //position.y += graphics.m_height / 2;
            //        sensorGO.transform.SetPositionAndRotation(position, Quaternion.identityQuaternion);

            //        graphics.SetColor(sensorSettings.Color.toColor());
            //        graphics.m_radius = sensorSettings.Radius;
            //        graphics.Setup();
            //        graphics.SetVisible(true);
            //    }
            //}

            //EOSLogger.Debug($"Live-Updated SensorSettings");
        }

        private SecuritySensorManager() : base()
        {
            LevelAPI.OnBuildStart += () => { Clear(); BuildSecuritySensor(); };
            LevelAPI.OnLevelCleanup += Clear;

            EOSWardenEventManager.Current.AddEventDefinition(SensorEventType.ToggleSensorGroupState.ToString(), (int)SensorEventType.ToggleSensorGroupState, ToggleSensorGroup);
        }

        static SecuritySensorManager() { }
    }
}
