using System.Collections.Generic;
using ExtraObjectiveSetup.BaseClasses;
using LevelGeneration;
using ExtraObjectiveSetup;
using ExtraObjectiveSetup.Utils;
using SecDoorTerminalInterface;
using GameData;
using ChainedPuzzles;
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

        public bool TryGetZoneEntranceSecDoor(LG_Zone zone, out LG_SecurityDoor door)
        {
            if (zone == null)
            {
                door = null;
                return false;
            }
            if (zone.m_sourceGate == null)
            {
                door = null;
                return false;
            }
            if (zone.m_sourceGate.SpawnedDoor == null)
            {
                door = null;
                return false;
            }
            door = zone.m_sourceGate.SpawnedDoor.TryCast<LG_SecurityDoor>();
            return door != null;
        }

        protected override void AddDefinitions(ZoneDefinitionsForLevel<SecurityDoorTerminalDefinition> definitions)
        {
            // instantiate sec door terminal instance in order
            Sort(definitions);
            base.AddDefinitions(definitions);
        }

        public static (eDimensionIndex dimensionIndex, LG_LayerType layerType, eLocalZoneIndex localIndex) GlobalZoneIndexOf(SecDoorTerminal sdt) => (sdt.SpawnNode.m_dimension.DimensionIndex, sdt.SpawnNode.LayerType, sdt.SpawnNode.m_zone.LocalIndex);

        private void AddUniqueCommand(SecDoorTerminal sdt, SDTCustomCommand cmd)
        {
            if (!sdt.CmdProcessor.TryGetUniqueCommandSlot(out var uniqueCmdSlot)) return;

            sdt.CmdProcessor.AddCommand(uniqueCmdSlot, cmd.Command, cmd.CommandDesc, cmd.SpecialCommandRule, cmd.CommandEvents.ToIl2Cpp(), cmd.PostCommandOutputs.ToIl2Cpp());
            for (int i = 0; i < cmd.CommandEvents.Count; i++)
            {
                var e = cmd.CommandEvents[i];
                if (e.ChainPuzzle != 0U)
                {
                    ChainedPuzzleDataBlock block = GameDataBlockBase<ChainedPuzzleDataBlock>.GetBlock(e.ChainPuzzle);
                    if (block != null)
                    {
                        ChainedPuzzleInstance puzzleInstance = ChainedPuzzleManager.CreatePuzzleInstance(block, sdt.ComputerTerminal.SpawnNode.m_area, sdt.ComputerTerminal.m_wardenObjectiveSecurityScanAlign.position, sdt.ComputerTerminal.m_wardenObjectiveSecurityScanAlign, e.UseStaticBioscanPoints);
                        var events = cmd.CommandEvents.GetRange(i, cmd.CommandEvents.Count - i).ToIl2Cpp(); // due to the nature of lambda, events cannot be put into System.Action
                        puzzleInstance.OnPuzzleSolved += new System.Action(() => {
                            WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(events, eWardenObjectiveEventTrigger.None, true);
                        });

                        sdt.ComputerTerminal.SetChainPuzzleForCommand(uniqueCmdSlot, i, puzzleInstance);
                    }
                }
            }
        }

        private void BuildSDT_UniqueCommands(SecDoorTerminal sdt, SecurityDoorTerminalDefinition def)
        {
            def.UniqueCommands.ForEach(cmd => AddUniqueCommand(sdt, cmd));
        }

        private void AddOverrideCommandWithAlarmText(SecDoorTerminal sdt)
        {

            string command_desc = $"<color=orange>{Text.Get(841)}</color>";
            
            if(sdt.LinkedDoorLocks.ChainedPuzzleToSolve != null && sdt.LinkedDoorLocks.ChainedPuzzleToSolve.Data.TriggerAlarmOnActivate)
            {
                command_desc = $"<color=orange>{string.Format(Text.Get(840), sdt.LinkedDoorLocks.ChainedPuzzleToSolve?.Data.PublicAlarmName)}</color>";
            }

            sdt.AddOverrideCommand(OVERRIDE_COMMAND, command_desc);
        }

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
            sdt.BioscanScanSolvedBehaviour = def.Settings.CPSolvedBehaviour;
            def.LocalLogFiles.ForEach(log => sdt.ComputerTerminal.AddLocalLog(log, true));

            switch (sdt.LinkedDoor.m_sync.GetCurrentSyncState().status)
            {
                case eDoorStatus.Closed_LockedWithKeyItem:
                    // TODO: if sdt is interactable, then inserting key is impossible, and vice ersa
                    if (!def.Settings.InteractableWhenDoorIsLocked)
                    {
                        break; // SDT default behaviour
                    }

                    sdt.LinkedDoorLocks.m_gateKeyItemNeeded.keyPickupCore.m_interact.add_OnPickedUpByPlayer(new System.Action<Player.PlayerAgent>((p) => {
                        sdt.SetTerminalActive(false);
                    }));

                    sdt.SetTerminalActive(true);
                    var hintText = new List<string>() {
                        string.Format($"<color=orange>{Text.Get(849)}</color>", sdt.LinkedDoorLocks.m_gateKeyItemNeeded.PublicName)
                    };
                    sdt.ComputerTerminal.m_command.AddOutput(hintText.ToIl2Cpp());
                    if (def.Settings.AddOverrideCommandWhenDoorUnlocked)
                    {
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
                    }
                    break;

                case eDoorStatus.Closed_LockedWithPowerGenerator:
                case eDoorStatus.Closed_LockedWithBulkheadDC:
                case eDoorStatus.Closed_LockedWithNoKey:
                    sdt.SetTerminalActive(def.Settings.InteractableWhenDoorIsLocked);
                    if (def.Settings.AddOverrideCommandWhenDoorUnlocked)
                    {
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
                    }
                    break;

                case eDoorStatus.Closed:
                case eDoorStatus.Closed_LockedWithChainedPuzzle:
                case eDoorStatus.Closed_LockedWithChainedPuzzle_Alarm:
                case eDoorStatus.Unlocked:
                    if (def.Settings.AddOverrideCommandWhenDoorUnlocked) AddOverrideCommandWithAlarmText(sdt);
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

        private void BuildLevelSDTs_UniqueCommands()
        {
            levelSDTs.ForEach((tp) => BuildSDT_UniqueCommands(tp.sdt, tp.def));
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
            BatchBuildManager.Current.Add_OnBatchDone(LG_Factory.BatchName.FinalLogicLinking, BuildLevelSDTs_UniqueCommands);

            LevelAPI.OnLevelCleanup += OnLevelCleanup;
        }

        static SecurityDoorTerminalManager() { }
    }
}
