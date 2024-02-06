using ChainedPuzzles;
using EOSExt.SecuritySensor.Definition;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup;
using FloLib.Networks.Replications;
using SNetwork;
using System.Linq;
using UnityEngine;
using GTFO.API.Extensions;

namespace EOSExt.SecuritySensor
{
    internal class MovableSensor
    {
        public GameObject movableGO { get; private set; }

        public CP_BasicMovable movingComp { get; private set; }

        private GameObject graphicsGO => movableGO?.transform.GetChild(0).gameObject;

        public static MovableSensor Instantiate(SensorSettings sensorSetting)
        {
            uint movableRID = EOSNetworking.AllotReplicatorID();
            if (movableRID == EOSNetworking.INVALID_ID)
            {
                EOSLogger.Error($"SensorGroup.Instantiate: replicator ID depleted, cannot create StateReplicator...");
                return null;
            }

            MovableSensor ms = new();

            ms.movableGO = Object.Instantiate(Assets.MovableSensor);
            ms.movingComp = ms.movableGO.GetComponent<CP_BasicMovable>();
            ms.movingComp.Setup();

            var ScanPositions = sensorSetting.MovingPosition
                .Prepend(sensorSetting.Position)
                .Append(sensorSetting.Position)
                .ToList().ConvertAll(vec3 => vec3.ToVector3()).ToIl2Cpp();
            ms.movingComp.ScanPositions = ScanPositions;
            ms.movingComp.m_amountOfPositions = sensorSetting.MovingPosition.Count;
            if (sensorSetting.MovingSpeedMulti > 0)
            {
                ms.movingComp.m_movementSpeed *= sensorSetting.MovingSpeedMulti;
            }

            return ms;
        }

        public void StartMoving()
        {
            graphicsGO.SetActive(true);
            movingComp.SyncUpdate();
            movingComp.StartMoving();
        }

        public void ResumeMoving()
        {
            graphicsGO.SetActive(true);
            movingComp.ResumeMovement();
        }

        public void StopMoving()
        {
            movingComp.StopMoving();
            graphicsGO.SetActive(false);
        }

        public void PauseMoving()
        {
            movingComp.PauseMovement();
            graphicsGO.SetActive(false);
        }

        public void Destroy()
        {
            Object.Destroy(movableGO);
            movableGO = null;
            movingComp = null;
        }

        private MovableSensor()
        {

        }
    }
}
