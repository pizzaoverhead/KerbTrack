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

            int maxAxisNum = 19;

            // Pitch axis selection.
            string pitchLabel = KerbTrack.joyPitchAxisId == -1 ? "Disabled" : KerbTrack.joyPitchAxisId.ToString();
            GuiUtils.LabelValue("Pitch axis", pitchLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous pitch axis"))
                KerbTrack.joyPitchAxisId--;
            if (GUILayout.Button("Next pitch axis"))
                KerbTrack.joyPitchAxisId++;
            KerbTrack.joyPitchInverted = GUILayout.Toggle(KerbTrack.joyPitchInverted, "Inverted");
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (KerbTrack.joyPitchAxisId > maxAxisNum)
                KerbTrack.joyPitchAxisId = 0;
            if (KerbTrack.joyPitchAxisId < -1)
                KerbTrack.joyPitchAxisId = maxAxisNum;

            // Yaw axis selection.
            string yawLabel = KerbTrack.joyYawAxisId == -1 ? "Disabled" : KerbTrack.joyYawAxisId.ToString();
            GuiUtils.LabelValue("Yaw axis", yawLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous yaw axis"))
                KerbTrack.joyYawAxisId--;
            if (GUILayout.Button("Next yaw axis"))
                KerbTrack.joyYawAxisId++;
            KerbTrack.joyYawInverted = GUILayout.Toggle(KerbTrack.joyYawInverted, "Inverted");
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (KerbTrack.joyYawAxisId > maxAxisNum)
                KerbTrack.joyYawAxisId = 0;
            if (KerbTrack.joyYawAxisId < -1)
                KerbTrack.joyYawAxisId = maxAxisNum;

            // Roll axis selection.
            string rollLabel = KerbTrack.joyRollAxisId == -1 ? "Disabled" : KerbTrack.joyRollAxisId.ToString();
            GuiUtils.LabelValue("Roll axis", rollLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous roll axis"))
                KerbTrack.joyRollAxisId--;
            if (GUILayout.Button("Next roll axis"))
                KerbTrack.joyRollAxisId++;
            KerbTrack.joyRollInverted = GUILayout.Toggle(KerbTrack.joyRollInverted, "Inverted");
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (KerbTrack.joyRollAxisId > maxAxisNum)
                KerbTrack.joyRollAxisId = 0;
            if (KerbTrack.joyRollAxisId < -1)
                KerbTrack.joyRollAxisId = maxAxisNum;

            // Flight camera orbit axis selection.
            string camOrbitLabel = KerbTrack.joyCamOrbitAxisId == -1 ? "Disabled" : KerbTrack.joyCamOrbitAxisId.ToString();
            GuiUtils.LabelValue("Flight camera orbit axis", camOrbitLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous orbit axis"))
                KerbTrack.joyCamOrbitAxisId--;
            if (GUILayout.Button("Next orbit axis"))
                KerbTrack.joyCamOrbitAxisId++;
            KerbTrack.joyCamOrbitInverted = GUILayout.Toggle(KerbTrack.joyCamOrbitInverted, "Inverted");
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (KerbTrack.joyCamOrbitAxisId > maxAxisNum)
                KerbTrack.joyCamOrbitAxisId = 0;
            if (KerbTrack.joyCamOrbitAxisId < -1)
                KerbTrack.joyCamOrbitAxisId = maxAxisNum;

            // Flight camera pitch axis selection.
            string camPitchLabel = KerbTrack.joyCamPitchAxisId == -1 ? "Disabled" : KerbTrack.joyCamPitchAxisId.ToString();
            GuiUtils.LabelValue("Flight camera pitch axis", camPitchLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous cam pitch axis"))
                KerbTrack.joyCamPitchAxisId--;
            if (GUILayout.Button("Next cam pitch axis"))
                KerbTrack.joyCamPitchAxisId++;
            KerbTrack.joyRollInverted = GUILayout.Toggle(KerbTrack.joyRollInverted, "Inverted");
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (KerbTrack.joyCamPitchAxisId > maxAxisNum)
                KerbTrack.joyCamPitchAxisId = 0;
            if (KerbTrack.joyCamPitchAxisId < -1)
                KerbTrack.joyCamPitchAxisId = maxAxisNum;
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
        }
    }
}