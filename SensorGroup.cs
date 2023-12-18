using System.Collections.Generic;
using EOSExt.SecuritySensor.Component;
using EOSExt.SecuritySensor.Definition;
using ExtraObjectiveSetup;
using ExtraObjectiveSetup.Utils;
using FloLib.Networks.Replications;
using UnityEngine;

namespace EOSExt.SecuritySensor
{
    public enum ActiveState
    {
        DISABLED,
        ENABLED,
    }

    public struct SensorGroupState
    {
        public ActiveState status;

        public SensorGroupState() { }

        public SensorGroupState(SensorGroupState o) { status = o.status; }

        public SensorGroupState(ActiveState status) {  this.status = status; }
    }

    internal class SensorGroup
    {
        private List<GameObject> GOSensorGroup = new();

        private int sensorGroupIndex;

        public StateReplicator<SensorGroupState> StateReplicator { get; private set; }

        public IEnumerable<GameObject> SensorGOs => GOSensorGroup;

        public void ChangeState(ActiveState status) => StateReplicator.SetState(new() { status = status });

        private void OnStateChanged(SensorGroupState oldState, SensorGroupState newState, bool isRecall)
        {
            if (oldState.status == newState.status) return;
            EOSLogger.Debug($"SecuritySensorGroup_{sensorGroupIndex} changed to state {newState.status}");
            bool active = newState.status == ActiveState.ENABLED;
            GOSensorGroup.ForEach(sensorGO => sensorGO.SetActive(active));
        }

        public static SensorGroup Instantiate(SensorGroupSettings sensorGroupSettings, int sensorGroupIndex)
        {
            SensorGroup sg = new SensorGroup();
            sg.sensorGroupIndex = sensorGroupIndex;

            foreach (var sensorSetting in sensorGroupSettings.SensorGroup)
            {
                var position = sensorSetting.Position.ToVector3();
                if (position == Vector3.zeroVector) continue;

                GameObject sensorGO = Object.Instantiate(Assets.SecuritySensor);

                sensorGO.transform.SetPositionAndRotation(position, Quaternion.identityQuaternion);
                sg.GOSensorGroup.Add(sensorGO);
                //sensorGroupIndex[sensorGO.Pointer] = groupIndex;

                float height = 0.6f / 3.7f;
                sensorGO.transform.localScale = new Vector3(sensorSetting.Radius, sensorSetting.Radius, sensorSetting.Radius);
                sensorGO.transform.localPosition += Vector3.up * height;

                var sensorCollider = sensorGO.AddComponent<SensorCollider>();
                sensorCollider.Setup(sensorSetting);
                sensorGO.SetActive(true);
            }

            uint allotedID = EOSNetworking.AllotReplicatorID();
            if (allotedID == EOSNetworking.INVALID_ID)
            {
                EOSLogger.Error($"SensorGroup.Instantiate: replicator ID depleted, cannot create StateReplicator...");
            }
            else
            {
                sg.StateReplicator = StateReplicator<SensorGroupState>.Create(allotedID, new() { status = ActiveState.ENABLED }, LifeTimeType.Level);
                sg.StateReplicator.OnStateChanged += sg.OnStateChanged;
            }

            return sg;
        }

        public void Destroy()
        {
            GOSensorGroup.ForEach(GameObject.Destroy);
            GOSensorGroup.Clear();
        }

        private SensorGroup() { }
    }
}
