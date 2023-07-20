using System;
using LevelGeneration;
using System.Collections.Generic;

namespace ExtraObjectiveSetup.Tweaks.SecurityDoorTerminal
{
    public class SDTStartStateData
    {
        public TERM_State StartingState { set; get; } = TERM_State.Sleeping;

        public bool PasswordProtected { set; get; } = false;

        public string Password { set; get; } = string.Empty;

        public string PasswordHintText { set; get; } = "Password Required.";

        public bool GeneratePassword { set; get; } = true;

        public int PasswordPartCount { set; get; } = 1;

        public bool ShowPasswordLength { set; get; } = false;

        public bool ShowPasswordPartPositions { set; get; } = false;

        public List<List<CustomTerminalZoneSelectionData>> TerminalZoneSelectionDatas { set; get; } = new() { new() { new() } }; 

        public SDTStartStateData()
        {
            // TODO: debug this
            PasswordPartCount = Math.Max(1, PasswordPartCount);
        }
    }
}
