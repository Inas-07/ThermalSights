using ChainedPuzzles;
using EOSExt.SecuritySensor.Definition;
using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup;
using System.Linq;
using UnityEngine;
using GTFO.API.Extensions;


namespace EOSExt.SecuritySensor
{
    public class MovableSensor
    {
        public GameObject movableGO { get; private set; }

        public CP_BasicMovable movingComp { get; private set; }

        private GameObject graphicsGO => movableGO?.transform.GetChild(0).gameObject;

        public static MovableSensor Instantiate(SensorSettings sensorSetting)
        {
            if(sensorSetting.MovingPosition.Count < 1)
            {
                EOSLogger.Error($"SensorGroup.Instantiate: At least 1 moving position required to setup T-Sensor!");
                return null;
            }

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

            var StartPosition = sensorSetting.Position.ToVector3();
            var FirstPosition = sensorSetting.MovingPosition.First().ToVector3();
            var LastPosition = sensorSetting.MovingPosition.Last().ToVector3();

            var ScanPositions = sensorSetting.MovingPosition.ConvertAll(e => e.ToVector3()).AsEnumerable();
            if (!StartPosition.Equals(FirstPosition))
            {
                ScanPositions = ScanPositions.Prepend(StartPosition);
            }

            if (!StartPosition.Equals(LastPosition))
            {
                ScanPositions = ScanPositions.Append(StartPosition);
            }

            ms.movingComp.ScanPositions = ScanPositions.ToList().ToIl2Cpp();
            ms.movingComp.m_amountOfPositions = ScanPositions.Count() - 1; // I'm not pretty sure why, but this is actually needed
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
