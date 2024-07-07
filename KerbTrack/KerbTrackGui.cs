using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbTrack
{
    public static class KerbTrackGui
    {
        public static bool guiVisible = false;
        private static Rect windowPos = new Rect(Screen.width / 4, Screen.height / 4, 10f, 10f);
        private static string[] trackerNames = Enum.GetNames(typeof(Enums.Trackers));
        public const int MaxAxisNum = 19;

        private static void MainGUI(int windowID)
        {
            GUILayout.BeginVertical();

            string statusText = (KerbTrack.trackerEnabled ? "Enabled" : "Disabled") +
                " (" + Enum.GetName(KerbTrack.toggleEnabledKey.GetType(), KerbTrack.toggleEnabledKey) + ")";
            GUILayout.Label(statusText);

            //if (activeTracker == (int)Trackers.Joystick)

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("IVA config"))
            {
                _showIvaGui = true;
                _showFlightGui = false;
                _showJoystickGui = false;
                _showSettingsGui = false;
            }
            if (GUILayout.Button("Flight config"))
            {
                _showIvaGui = false;
                _showFlightGui = true;
                _showJoystickGui = false;
                _showSettingsGui = false;
            }
            if (GUILayout.Button("Joystick config"))
            {
                _showIvaGui = false;
                _showJoystickGui = true;
                _showFlightGui = false;
                _showSettingsGui = false;
            }
            if (GUILayout.Button("Settings"))
            {
                _showIvaGui = false;
                _showFlightGui = false;
                _showJoystickGui = false;
                _showSettingsGui = true;
            }
            GUILayout.EndHorizontal();

            if (_showJoystickGui)
                JoystickGui();
            if (_showIvaGui)
                IvaGui();
            if (_showFlightGui)
                FlightGui();
            if (_showSettingsGui)
                SettingsGui();

            /*if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal ||
                CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)*/

            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        public static void OnGUI(int instanceId)
        {
            if (guiVisible)
            {
                GUI.skin = HighLogic.Skin;
                windowPos = GUILayout.Window(instanceId, windowPos, MainGUI, "KerbTrack", GUILayout.Width(500), GUILayout.Height(50));
            }
        }

        private static bool _showIvaGui = false;
        private static void IvaGui()
        {
            if (KerbTrack.tracker is IQuatTracker)
            {
                GUILayout.Label("This tracker's rotation cannot be adjusted.");
            }
            else
            {
                GuiUtils.LabelValue("IVA Pitch", KerbTrack.pv);
                GuiUtils.LabelValue("IVA Yaw", KerbTrack.yv);
                GuiUtils.LabelValue("IVA Roll", KerbTrack.rv);

                GUILayout.Label("<b>Scale</b>");
                GuiUtils.SliderScale("IVA Pitch", ref KerbTrack.pitchScaleIVA);
                GuiUtils.SliderScale("IVA Yaw", ref KerbTrack.yawScaleIVA);
                GuiUtils.SliderScale("IVA Roll", ref KerbTrack.rollScaleIVA);

                GUILayout.Label("<b>Offset</b>");
                GuiUtils.SliderOffset("IVA Pitch", ref KerbTrack.pitchOffsetIVA);
                GuiUtils.SliderOffset("IVA Yaw", ref KerbTrack.yawOffsetIVA);
                GuiUtils.SliderOffset("IVA Roll", ref KerbTrack.rollOffsetIVA);
            }

            GuiUtils.LabelValue("IVA Left-Right", KerbTrack.xp);
            GuiUtils.LabelValue("IVA Up-Down", KerbTrack.yp);
            GuiUtils.LabelValue("IVA In-Out", KerbTrack.zp);

            GUILayout.Label("<b>Scale</b>");
            GuiUtils.SliderScale("Left/Right (X)", ref KerbTrack.xScale);
            GuiUtils.SliderScale("Up/Down (Y)", ref KerbTrack.yScale);
            GuiUtils.SliderScale("In/Out (Z)", ref KerbTrack.zScale);

            GUILayout.Label("<b>Offset</b>");
            GuiUtils.Slider("Left/Right (X)", ref KerbTrack.xOffset, KerbTrack.xMinIVA, KerbTrack.xMaxIVA);
            GuiUtils.Slider("Up/Down (Y)", ref KerbTrack.yOffset, KerbTrack.yMinIVA, KerbTrack.yMaxIVA);
            GuiUtils.Slider("In/Out (Z)", ref KerbTrack.zOffset, KerbTrack.zMinIVA, KerbTrack.zMaxIVA);
        }

        private static bool _showFlightGui = false;
        private static void FlightGui()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Flight Pitch");
            GUILayout.Label(KerbTrack.pv.ToString());
            GUILayout.EndHorizontal();
            GUILayout.Label(KerbTrack.pitchScaleFlight.ToString());
            KerbTrack.pitchScaleFlight = GUILayout.HorizontalSlider(KerbTrack.pitchScaleFlight, 0, 1);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Flight Yaw");
            GUILayout.Label(KerbTrack.yv.ToString());
            GUILayout.EndHorizontal();
            GUILayout.Label(KerbTrack.yawScaleFlight.ToString());
            KerbTrack.yawScaleFlight = GUILayout.HorizontalSlider(KerbTrack.yawScaleFlight, 0, 1);
        }

        private static bool _showMapGui = false;
        private static void MapGui()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Map Pitch");
            GUILayout.Label(KerbTrack.pv.ToString());
            GUILayout.EndHorizontal();
            GUILayout.Label(KerbTrack.pitchScaleMap.ToString());
            KerbTrack.pitchScaleMap = GUILayout.HorizontalSlider(KerbTrack.pitchScaleMap, 0, 1);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Map Yaw");
            GUILayout.Label(KerbTrack.yv.ToString());
            GUILayout.EndHorizontal();
            GUILayout.Label(KerbTrack.yawScaleMap.ToString());
            KerbTrack.yawScaleMap = GUILayout.HorizontalSlider(KerbTrack.yawScaleMap, 0, 1);
        }

        private static bool _showJoystickGui = false;
        private static void JoystickGui()
        {
            string[] joysticks = Input.GetJoystickNames();
            if (joysticks.Length == 0)
            {
                GUILayout.Label("<b>No joysticks detected!</b>");
                return;
            }

            // Joystick selection.
            if (KerbTrack.joystickId >= joysticks.Length)
                KerbTrack.joystickId = 0;
            GUILayout.Label("Active joystick");
            GUILayout.Label(KerbTrack.joystickId + " - " + joysticks[KerbTrack.joystickId]);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous joystick"))
                KerbTrack.joystickId--;
            if (GUILayout.Button("Next joystick"))
                KerbTrack.joystickId++;
            GUILayout.EndHorizontal();
            if (KerbTrack.joystickId >= joysticks.Length)
                KerbTrack.joystickId = 0;
            if (KerbTrack.joystickId < 0)
                KerbTrack.joystickId = joysticks.Length - 1;
            GUILayout.Space(10);

            SelectAxis(ref KerbTrack.joyYawAxisId, ref KerbTrack.joyYawInverted, "Yaw");
            SelectAxis(ref KerbTrack.joyPitchAxisId, ref KerbTrack.joyPitchInverted, "Pitch");
            SelectAxis(ref KerbTrack.joyRollAxisId, ref KerbTrack.joyRollInverted, "Roll");
            SelectAxis(ref KerbTrack.joyXAxisId, ref KerbTrack.joyXInverted, "X");
            SelectAxis(ref KerbTrack.joyYAxisId, ref KerbTrack.joyYInverted, "Y");
            SelectAxis(ref KerbTrack.joyZAxisId, ref KerbTrack.joyZInverted, "Z");
            SelectAxis(ref KerbTrack.joyCamOrbitAxisId, ref KerbTrack.joyCamOrbitInverted, "Flight Camera Orbit");
            SelectAxis(ref KerbTrack.joyCamPitchAxisId, ref KerbTrack.joyCamPitchInverted, "Flight Camera Pitch");
        }

        private static void SelectAxis(ref int axisId, ref bool axisInverted, string axisName)
        {
            string label = axisId == -1 ? "Disabled" : axisId.ToString();
            GuiUtils.LabelValue(axisName + " axis", label);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous " + axisName + " Axis"))
                axisId--;
            if (GUILayout.Button("Next " + axisName + " Axis"))
                axisId++;
            axisInverted = GUILayout.Toggle(axisInverted, "Inverted");
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (axisId > MaxAxisNum)
                axisId = 0;
            if (axisId < -1)
                axisId = MaxAxisNum;
        }

        private static bool _showSettingsGui = false;
        private static void SettingsGui()
        {
            KerbTrack.mapTrackingEnabled = GUILayout.Toggle(KerbTrack.mapTrackingEnabled, "Enabled in map view");
            KerbTrack.externalTrackingEnabled = GUILayout.Toggle(KerbTrack.externalTrackingEnabled, "Enabled in external view");

            int oldTracker = KerbTrack.activeTracker;
            KerbTrack.activeTracker = GuiUtils.RadioButton(trackerNames, KerbTrack.activeTracker);
            if (oldTracker != KerbTrack.activeTracker)
                KerbTrack.ChangeTracker((Enums.Trackers)KerbTrack.activeTracker);

            Enums.Trackers currentTracker = (Enums.Trackers)KerbTrack.activeTracker;
            switch (currentTracker)
            {
                /*case Enums.Trackers.FreeTrack:
                    GUILayout.Label("<b>FreeTrack</b>\r\nThis is used for FaceTrackNoIR. Freetrackclient.dll must be placed next to KSP.exe, and must be a 64-bit version if 64-bit KSP is used.");
                    break;*/
                case Enums.Trackers.TrackIR:
                    GUILayout.Label("<b>TrackIR</b>\r\nSupports TrackIR and other systems which emulate it, such as opentrack.\r\n" +
                        "<b>opentrack</b>\r\nWhen using opentrack, select the Input tracker appripriate to your hardware setup, and select \"freetrack 2.0 Enhanced\" as the Output.\r\n" +
                        "In the Output settings, ensure \"Use TrackIR\" or \"Enable both\" is selected.");
                    break;
                /*case Enums.Trackers.OculusRift:
                    GUILayout.Label("<b>Oculus Rift</b>\r\nRequires an older version of the Oculus Rift runtime (2015), and only 64-bit is supported.\r\n" + 
                        "It's recommended to select \"TrackIR\" as your tracker and use opentrack instead.\r\n" +
                        "Place \"Oculus OVR PosRotWrapper 64-bit.dll\" next to KSP.exe.");
                    break;*/
                case Enums.Trackers.Joystick:
                    GUILayout.Label("<b>Joystick</b>\r\nUse your joystick axes as input. Good for assigning to a spare axis on a joystick if you don't have a head tracker.\r\n" +
                        "If you have a head tracker that isn't supported, try setting it to output as a joystick and using this setting to receive it in KerbTrack.");
                    break;
                case Enums.Trackers.OpentrackUdp:
                    GUILayout.Label("<b>Opentrack Udp</b>\r\n Supports opentrack's udp protocol, listening on port 4242.");
                    break;
            }
        }
    }
}
