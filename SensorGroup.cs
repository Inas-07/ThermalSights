using System.Collections.Generic;
using System.Linq;
using ChainedPuzzles;
using EOSExt.SecuritySensor.Component;
using EOSExt.SecuritySensor.Definition;
using ExtraObjectiveSetup;
using ExtraObjectiveSetup.Utils;
using FloLib.Networks.Replications;
using GTFO.API.Extensions;
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

        private List<CP_BasicMovable> MovableComps = new();

        public int sensorGroupIndex { get; private set; }

        public StateReplicator<SensorGroupState> StateReplicator { get; private set; }

        public IEnumerable<GameObject> SensorGOs => GOSensorGroup;

        public IEnumerable<CP_BasicMovable> MovableSensorCPs => MovableComps;

        public void ChangeState(ActiveState status)
        {

            StateReplicator.SetState(new() { status = status });
        }
        private void OnStateChanged(SensorGroupState oldState, SensorGroupState newState, bool isRecall)
        {
            if (!isRecall) return;
            EOSLogger.Debug($"SecuritySensorGroup_{sensorGroupIndex} changed to state {newState.status}");
            switch (newState.status)
            {
                case ActiveState.ENABLED:
                    GOSensorGroup.ForEach(sensorGO => sensorGO.SetActiveRecursively(true));
                    ResumeMovingMovables();
                    break;
                case ActiveState.DISABLED:
                    PauseMovingMovables();
                    GOSensorGroup.ForEach(sensorGO => sensorGO.SetActiveRecursively(false));
                    break;
            }
        }

        public static SensorGroup Instantiate(SensorGroupSettings sensorGroupSettings, int sensorGroupIndex)
        {
            SensorGroup sg = new SensorGroup();
            sg.sensorGroupIndex = sensorGroupIndex;

            foreach (var sensorSetting in sensorGroupSettings.SensorGroup)
            {
                var position = sensorSetting.Position.ToVector3();
                if (position == Vector3.zeroVector) continue;

                GameObject sensorGO = null; 
                switch (sensorSetting.SensorType)
                {
                    case SensorType.BASIC:
                        sensorGO = Object.Instantiate(Assets.CircleSensor);
                        break;
                    case SensorType.MOVABLE:
                        sensorGO = Object.Instantiate(Assets.MovableSensor);
                        var movingComp = sensorGO.GetComponent<CP_BasicMovable>();
                        movingComp.Setup();

                        var ScanPositions = sensorSetting.MovingPosition.Prepend(sensorSetting.Position).ToList().ConvertAll(vec3 => vec3.ToVector3()).ToIl2Cpp();
                        movingComp.ScanPositions = ScanPositions;
                        movingComp.m_amountOfPositions = sensorSetting.MovingPosition.Count;
                        if(sensorSetting.MovingSpeedMulti > 0)
                        {
                            movingComp.m_movementSpeed *= sensorSetting.MovingSpeedMulti;
                        }
                        sg.MovableComps.Add(movingComp); 
                        break;
                    default:
                        EOSLogger.Error($"Unsupported SensorType {sensorSetting.SensorType}, skipped");
                        continue;
                }

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

        public void StartMovingMovables() => MovableComps.ForEach(movable => movable.StartMoving());

        public void PauseMovingMovables() => MovableComps.ForEach(movable => movable.StopMoving());

        public void ResumeMovingMovables() => MovableComps.ForEach(movable => movable.StartMoving());

        public void Destroy()
        {
            GOSensorGroup.ForEach(GameObject.Destroy);
            GOSensorGroup.Clear();
            MovableComps.Clear();
        }

        private SensorGroup() { }
    }
}
