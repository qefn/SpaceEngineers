using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;

namespace SpaceEngineers.Scripts.Sprill {
    public class SprillSidePiston : MyGridProgram {
        //!!!!!!!! Remove the void return type here. when copying the content to the game !!!!!!!!!
        public void Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public bool firstOut = true;

        public void Main() {
            try {
                firstOut = true;
                IMyPistonBase mainPiston = GridTerminalSystem.GetBlockWithName("Sprill Main Piston") as IMyPistonBase;
                List<IMyPistonBase> sidePistons = GetSidePistons().ToList();
                MyEcho($"Main Piston Status: {mainPiston.Status.ToString()}");
                MyEcho($"Main Piston DetailInfo: {mainPiston.DetailedInfo}");
                MyEcho("");
                PrintSidePistoninfos(sidePistons);
                if (mainPiston.Status == PistonStatus.Extended) {
                    ExtendSidePistons(sidePistons);
                    mainPiston.Velocity = -0.025f;
                } else if (mainPiston.Status == PistonStatus.Retracted) {
                    ExtendSidePistons(sidePistons);
                    mainPiston.Velocity = 0.025f;
                }
            } catch (Exception e) {
                Echo($"Error occured in script: {e.Message}");
            }
        }

        private void MyEcho(string msg) {
            Echo(msg);
            if (Me.SurfaceCount > 0) {
                string extra = "";
                if (!firstOut && msg.Length != 0) extra = Environment.NewLine;
                IMyTextSurface surface = Me.GetSurface(0);
                surface.ContentType = ContentType.TEXT_AND_IMAGE;
                surface.WriteText(extra + msg, !firstOut);
                firstOut = false;
            }
        }

        private void ExtendSidePistons(List<IMyPistonBase> sidePistons) {
            for (int i = 0; i < sidePistons.Count; i++) {
                ExtendSidePiston(sidePistons[i]);
            }
        }

        private void PrintSidePistoninfos(List<IMyPistonBase> sidePistons) {
            for (int i = 0; i < sidePistons.Count; i++) {
                PrintSidePistonInfo(sidePistons[i]);
            }
        }

        private IEnumerable<IMyPistonBase> GetSidePistons() {
            string sidePiston1Name = "Sprill Side Piston 1";
            string sidePiston2Name = "Sprill Side Piston 2";
            yield return GridTerminalSystem.GetBlockWithName(sidePiston1Name) as IMyPistonBase;
            yield return GridTerminalSystem.GetBlockWithName(sidePiston2Name) as IMyPistonBase;
        }

        private void ExtendSidePiston(IMyPistonBase sidePiston) {
            sidePiston.Velocity = 0.25f;
            sidePiston.MaxLimit += 2.0f;
        }

        private void PrintSidePistonInfo(IMyPistonBase sidePiston) {
            MyEcho($"{sidePiston.CustomName} MaxLimit: {sidePiston.MaxLimit.ToString()}");
        }
    }
}