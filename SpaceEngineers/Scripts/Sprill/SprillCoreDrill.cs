using Sandbox.ModAPI.Ingame;
using System;
using VRage.Game.GUI.TextPanel;

namespace SpaceEngineers.Scripts.Sprill {
    public class SprillCoreDrill : MyGridProgram {
        //!!!!!!!! Remove the void return type here. when copying the content to the game !!!!!!!!!
        public void Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public bool firstOut = true;

        public float lastAngle = -1.0f;

        public void Main() {
            try {
                firstOut = true;
                IMyPistonBase mainPiston = GridTerminalSystem.GetBlockWithName("Sprill Main Piston 2") as IMyPistonBase;
                MyEcho($"Main Piston DetailInfo: {mainPiston.MaxLimit.ToString()}");
                MyEcho($"Main Piston DetailInfo: {mainPiston.DetailedInfo}");
                MyEcho("");

                IMyMotorAdvancedStator rotor = GridTerminalSystem.GetBlockWithName("Sprill Rotor") as IMyMotorAdvancedStator;
                MyEcho($"Rotor Angle: {rotor.Angle.ToString("0.0")}");
                MyEcho($"Last Iteration Rotor Angle: {lastAngle.ToString("0.000")}");
                MyEcho($"Rotor Velocity: {rotor.TargetVelocityRPM.ToString("0.000")}");

                if (rotor.TargetVelocityRPM < 0.00001f) {
                    rotor.TargetVelocityRPM = 0.2f;
                }

                if (lastAngle < 0.0f) {
                    lastAngle = rotor.Angle;
                }
                float diff = rotor.Angle - lastAngle;
                MyEcho($"Diff: {diff.ToString()}");
                if (diff < -2.0f) {
                    rotor.TargetVelocityRPM = 0.0f;
                    mainPiston.MaxLimit += 2.0f;
                    mainPiston.Velocity = 0.4f;
                }

                lastAngle = rotor.Angle;

            } catch (Exception e) {
                Echo($"Error occured in script: {e.Message}");
            }
        }

        private void MyEcho(string msg) {
            Echo(msg);
            if (Me.SurfaceCount > 0) {
                string extra = "";
                if (!firstOut) extra = Environment.NewLine;
                IMyTextSurface surface = Me.GetSurface(0);
                surface.ContentType = ContentType.TEXT_AND_IMAGE;
                surface.WriteText(extra + msg, !firstOut);
                firstOut = false;
            }
        }

    }
}