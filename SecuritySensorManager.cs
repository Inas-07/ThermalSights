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

namespace EOSExt.SecuritySensor
{
    public enum SensorEventType
    {
        ToggleSensorGroupState = 400,
    }

    public sealed class SecuritySensorManager : GenericExpeditionDefinitionManager<SensorGroupSettings>
    {
        public static SecuritySensorManager Current { get; private set; } = new();

        private List<(SensorGroupSettings settings, List<GameObject> sensorGO)> securitySensors = new();

        private Dictionary<IntPtr, int> sensorGroupIndex = new();

        protected override string DEFINITION_NAME => "SecuritySensor";

        public void BuildSensorGroup(SensorGroupSettings sensorGroupSettings)
        {
            List<GameObject> GOSensorGroup = new();
            int groupIndex = securitySensors.Count;
            securitySensors.Add((sensorGroupSettings, GOSensorGroup));

            foreach(var sensorSetting in sensorGroupSettings.SensorGroup) 
            {
                var position = sensorSetting.Position.ToVector3(); 
                if (position == Vector3.zeroVector) continue;

                GameObject sensorGO = UnityEngine.Object.Instantiate(Assets.SecuritySensor);
                CP_Bioscan_Graphics graphics = sensorGO.GetComponent<CP_Bioscan_Graphics>();
                graphics.m_radius = sensorSetting.Radius;
                //position.y += graphics.m_height / 2;

                sensorGO.transform.SetPositionAndRotation(position, Quaternion.identityQuaternion);
                GOSensorGroup.Add(sensorGO);
                sensorGroupIndex[sensorGO.Pointer] = groupIndex;

                graphics.Setup();
                graphics.SetColor(sensorGroupSettings.Color.toColor());
                //graphics.m_zoneMaterialInstance.
                graphics.SetVisible(true);

                var sensorCollider = sensorGO.AddComponent<SensorCollider>();
                sensorCollider.Setup(sensorSetting);
                sensorGO.SetActive(true);
            }
        }

        internal void TriggerSensor(IntPtr pointer)
        {
            int groupIndex = sensorGroupIndex[pointer];
            if(groupIndex < 0 || groupIndex >= securitySensors.Count)
            {
                EOSLogger.Error($"TriggerSensor: invalid SensorGroup index {groupIndex}");
                return;
            }

            securitySensors[groupIndex]
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
            securitySensors.Clear();
            sensorGroupIndex.Clear();
        }

        private void ToggleSensorGroup(WardenObjectiveEventData e)
        {
            int groupIndex = e.Count;
            bool active = e.Enabled;
            if(groupIndex < 0 || groupIndex >= securitySensors.Count)
            {
                EOSLogger.Error($"ToggleSensorGroup: invalid SensorGroup index {groupIndex}");
                return;
            }

            securitySensors[groupIndex].sensorGO.ForEach(go => go.SetActive(active));
            EOSLogger.Warning($"ToggleSensorGroup: SensorGroup_{groupIndex} toggled state to {active}");
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
            LevelAPI.OnBuildStart += Clear;
            LevelAPI.OnLevelCleanup += Clear;

            LevelAPI.OnBuildDone += BuildSecuritySensor;
            //BatchBuildManager.Current.Add_OnBatchDone(LG_Factory.BatchName., BuildSecuritySensor);

            EOSWardenEventManager.Current.AddEventDefinition(SensorEventType.ToggleSensorGroupState.ToString(), (int)SensorEventType.ToggleSensorGroupState, ToggleSensorGroup);
        }

        static SecuritySensorManager() { }
    }
}
