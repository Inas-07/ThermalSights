﻿using System.Collections.Generic;
using ExtraObjectiveSetup.BaseClasses;
using LevelGeneration;
using ExtraObjectiveSetup;
using ExtraObjectiveSetup.Utils;
using SecDoorTerminalInterface;
using GameData;
using GTFO.API.Extensions;
using Localization;
using GTFO.API;
using ExtraObjectiveSetup.ExtendedWardenEvents;
using EOSExt.SecurityDoorTerminal.Definition;

namespace EOSExt.SecurityDoorTerminal
{
    public sealed partial class SecurityDoorTerminalManager : ZoneDefinitionManager<SecurityDoorTerminalDefinition>
    {
        public static SecurityDoorTerminalManager Current { get; private set; } = new();

        private List<(SecDoorTerminal sdt, SecurityDoorTerminalDefinition def)> levelSDTs = new();

        protected override string DEFINITION_NAME => "SecDoorTerminal";

        private SecDoorTerminal BuildSDT_Instantiation(SecurityDoorTerminalDefinition def)
        {
            var (dimensionIndex, layer, localIndex) = def.GlobalZoneIndexTuple();

            if (!Builder.CurrentFloor.TryGetZoneByLocalIndex(dimensionIndex, layer, localIndex, out var targetZone) || targetZone == null)
            {
                EOSLogger.Error($"SecDoorTerminal: Cannot find target zone {def.GlobalZoneIndexTuple()}");
                return null;
            }

            if (!TryGetZoneEntranceSecDoor(targetZone, out var door) || door == null)
            {
                EOSLogger.Error($"SecDoorTerminal: Cannot find spawned sec-door for zone {def.GlobalZoneIndexTuple()}");
                return null;
            }

            var sdt = SecDoorTerminal.Place(door, new TerminalStartStateData()
            {
                StartingState = TERM_State.Sleeping
            });

            if (sdt == null)
            {
                EOSLogger.Error("Build failed: Can only attach SDT to regular security door");
                return null;
            }

            targetZone.m_sourceGate.m_linksFrom.m_zone.TerminalsSpawnedInZone.Add(sdt.ComputerTerminal);
            sdt.BioscanScanSolvedBehaviour = CPSolvedBehaviour.AddOpenCommand;
            def.LocalLogFiles.ForEach(log => sdt.ComputerTerminal.AddLocalLog(log, true));

            // === initial state configuration === 
            switch (sdt.LinkedDoor.m_sync.GetCurrentSyncState().status)
            {
                case eDoorStatus.Closed_LockedWithKeyItem:
                    if (!def.StateSettings.LockedStateSetting.AccessibleWhenLocked) // SDT default behaviour, won't config
                    {
                        break; 
                    }

                    // after picking up the key, SDT must be inactive to be able to insert the key
                    sdt.LinkedDoorLocks.m_gateKeyItemNeeded.keyPickupCore.m_interact.add_OnPickedUpByPlayer(new System.Action<Player.PlayerAgent>((p) => {
                        sdt.SetTerminalActive(false);
                    }));

                    sdt.SetTerminalActive(def.StateSettings.LockedStateSetting.AccessibleWhenLocked /*true*/ );
                    var hintText = new List<string>() {
                        string.Format($"<color=orange>{Text.Get(849)}</color>", sdt.LinkedDoorLocks.m_gateKeyItemNeeded.PublicName)
                    };

                    sdt.ComputerTerminal.m_command.AddOutput(hintText.ToIl2Cpp());
                    break;

                case eDoorStatus.Closed_LockedWithPowerGenerator:
                    sdt.SetTerminalActive(def.StateSettings.LockedStateSetting.AccessibleWhenLocked);
                    if (def.StateSettings.LockedStateSetting.AccessibleWhenLocked)
                    {
                        var _hintText = new List<string>() {
                            string.Format($"<color=orange>{Text.Get(842)}</color>", sdt.LinkedDoorLocks.m_powerGeneratorNeeded.PublicName)
                        };

                        sdt.ComputerTerminal.m_command.AddOutput(_hintText.ToIl2Cpp());
                    }
                    break;

                case eDoorStatus.Closed_LockedWithBulkheadDC:
                    sdt.SetTerminalActive(def.StateSettings.LockedStateSetting.AccessibleWhenLocked);
                    if (def.StateSettings.LockedStateSetting.AccessibleWhenLocked)
                    {
                        var _hintText = new List<string>() {
                            string.Format($"<color=orange>{Text.Get(842)}</color>", sdt.LinkedDoorLocks.m_powerGeneratorNeeded.PublicName)
                        };

                        sdt.ComputerTerminal.m_command.AddOutput(_hintText.ToIl2Cpp());
                    }
                    break;

                case eDoorStatus.Closed_LockedWithNoKey:
                    sdt.SetTerminalActive(def.StateSettings.LockedStateSetting.AccessibleWhenLocked);
                    if (def.StateSettings.LockedStateSetting.AccessibleWhenLocked)
                    {
                        var _hintText = new List<string>() {
                            string.Format($"<color=orange>{Text.Get(843)}</color>", sdt.LinkedDoorLocks.m_bulkheadDCNeeded.PublicName)
                        };

                        sdt.ComputerTerminal.m_command.AddOutput(_hintText.ToIl2Cpp());
                    }
                    break;

                case eDoorStatus.Closed:
                case eDoorStatus.Closed_LockedWithChainedPuzzle:
                case eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm:
                case eDoorStatus.Unlocked:
                    AddOverrideCommandWithAlarmText(sdt); // already unlocked, so add override command 
                    sdt.SetTerminalActive(def.StateSettings.LockedStateSetting.AccessibleWhenUnlocked);
                    break;
            }

            // === state replicator config === 
            sdt.LinkedDoor.m_sync.add_OnDoorStateChange(new System.Action<pDoorState, bool>((state, isDropin) => {
                switch (state.status)
                {
                    case eDoorStatus.Closed_LockedWithKeyItem:
                    case eDoorStatus.Closed_LockedWithPowerGenerator:
                    case eDoorStatus.Closed_LockedWithBulkheadDC:
                    case eDoorStatus.Closed_LockedWithNoKey:
                        sdt.SetTerminalActive(def.StateSettings.LockedStateSetting.AccessibleWhenLocked);
                        break;

                    case eDoorStatus.Closed_LockedWithChainedPuzzle:
                    case eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm:
                    case eDoorStatus.Unlocked:
                        sdt.SetTerminalActive(def.StateSettings.LockedStateSetting.AccessibleWhenUnlocked);
                        break;
                }
            }));


            switch (def.StateSettings.OverrideCommandAccessibility)
            {
                case OverrideCmdAccess.ALWAYS:
                    AddOverrideCommandWithAlarmText(sdt);
                    break;
                case OverrideCmdAccess.ON_UNLOCK:
                    sdt.LinkedDoor.m_sync.add_OnDoorStateChange(new System.Action<pDoorState, bool>((state, isDropin) => {
                        switch (state.status)
                        {
                            case eDoorStatus.Closed_LockedWithChainedPuzzle:
                            case eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm:
                            case eDoorStatus.Unlocked:
                                AddOverrideCommandWithAlarmText(sdt);
                                break;
                        }
                    }));
                    break;
            }
            return sdt;
        }

        public override void Init()
        {
            base.Init();
        }

        private void BuildLevelSDTs_Instantiation()
        {
            if (!definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData)) return;
            foreach (var def in definitions[RundownManager.ActiveExpedition.LevelLayoutData].Definitions)
            {
                var sdt = BuildSDT_Instantiation(def);
                levelSDTs.Add((sdt, def));
            }
        }

        private void OnLevelCleanup()
        {
            levelSDTs.Clear();
        }

        private SecurityDoorTerminalManager() : base()
        {
            EOSWardenEventManager.Current.AddEventDefinition(SDTWardenEvents.ADD_OVERRIDE_COMMAND.ToString(), (uint)SDTWardenEvents.ADD_OVERRIDE_COMMAND, WardenEvent_AddOverrideCommand);

            // To make putting password log on SDT a thing, SDT instantiation must be done before FinalLogicLinking
            BatchBuildManager.Current.Add_OnBatchDone(LG_Factory.BatchName.LateCustomObjectCollection, BuildLevelSDTs_Instantiation);
            BatchBuildManager.Current.Add_OnBatchDone(LG_Factory.BatchName.FinalLogicLinking, BuildLevelSDTs_Passwords);

            LevelAPI.OnBuildDone += BuildLevelSDTs_UniqueCommands;
            LevelAPI.OnLevelCleanup += OnLevelCleanup;
        }

        static SecurityDoorTerminalManager() { }
    }
}
