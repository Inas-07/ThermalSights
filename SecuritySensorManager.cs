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
using SNetwork;

namespace EOSExt.SecuritySensor
{
    public enum SensorEventType
    {
        ToggleSensorGroupState = 400,
    }

    public sealed class SecuritySensorManager : GenericExpeditionDefinitionManager<SensorGroupSettings>
    {
        public static SecuritySensorManager Current { get; private set; } = new();

        private List<SensorGroup> sensorGroups = new();

        private Dictionary<IntPtr, int> sensorGroupIndex = new();

        protected override string DEFINITION_NAME => "SecuritySensor";

        public void BuildSensorGroup(SensorGroupSettings sensorGroupSettings)
        {
            int groupIndex = sensorGroups.Count;
            var sg = SensorGroup.Instantiate(sensorGroupSettings, groupIndex);
            sensorGroups.Add(sg);
            
            foreach (var go in sg.BasicSensors)
            {
                sensorGroupIndex[go.Pointer] = groupIndex;
            }

            foreach (var m in sg.MovableSensors)
            {
                sensorGroupIndex[m.movableGO.Pointer] = groupIndex;
            }

            EOSLogger.Debug($"SensorGroup_{groupIndex} built");
        }

        internal void TriggerSensor(IntPtr pointer)
        {
            if(!sensorGroupIndex.ContainsKey(pointer))
            {
                EOSLogger.Error($"Triggering a sensor but doesn't find its corresponding sensor group! This should not happen!");
                return;
            }

            int groupIndex = sensorGroupIndex[pointer];
            if(groupIndex < 0 || groupIndex >= sensorGroups.Count)
            {
                EOSLogger.Error($"TriggerSensor: invalid SensorGroup index {groupIndex}");
                return;
            }

            var sg = sensorGroups[groupIndex];
            //// MovableSensor would trigger `SensorCollider` cuz its game object is not (and cannot) set to inactive
            //// So do some special case handling here
            //if (sg.sensorGroup.StateReplicator.State.status != ActiveState.ENABLED)
            //{
            //    return; 
            //}

            sg.Settings
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
            sensorGroups.ForEach(sg => sg.Destroy());
            sensorGroups.Clear();
            sensorGroupIndex.Clear();
        }

        private void ToggleSensorGroup(WardenObjectiveEventData e)
        {
            int groupIndex = e.Count;
            bool active = e.Enabled;
            if(groupIndex < 0 || groupIndex >= sensorGroups.Count)
            {
                EOSLogger.Error($"ToggleSensorGroup: invalid SensorGroup index {groupIndex}");
                return;
            }

            sensorGroups[groupIndex].ChangeToState(active ? ActiveState.ENABLED : ActiveState.DISABLED);
        }

        private SecuritySensorManager() : base()
        {
            LevelAPI.OnBuildStart += () => { Clear(); BuildSecuritySensor(); };
            LevelAPI.OnLevelCleanup += Clear;

            EventAPI.OnExpeditionStarted += () => {
                sensorGroups.ForEach(sg => sg.StartMovingMovables());
            };

            EOSWardenEventManager.Current.AddEventDefinition(SensorEventType.ToggleSensorGroupState.ToString(), (int)SensorEventType.ToggleSensorGroupState, ToggleSensorGroup);
        }

        static SecuritySensorManager() { }
    }
}
