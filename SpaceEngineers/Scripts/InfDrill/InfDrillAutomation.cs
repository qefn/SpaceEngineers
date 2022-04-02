using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.GUI.TextPanel;

namespace SpaceEngineers.Scripts.InfDrill {
    internal class InfDrillAutomation : MyGridProgram {
        //!!!!!!!! Remove the void return type here. when copying the content to the game !!!!!!!!!
        public void Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            if(Storage.Length > 0) {
                stage = (Stage)Enum.Parse(typeof(Stage), Storage, true);
            }

            projector = GridTerminalSystem.GetBlockWithName("infDrill Projector") as IMyProjector;
            extensionMergeBlock = GridTerminalSystem.GetBlockWithName("infDrill Extension Merge Block") as IMyShipMergeBlock;
            stabilizerMergeBlock = GridTerminalSystem.GetBlockWithName("InfDrill Stabilizer Merge Block") as IMyShipMergeBlock;
            extensionPiston = GridTerminalSystem.GetBlockWithName("InfDrill Extension Piston") as IMyPistonBase;
            stabilizerPiston = GridTerminalSystem.GetBlockWithName("InfDrill Stabilizer Piston") as IMyPistonBase;
            stabilizerConnector = GridTerminalSystem.GetBlockWithName("InfDrill Stabilizer Connector") as IMyShipConnector;
            welderGroup = GridTerminalSystem.GetBlockGroupWithName("InfDrill Welders");
        }

        public void Save() {
            Storage = stage.ToString();
            Echo("Saved");
        }

        private bool firstOut = true;
        private IMyProjector projector;
        private IMyShipMergeBlock extensionMergeBlock;
        private IMyShipMergeBlock stabilizerMergeBlock;
        private IMyPistonBase extensionPiston;
        private IMyPistonBase stabilizerPiston;
        private IMyShipConnector stabilizerConnector;
        private IMyBlockGroup welderGroup;
        private Stage stage = Stage.Initial;
        public void Main() {
            try {
                firstOut = true;
                MyEcho($"Current stage: {stage}");
                switch (stage) {
                    case Stage.Initial:
                        extensionMergeBlock.Enabled = true;
                        stage = Stage.Weld;
                        break;
                    case Stage.Weld:
                        ToggleWelders(true);
                        if (projector.RemainingBlocks == 0) {
                            stage = Stage.ConnectToPreviousSection;
                        }
                        break;
                    case Stage.ConnectToPreviousSection:
                        extensionPiston.Velocity = 0.2f;
                        List<IMyShipMergeBlock> topSectionMergeBlocks = new List<IMyShipMergeBlock>();
                        GridTerminalSystem.GetBlocksOfType(topSectionMergeBlocks, mb => mb.CustomName == "InfDrill Section  Merge Block 1");
                        MyEcho($"Top merge block count: {topSectionMergeBlocks.Count}");
                        MyEcho($"Top merge blocks connected: {topSectionMergeBlocks.Count(mb => mb.IsConnected)}");
                        if(topSectionMergeBlocks.Count == 2 && topSectionMergeBlocks.Count(mb => mb.IsConnected) == 2) {
                            ToggleWelders(false);
                            stage = Stage.UndockStabilizer;
                        }
                        break;
                    case Stage.UndockStabilizer:
                        extensionPiston.Velocity = 0.0f;
                        stabilizerConnector.Disconnect();
                        stabilizerMergeBlock.Enabled = false;
                        stabilizerPiston.Reverse();
                        stage = Stage.Drill;
                        break;
                    case Stage.Drill:
                        extensionPiston.Velocity = 0.05f;
                        if(extensionPiston.Status == PistonStatus.Extended) {
                            stage = Stage.DockStabilizer;
                        }
                        break;
                    case Stage.DockStabilizer:
                        extensionPiston.Velocity = 0.0f;
                        stabilizerPiston.Velocity = 1.0f;
                        stabilizerMergeBlock.Enabled = true;
                        if(stabilizerMergeBlock.IsConnected) {
                            stabilizerConnector.Connect();
                            stage = Stage.RetractExtensionPiston;
                        }
                        break;
                    case Stage.RetractExtensionPiston:
                        extensionMergeBlock.Enabled = false;
                        extensionPiston.Velocity = -1.0f;
                        if(extensionPiston.CurrentPosition < 5.0f) {
                            stage = Stage.Initial;
                        }
                        break;
                    default:
                        MyEcho($"No steps defined for stage {stage}");
                        break;
                }
            } catch (Exception e) {
                MyEcho($"Error occured in script: {e.Message}");
            }
        }

        private enum Stage {
            Initial,
            Weld,
            ConnectToPreviousSection,
            UndockStabilizer,
            Drill,
            DockStabilizer,
            RetractExtensionPiston
        }

        private void ToggleWelders(bool on) {
            List<IMyShipWelder> welders = new List<IMyShipWelder>();
            welderGroup.GetBlocksOfType(welders);
            foreach (IMyShipWelder welder in welders) {
                welder.Enabled = on;
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
