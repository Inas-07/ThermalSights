using ExtraObjectiveSetup.BaseClasses;
using SecDoorTerminalInterface;
using GameData;
using ChainedPuzzles;
using GTFO.API.Extensions;
using EOSExt.SecurityDoorTerminal.Definition;

namespace EOSExt.SecurityDoorTerminal
{
    public sealed partial class SecurityDoorTerminalManager : ZoneDefinitionManager<SecurityDoorTerminalDefinition>
    {
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

        private void BuildLevelSDTs_UniqueCommands()
        {
            levelSDTs.ForEach((tp) => BuildSDT_UniqueCommands(tp.sdt, tp.def));
        }
    }
}
