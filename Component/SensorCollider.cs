using EOSExt.SecuritySensor.Definition;
using LevelGeneration;
using Player;
using SNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EOSExt.SecuritySensor.Component
{
    public class SensorCollider: MonoBehaviour
    {
        public static float CHECK_INTERVAL { get; } = 0.1f;

        private float nextCheckTime = float.NaN;

        private SensorSettings sensorSettings;

        private Vector3 Position;

        private int last_playersInSensor = 0;

        internal void Setup(SensorSettings sensorSettings)
        {
            this.sensorSettings = sensorSettings;
            this.Position = sensorSettings.Position.ToVector3();
        }

        void Update()
        {
            if (GameStateManager.CurrentStateName != eGameStateName.InLevel) return;

            if (!float.IsNaN(nextCheckTime) && Clock.Time < nextCheckTime) return;

            nextCheckTime = Clock.Time + CHECK_INTERVAL;
            int current_playersInSensor = 0; 
            foreach (var player in PlayerManager.PlayerAgentsInLevel)
            {
                if (player.Owner.IsBot || !player.Alive) continue;

                if((this.Position - player.Position).magnitude < sensorSettings.Radius)
                {
                    current_playersInSensor++;
                }
            }

            if (current_playersInSensor > last_playersInSensor)
            {
                SecuritySensorManager.Current.TriggerSensor(gameObject.Pointer);
            }

            last_playersInSensor = current_playersInSensor;
        }
    }
}
