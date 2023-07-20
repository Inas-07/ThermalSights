using System.Collections.Generic;
using ExtraObjectiveSetup.BaseClasses;
using LevelGeneration;
using ExtraObjectiveSetup.Utils;
using SecDoorTerminalInterface;
using GameData;
using ChainedPuzzles;
using GTFO.API.Extensions;
using LogUtils;
using Localization;

namespace ExtraObjectiveSetup.Tweaks.SecurityDoorTerminal
{
    public sealed partial class SecurityDoorTerminalManager : ZoneDefinitionManager<SecurityDoorTerminalDefinition>
    {
        public static SecurityDoorTerminalManager Current { get; private set; } = new();

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

        internal void BuildPassword(SecDoorTerminal sdt, SDTStartStateData data)
        {
            if (data.PasswordProtected && !data.GeneratePassword)
            {
                sdt.ComputerTerminal.LockWithPassword(data.Password, data.PasswordHintText);
                return;
            }

            if (data.TerminalZoneSelectionDatas.Count <= 0)
            {
                EOSLogger.Error($"Tried to generate a password for terminal {sdt.ComputerTerminal.PublicName} with no {typeof(TerminalZoneSelectionData).Name}!! This is not allowed.");
                return;
            }
            string codeWord = SerialGenerator.GetCodeWord();
            string hintText = data.PasswordHintText;
            string logPositionText = "<b>[Forgot your password?]</b> Backup security key(s) located in logs on ";
            int passwordPartCount = data.PasswordPartCount;

            if (codeWord.Length % passwordPartCount != 0)
            {
                EOSLogger.Error($"Build() password.Length ({codeWord.Length}) not divisible by passwordParts ({passwordPartCount}). Defaulting to 1.");
                passwordPartCount = 1;
            }

            string[] strArray;
            if (passwordPartCount <= 1)
            {
                strArray = new string[1] { codeWord };
            }
            else
            {
                strArray = codeWord.SplitIntoChunksArray(codeWord.Length / passwordPartCount);
            }

            string str3 = "";
            if (data.ShowPasswordPartPositions)
            {
                for (int i = 0; i < strArray[0].Length; ++i)
                    str3 += "-";
            }

            HashSet<LG_ComputerTerminal> computerTerminalSet = new HashSet<LG_ComputerTerminal>();
            for (int i = 0; i < passwordPartCount; ++i)
            {
                int j = i % data.TerminalZoneSelectionDatas.Count;
                var selectedRange = data.TerminalZoneSelectionDatas[j];
                int selectedIdx = Builder.SessionSeedRandom.Range(0, selectedRange.Count, "NO_TAG");
                var selectedData = selectedRange[selectedIdx];
                LG_ComputerTerminal passwordTerminal;
                
                if(selectedData.SeedType == eSeedType.None)
                {
                    if(!Builder.CurrentFloor.TryGetZoneByLocalIndex(selectedData.DimensionIndex, selectedData.LayerType, selectedData.LocalIndex, out var passwordZone) || passwordZone == null)
                    {
                        EOSLogger.Error($"BuildPassword: seedType {eSeedType.None} specified but cannot find zone {selectedData.GlobalZoneIndexTuple()}");
                        //continue; // fallback to critical error output
                    }

                    if (passwordZone.TerminalsSpawnedInZone.Count <= 0)
                    {
                        EOSLogger.Error($"BuildPassword: seedType {eSeedType.None} specified but cannot find terminal zone {selectedData.GlobalZoneIndexTuple()}");
                        //continue; // fallback to critical error output
                    }

                    passwordTerminal = passwordZone.TerminalsSpawnedInZone[selectedData.TerminalIndex];
                }
                else
                {
                    passwordTerminal = Helper.SelectTerminal(selectedData.DimensionIndex, selectedData.LayerType, selectedData.LocalIndex, 
                        selectedData.SeedType, predicate: x => !x.HasPasswordPart);
                }

                if (passwordTerminal == null)
                {
                    EOSLogger.Error($"BuildPassword: CRITICAL ERROR, could not get a LG_ComputerTerminal for password part ({i + 1}/{passwordPartCount}) for {sdt.ComputerTerminal.PublicName} backup log.");
                    continue;
                }

                string str4 = "";
                string str5;
                if (data.ShowPasswordPartPositions)
                {
                    for (int index3 = 0; index3 < i; ++index3) 
                        str4 += str3;
                    str5 = str4 + strArray[i];
                    for (int index4 = i; index4 < passwordPartCount - 1; ++index4)
                        str5 += str3;
                }
                else
                {
                    str5 = strArray[i];
                }

                string str6 = data.ShowPasswordPartPositions ? $"0{i + 1}"  : $"0{Builder.SessionSeedRandom.Range(0, 9, "NO_TAG")}";
                TerminalLogFileData passwordLog = new TerminalLogFileData()
                {
                    FileName = $"key{str6}_{LG_TerminalPasswordLinkerJob.GetTerminalNumber(sdt.ComputerTerminal)}{(passwordTerminal.HasPasswordPart ? "_1" : "")}.LOG",
                    FileContent = new LocalizedText() {
                        UntranslatedText = string.Format(Text.Get(passwordPartCount > 1 ? 1431221909 : 2260297836), str5),
                        Id = 0u
                    }
                };
                passwordTerminal.AddLocalLog(passwordLog);
                if (!computerTerminalSet.Contains(passwordTerminal))
                {
                    if (i > 0) logPositionText += ", ";
                    logPositionText = logPositionText + passwordTerminal.PublicName + " in " + passwordTerminal.SpawnNode.m_zone.AliasName;
                }
                computerTerminalSet.Add(passwordTerminal);
                passwordTerminal.HasPasswordPart = true;
            }

            string str7 = logPositionText + ".";
            if (data.ShowPasswordLength)
            {
                sdt.ComputerTerminal.LockWithPassword(codeWord, hintText, str7, "Char[" + codeWord.Length + "]");
            }
            else
            {
                sdt.ComputerTerminal.LockWithPassword(codeWord, hintText, str7);
            }
        }

        internal void Build(SecurityDoorTerminalDefinition def) // invoked on batch `Distribution` done
        {
            var (dimensionIndex, layer, localIndex) = def.GlobalZoneIndexTuple();

            if (!Builder.CurrentFloor.TryGetZoneByLocalIndex(dimensionIndex, layer, localIndex, out var targetZone) || targetZone == null)
            {
                EOSLogger.Error($"SecDoorTerminal: Cannot find target zone {def.GlobalZoneIndexTuple()}");
                return;
            }

            if (!TryGetZoneEntranceSecDoor(targetZone, out var door) || door == null)
            {
                EOSLogger.Error($"SecDoorTerminal: Cannot find spawned sec-door for zone {def.GlobalZoneIndexTuple()}");
                return;
            }

            var sdt = SecDoorTerminal.Place(door, new TerminalStartStateData()
            {
                StartingState = TERM_State.Sleeping
            });

            sdt.AddOverrideCommand("OVERRIDE_TEST", "TESTING");
            //sdt.AddOpenCommand("OPEN_TEST", "TESTING"); // Designed for AddOverrideCommand I think, so do not use it here

            targetZone.TerminalsSpawnedInZone.Add(sdt.ComputerTerminal); // set as the last terminal in zone
            sdt.BioscanScanSolvedBehaviour = def.CPSolvedBehaviour;
            def.LocalLogFiles.ForEach(log => sdt.ComputerTerminal.AddLocalLog(log, true));
            def.UniqueCommands.ForEach(cmd => AddUniqueCommand(sdt, cmd));

            if (def.StartingStateData.PasswordProtected)
            {
                BuildPassword(sdt, def.StartingStateData);
            }
        }

        public override void Init()
        {
            base.Init();
            EOSLogger.Warning("SecurityDoorTerminal Manager loaded.");
        }

        private void OnBuildDone()
        {
            if (!definitions.ContainsKey(RundownManager.ActiveExpedition.LevelLayoutData)) return;
            definitions[RundownManager.ActiveExpedition.LevelLayoutData].Definitions.ForEach(Build);
        }

        private SecurityDoorTerminalManager(): base()
        {
            BatchBuildManager.Current.Add_OnBeforeFactoryDone(OnBuildDone);
        }

        static SecurityDoorTerminalManager() { }
    }
}
