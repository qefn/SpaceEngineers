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

        public float lastAngle = -1.0f;

        public Stage stage = Stage.StopRotation;

        private bool extend = true;

        public void Main() {
            try {
                firstOut = true;
                MyEcho($"Current stage: {stage}");
                MyEcho($"Is Extending: {extend}");
                MyEcho("");

                IMyPistonBase mainPiston = GridTerminalSystem.GetBlockWithName("Sprill Main Piston") as IMyPistonBase;
                MyEcho($"Main Piston MaxLimit: {mainPiston.MaxLimit.ToString()}");
                MyEcho($"Main Piston Position: {mainPiston.CurrentPosition}");

                IMyPistonBase mainPiston2 = GridTerminalSystem.GetBlockWithName("Sprill Main Piston 2") as IMyPistonBase;
                MyEcho($"Main Piston 2 MaxLimit: {mainPiston2.MaxLimit.ToString()}");
                MyEcho($"Main Piston 2 Position: {mainPiston2.CurrentPosition}");
                MyEcho("");

                IMyMotorAdvancedStator rotor = GridTerminalSystem.GetBlockWithName("Sprill Rotor") as IMyMotorAdvancedStator;
                MyEcho($"Rotor Angle: {rotor.Angle.ToString("0.0")}");
                MyEcho($"Last Iteration Rotor Angle: {lastAngle.ToString("0.000")}");
                MyEcho($"Rotor Velocity: {rotor.TargetVelocityRPM.ToString("0.000")}");
                MyEcho("");

                IMyPistonBase sidePiston1 = GridTerminalSystem.GetBlockWithName("Sprill Side Piston 1") as IMyPistonBase;
                MyEcho($"Side Piston 1 MaxLimit: {sidePiston1.MaxLimit.ToString()}");
                MyEcho($"Side Piston 1 Position: {sidePiston1.CurrentPosition}");

                IMyPistonBase sidePiston2 = GridTerminalSystem.GetBlockWithName("Sprill Side Piston 2") as IMyPistonBase;
                MyEcho($"Side Piston 2 MaxLimit: {sidePiston2.MaxLimit.ToString()}");
                MyEcho($"Side Piston 2 Position: {sidePiston2.CurrentPosition}");

                if (lastAngle < 0.0f) {
                    lastAngle = rotor.Angle;
                }

                switch (stage) {
                    case Stage.StopRotation:
                        rotor.TargetVelocityRPM = 0.0f;
                        stage = Stage.StartExtendHorizontal;
                        break;
                    case Stage.StartExtendHorizontal:
                        sidePiston1.Velocity = 0.4f;
                        sidePiston2.Velocity = 0.4f;

                        sidePiston1.MaxLimit += 1.0f;
                        sidePiston2.MaxLimit += 1.0f;

                        stage = Stage.FinishExtendHorizontal;
                        break;
                    case Stage.FinishExtendHorizontal:
                        if ((sidePiston1.MaxLimit - sidePiston1.CurrentPosition) > 0.0001f || (sidePiston2.MaxLimit - sidePiston2.CurrentPosition) > 0.0001f) {
                            lastAngle = rotor.Angle;
                            return;
                        }
                        stage = Stage.Rotate;
                        break;
                    case Stage.Rotate:
                        rotor.TargetVelocityRPM = 0.2f;

                        float diff = rotor.Angle - lastAngle;
                        MyEcho($"Rotot Angle Change: {diff.ToString()}");
                        if (diff < -2.0f) {
                            stage = Stage.ExtendVertical;
                        }
                        break;
                    case Stage.ExtendVertical:
                        if (extend) {
                            mainPiston.Velocity = 0.4f;
                            mainPiston2.Velocity = 0.4f;
                            if ((35.0f - mainPiston.MaxLimit) > 0.00001f) {
                                mainPiston.MaxLimit += 1.0f;
                            } else {
                                if ((35.0f - mainPiston2.MaxLimit) > 0.00001f) {
                                    mainPiston2.MaxLimit += 1.0f;
                                } else {
                                    extend = false;
                                    mainPiston.MinLimit = 35.0f;
                                    mainPiston2.MinLimit = 35.0f;
                                    stage = Stage.StopRotation;
                                    lastAngle = rotor.Angle;
                                    return;
                                }
                            }
                        } else {
                            mainPiston.Velocity = -0.4f;
                            mainPiston2.Velocity = -0.4f;
                            if (mainPiston2.MinLimit > 0.00001f) {
                                mainPiston2.MinLimit -= 1.0f;
                            } else {
                                if ((mainPiston.MinLimit - 23.0f) > 0.00001f) {
                                    mainPiston.MinLimit -= 1.0f;
                                } else {
                                    extend = true;
                                    mainPiston.MaxLimit = 23.0f;
                                    mainPiston2.MaxLimit = 0.0f;
                                    stage = Stage.StopRotation;
                                    lastAngle = rotor.Angle;
                                    return;
                                }
                            }
                        }
                        stage = Stage.Rotate;
                        break;
                }
                lastAngle = rotor.Angle;
            } catch (Exception e) {
                Echo($"Error occured in script: {e.Message}");
            }
        }

        public enum Stage {
            StopRotation,
            StartExtendHorizontal,
            FinishExtendHorizontal,
            Rotate,
            ExtendVertical
        }

        private void MyEcho(string msg) {
            Echo(msg);
            if (((IMyTextSurfaceProvider)Me).SurfaceCount > 0) {
                string extra = "";
                if (!firstOut) extra = Environment.NewLine;
                IMyTextSurface surface = ((IMyTextSurfaceProvider)Me).GetSurface(0);
                surface.ContentType = ContentType.TEXT_AND_IMAGE;
                surface.WriteText(extra + msg, !firstOut);
                firstOut = false;
            }
        }
    }
}