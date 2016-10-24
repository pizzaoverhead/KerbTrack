/*
 * InternalCamera:
 * Rotation X+ = rotate head down to feet.
 * Rotation Y+ = rotate head right
 * Rotation Z+ = rotate head anti-clockwise
 * Translation X+ = Right
 * Translation Y+ = Up
 * Translation Z+ = Away
 * 
 * FlightCamera: 
 * Pitch: Looking down in positive, looking up is negative
 * Heading: From above, rotating the craft anti-clockwise is positive, clockwise is negative.
 */

using System;
using System.Reflection;
using UnityEngine;

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class KerbTrack : MonoBehaviour
{
    public bool guiVisible = false;
    public bool trackerEnabled = true;
    public ITracker tracker;

    // [...]GameData\KerbTrack\Plugins\PluginData\KerbTrack\settings.cfg
    private string savePath = AssemblyLoader.loadedAssemblies.GetPathByType(typeof(KerbTrack)).Replace("/", @"\\") + @"\\settings.cfg";

    public enum Trackers
    {
        FreeTrack = 0,
        TrackIR = 1,
        OculusRift = 2,
        Joystick = 3
    }

    public string GetTrackerName(Trackers t)
    {
        return Enum.GetName(t.GetType(), t);
    }

    public void ChangeTracker(Trackers t)
    {
        try
        {
            switch (t)
            {
                case Trackers.FreeTrack:
                    {
                        Debug.Log("[KerbTrack] Using FreeTrack");
                        tracker = new FreeTrackTracker();
                        break;
                    }
                case Trackers.TrackIR:
                    {
                        Debug.Log("[KerbTrack] Using TrackIR");
                        tracker = new TrackIRTracker();
                        break;
                    }
                case Trackers.OculusRift:
                    {
                        Debug.Log("[KerbTrack] Using Oculus Rift");
                        tracker = new OVRTracker();
                        break;
                    }
                case Trackers.Joystick:
                    {
                        Debug.Log("KerbTrack: Using Joystick");
                        tracker = new JoystickTracker();
                        break;
                    }
            }
        }
        catch (Exception)
        {
            trackerEnabled = false;
            throw;
        }
    }


    void Start()
    {
        Debug.Log("[KerbTrack] Starting");
        GameEvents.onGamePause.Add(new EventVoid.OnEvent(OnPause));
        GameEvents.onGameUnpause.Add(new EventVoid.OnEvent(OnUnPause));
        LoadSettings();
        ChangeTracker((Trackers)activeTracker);
    }

    public void OnDestroy()
    {
        GameEvents.onGamePause.Remove(new EventVoid.OnEvent(OnPause));
        GameEvents.onGameUnpause.Remove(new EventVoid.OnEvent(OnUnPause));
        SaveSettings();
    }

    #region GUI

    public void OnPause()
    {
        guiVisible = true;
    }

    public void OnUnPause()
    {
        guiVisible = false;
    }

    protected Rect windowPos = new Rect(Screen.width / 4, Screen.height / 4, 10f, 10f);

    private static string[] trackerNames = Enum.GetNames(typeof(Trackers));

    private void mainGUI(int windowID)
    {
        GUILayout.BeginVertical();

        string statusText = (trackerEnabled ? "Enabled" : "Disabled") +
            " (" + Enum.GetName(toggleEnabledKey.GetType(), toggleEnabledKey) + ")";
        GUILayout.Label(statusText);

        //if (activeTracker == (int)Trackers.Joystick)

        JoystickGui();
        IvaGui();
        FlightGui();

        /*if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal ||
            CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)*/

        GUILayout.Space(10);

        mapTrackingEnabled = GUILayout.Toggle(mapTrackingEnabled, "Enabled in map view");
        externalTrackingEnabled = GUILayout.Toggle(externalTrackingEnabled, "Enabled in external view");

        int oldTracker = activeTracker;
        activeTracker = GuiUtils.RadioButton(trackerNames, activeTracker);
        if (oldTracker != activeTracker)
            ChangeTracker((Trackers)activeTracker);

        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    public void OnGUI()
    {
        if (guiVisible)
            windowPos = GUILayout.Window(-5234628, windowPos, mainGUI, "KerbTrack", GUILayout.Width(250), GUILayout.Height(50));
    }

    private bool _showIvaGui = false;
    private void IvaGui()
    {
        string buttonText = _showIvaGui ? "Hide IVA config" : "Show IVA config";
        if (GUILayout.Button(buttonText))
            _showIvaGui = !_showIvaGui;
        if (!_showIvaGui) return;

        if (tracker is IQuatTracker)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("This tracker's rotation cannot be adjusted.");
            GUILayout.EndHorizontal();
        }
        else
        {
            GuiUtils.LabelValue("IVA Pitch", pv);
            GuiUtils.LabelValue("IVA Yaw", yv);
            GuiUtils.LabelValue("IVA Roll", rv);

            GUILayout.Label("<b>Scale</b>");
            GuiUtils.SliderScale("IVA Pitch", ref pitchScaleIVA);
            GuiUtils.SliderScale("IVA Yaw", ref yawScaleIVA);
            GuiUtils.SliderScale("IVA Roll", ref rollScaleIVA);

            GUILayout.Label("<b>Offset</b>");
            GuiUtils.SliderOffset("IVA Pitch", ref pitchOffsetIVA);
            GuiUtils.SliderOffset("IVA Yaw", ref yawOffsetIVA);
            GuiUtils.SliderOffset("IVA Roll", ref rollOffsetIVA);
        }

        GuiUtils.LabelValue("IVA Left-Right", xp);
        GuiUtils.LabelValue("IVA Up-Down", yp);
        GuiUtils.LabelValue("IVA In-Out", zp);

        GUILayout.Label("<b>Scale</b>");
        GuiUtils.SliderScale("Left/Right (X)", ref xScale);
        GuiUtils.SliderScale("Up/Down (Y)", ref yScale);
        GuiUtils.SliderScale("In/Out (Z)", ref zScale);

        GUILayout.Label("<b>Offset</b>");
        GuiUtils.Slider("Left/Right (X)", ref xOffset, xMinIVA, xMaxIVA);
        GuiUtils.Slider("Up/Down (Y)", ref yOffset, yMinIVA, yMaxIVA);
        GuiUtils.Slider("In/Out (Z)", ref zOffset, zMinIVA, zMaxIVA);
    }

    private bool _showFlightGui = false;
    private void FlightGui()
    {
        string buttonText = _showFlightGui ? "Hide flight config" : "Show flight config";
        if (GUILayout.Button(buttonText))
            _showFlightGui = !_showFlightGui;

        if (!_showFlightGui) return;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Flight Pitch");
        GUILayout.Label(pv.ToString());
        GUILayout.EndHorizontal();
        GUILayout.Label(pitchScaleFlight.ToString());
        pitchScaleFlight = GUILayout.HorizontalSlider(pitchScaleFlight, 0, 1);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Flight Yaw");
        GUILayout.Label(yv.ToString());
        GUILayout.EndHorizontal();
        GUILayout.Label(yawScaleFlight.ToString());
        yawScaleFlight = GUILayout.HorizontalSlider(yawScaleFlight, 0, 1);
    }

    private bool _showJoystickGui = false;
    private void JoystickGui()
    {
        string buttonText = _showJoystickGui ? "Hide joystick config" : "Show joystick config";
        if (GUILayout.Button(buttonText))
            _showJoystickGui = !_showJoystickGui;

        if (!_showJoystickGui) return;

        string[] joysticks = Input.GetJoystickNames();
        if (joysticks.Length == 0)
        {
            GUILayout.Label("<b>No joysticks detected!</b>");
            return;
        }

        // Joystick selection.
        if (joystickId >= joysticks.Length)
            joystickId = 0;
        GUILayout.Label("Active joystick");
        GUILayout.Label(joystickId + " - " + joysticks[joystickId]);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous joystick"))
            joystickId--;
        if (GUILayout.Button("Next joystick"))
            joystickId++;
        GUILayout.EndHorizontal();
        if (joystickId >= joysticks.Length)
            joystickId = 0;
        if (joystickId < 0)
            joystickId = joysticks.Length - 1;
        GUILayout.Space(10);

        int maxAxisNum = 19;

        // Pitch axis selection.
        string pitchLabel = joyPitchAxisId == -1 ? "Disabled" : joyPitchAxisId.ToString();
        GuiUtils.LabelValue("Pitch axis", pitchLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous pitch axis"))
            joyPitchAxisId--;
        if (GUILayout.Button("Next pitch axis"))
            joyPitchAxisId++;
        joyPitchInverted = GUILayout.Toggle(joyPitchInverted, "Inverted");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        if (joyPitchAxisId > maxAxisNum)
            joyPitchAxisId = 0;
        if (joyPitchAxisId < -1)
            joyPitchAxisId = maxAxisNum;

        // Yaw axis selection.
        string yawLabel = joyYawAxisId == -1 ? "Disabled" : joyYawAxisId.ToString();
        GuiUtils.LabelValue("Yaw axis", yawLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous yaw axis"))
            joyYawAxisId--;
        if (GUILayout.Button("Next yaw axis"))
            joyYawAxisId++;
        joyYawInverted = GUILayout.Toggle(joyYawInverted, "Inverted");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        if (joyYawAxisId > maxAxisNum)
            joyYawAxisId = 0;
        if (joyYawAxisId < -1)
            joyYawAxisId = maxAxisNum;

        // Roll axis selection.
        string rollLabel = joyRollAxisId == -1 ? "Disabled" : joyRollAxisId.ToString();
        GuiUtils.LabelValue("Roll axis", rollLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous roll axis"))
            joyRollAxisId--;
        if (GUILayout.Button("Next roll axis"))
            joyRollAxisId++;
        joyRollInverted = GUILayout.Toggle(joyRollInverted, "Inverted");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        if (joyRollAxisId > maxAxisNum)
            joyRollAxisId = 0;
        if (joyRollAxisId < -1)
            joyRollAxisId = maxAxisNum;

        // Flight camera orbit axis selection.
        string camOrbitLabel = joyCamOrbitAxisId == -1 ? "Disabled" : joyCamOrbitAxisId.ToString();
        GuiUtils.LabelValue("Flight camera orbit axis", camOrbitLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous orbit axis"))
            joyCamOrbitAxisId--;
        if (GUILayout.Button("Next orbit axis"))
            joyCamOrbitAxisId++;
        joyCamOrbitInverted = GUILayout.Toggle(joyCamOrbitInverted, "Inverted");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        if (joyCamOrbitAxisId > maxAxisNum)
            joyCamOrbitAxisId = 0;
        if (joyCamOrbitAxisId < -1)
            joyCamOrbitAxisId = maxAxisNum;

        // Flight camera pitch axis selection.
        string camPitchLabel = joyCamPitchAxisId == -1 ? "Disabled" : joyCamPitchAxisId.ToString();
        GuiUtils.LabelValue("Flight camera pitch axis", camPitchLabel);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Previous cam pitch axis"))
            joyCamPitchAxisId--;
        if (GUILayout.Button("Next cam pitch axis"))
            joyCamPitchAxisId++;
        joyRollInverted = GUILayout.Toggle(joyRollInverted, "Inverted");
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        if (joyCamPitchAxisId > maxAxisNum)
            joyCamPitchAxisId = 0;
        if (joyCamPitchAxisId < -1)
            joyCamPitchAxisId = maxAxisNum;
    }

    #endregion GUI

    #region Persistence

    ConfigNode settings = null;
    public void SaveSettings()
    {
        Debug.Log("[KerbTrack] Saving settings to " + savePath);
        settings = new ConfigNode();
        settings.name = "SETTINGS";
        // Save all [KSPField] public floats by reflection
        // foreach member field...
        foreach (FieldInfo f in GetType().GetFields())
        {
            // if they're a [KSPField] float...
            if (Attribute.IsDefined(f, typeof(KSPField)) && f.FieldType.Equals(typeof(float)))
            {
                // add them.
                settings.AddValue(f.Name, f.GetValue(this));
            }
        }
        settings.AddValue("tracker", GetTrackerName((Trackers)activeTracker));
        settings.AddValue("toggleEnabledKey", toggleEnabledKey.ToString());
        settings.AddValue("resetOrientationKey", resetOrientationKey.ToString());
        settings.AddValue("externalTrackingEnabled", externalTrackingEnabled.ToString());
        settings.AddValue("mapTrackingEnabled", mapTrackingEnabled.ToString());

        settings.AddValue("joystickId", joystickId.ToString());
        settings.AddValue("joypitchAxisId", joyPitchAxisId.ToString());
        settings.AddValue("joyPitchInverted", joyPitchInverted.ToString());
        settings.AddValue("joyyawAxisId", joyYawAxisId.ToString());
        settings.AddValue("joyYawInverted", joyYawInverted.ToString());
        settings.AddValue("joyrollAxisId", joyRollAxisId.ToString());
        settings.AddValue("joyRollInverted", joyRollInverted.ToString());
        settings.AddValue("joyCamPitchAxisId", joyCamPitchAxisId.ToString());
        settings.AddValue("joyCamPitchInverted", joyCamPitchInverted.ToString());
        settings.AddValue("joyCamOrbitAxisId", joyCamOrbitAxisId.ToString());
        settings.AddValue("joyCamOrbitInverted", joyCamOrbitInverted.ToString());

        settings.Save(savePath);
    }

    public void LoadSettings()
    {
        Debug.Log("KerbTrack: Loading settings from " + savePath);
        settings = new ConfigNode();
        settings = ConfigNode.Load(savePath);

        if (settings != null)
        {
            // Load all [KSPField] public floats by reflection
            // foreach member field...
            foreach (FieldInfo f in GetType().GetFields())
            {
                // if they're a [KSPField] float...
                if (Attribute.IsDefined(f, typeof(KSPField)) && f.FieldType.Equals(typeof(float)))
                {
                    // load them from the settings file.
                    if (settings.HasValue(f.Name))
                        f.SetValue(this, float.Parse(settings.GetValue(f.Name)));
                }
            }

            if (settings.HasValue("tracker"))
            {
                string t = settings.GetValue("tracker");
                activeTracker = (int)Enum.Parse(typeof(Trackers), t, true);
            }
            if (settings.HasValue("toggleEnabledKey")) toggleEnabledKey =
                (KeyCode)Enum.Parse(typeof(KeyCode), settings.GetValue("toggleEnabledKey"));
            if (settings.HasValue("resetOrientationKey")) resetOrientationKey =
                (KeyCode)Enum.Parse(typeof(KeyCode), settings.GetValue("resetOrientationKey"));
            if (settings.HasValue("externalTrackingEnabled"))
                externalTrackingEnabled = Boolean.Parse(settings.GetValue("externalTrackingEnabled"));
            if (settings.HasValue("mapTrackingEnabled"))
                mapTrackingEnabled = Boolean.Parse(settings.GetValue("mapTrackingEnabled"));

            if (settings.HasValue("joystickId"))
                joystickId = Int32.Parse(settings.GetValue("joystickId"));
            if (settings.HasValue("joyPitchAxisId"))
                joyPitchAxisId = Int32.Parse(settings.GetValue("joyPitchAxisId"));
            if (settings.HasValue("joyPitchInverted"))
                joyPitchInverted = Boolean.Parse(settings.GetValue("joyPitchInverted"));
            if (settings.HasValue("joyYawAxisId"))
                joyYawAxisId = Int32.Parse(settings.GetValue("joyYawAxisId"));
            if (settings.HasValue("joyYawInverted"))
                joyYawInverted = Boolean.Parse(settings.GetValue("joyYawInverted"));
            if (settings.HasValue("joyRollAxisId"))
                joyRollAxisId = Int32.Parse(settings.GetValue("joyRollAxisId"));
            if (settings.HasValue("joyRollInverted"))
                joyRollInverted = Boolean.Parse(settings.GetValue("joyRollInverted"));
            if (settings.HasValue("joyCamPitchAxisId"))
                joyCamPitchAxisId = Int32.Parse(settings.GetValue("joyCamPitchAxisId"));
            if (settings.HasValue("joyCamPitchInverted"))
                joyCamPitchInverted = Boolean.Parse(settings.GetValue("joyCamPitchInverted"));
            if (settings.HasValue("joyCamOrbitAxisId"))
                joyCamOrbitAxisId = Int32.Parse(settings.GetValue("joyCamOrbitAxisId"));
            if (settings.HasValue("joyCamOrbitInverted"))
                joyCamOrbitInverted = Boolean.Parse(settings.GetValue("joyCamOrbitInverted"));
        }
    }

    #endregion Persistence

    public int activeTracker = (int)Trackers.FreeTrack;

    [KSPField]
    public KeyCode toggleEnabledKey = KeyCode.ScrollLock;
    [KSPField]
    public KeyCode resetOrientationKey = KeyCode.Home;
    [KSPField]
    public bool externalTrackingEnabled = true;
    [KSPField]
    public bool mapTrackingEnabled = true;

    [KSPField]
    public static int joystickId = 0;
    [KSPField]
    public static int joyPitchAxisId = 1;
    [KSPField]
    public static bool joyPitchInverted = false;
    [KSPField]
    public static int joyYawAxisId = 0;
    [KSPField]
    public static bool joyYawInverted = true;
    [KSPField]
    public static int joyRollAxisId = -1;
    [KSPField]
    public static bool joyRollInverted = false;
    [KSPField]
    public static int joyCamPitchAxisId = -1;
    [KSPField]
    public static bool joyCamPitchInverted = false;
    [KSPField]
    public static int joyCamOrbitAxisId = -1;
    [KSPField]
    public static bool joyCamOrbitInverted = false;

    [KSPField]
    public float pitchScaleIVA = 0.3f, pitchOffsetIVA = 0.0f;
    [KSPField]
    public float yawScaleIVA = 0.3f, yawOffsetIVA = 0.0f;
    [KSPField]
    public float rollScaleIVA = 0.15f, rollOffsetIVA = 0.0f;
    [KSPField]
    public float xScale = 0.1f, xOffset = 0.0f;
    [KSPField]
    public float yScale = 0.1f, yOffset = 0.0f;
    [KSPField]
    public float zScale = 0.1f, zOffset = 0.0f;
    [KSPField]
    public float pitchScaleFlight = 0.01f;
    [KSPField]
    public float yawScaleFlight = 0.01f;

    // Ignore the built-in max/min values.
    [KSPField]
    public float pitchMaxIVA = 120f;
    [KSPField]
    public float pitchMinIVA = -90f;
    [KSPField]
    public float yawMaxIVA = 135f;
    [KSPField]
    public float yawMinIVA = -135f;
    [KSPField]
    public float rollMaxIVA = 90f;
    [KSPField]
    public float rollMinIVA = -90f;
    [KSPField]
    public float xMaxIVA = 0.15f;
    [KSPField]
    public float xMinIVA = -0.15f;
    [KSPField]
    public float yMaxIVA = 0.1f;
    [KSPField]
    public float yMinIVA = -0.1f;
    [KSPField]
    public float zMaxIVA = 0.1f;
    [KSPField]
    public float zMinIVA = -0.15f;

    // Values after scaling.
    public float pv = 0f;
    public float yv = 0f;
    public float rv = 0f;
    public float xp = 0f;
    public float yp = 0f;
    public float zp = 0f;

    Quaternion lastRotation = Quaternion.identity;

    void Update()
    {
        if (Input.GetKeyDown(toggleEnabledKey))
            trackerEnabled = !trackerEnabled;
        if (Input.GetKeyDown(resetOrientationKey))
            tracker.ResetOrientation();

        if (trackerEnabled)
        {
            if (tracker != null)
            {
                Vector3 rot = new Vector3(0, 0, 0);
                Vector3 pos = new Vector3(0, 0, 0);
                try
                {
                    tracker.GetData(ref rot, ref pos);
                }
                catch (Exception e)
                {
                    Debug.Log("[KerbTrack] " + GetTrackerName((Trackers)activeTracker) + " error: " + e.Message + "\n" + e.StackTrace);
                    trackerEnabled = false;
                    return;
                }
                float pitch = (float)rot.x;
                float yaw = (float)rot.y;
                float roll = (float)rot.z;
                float x = pos.x;
                float y = pos.y;
                float z = pos.z;

                switch (CameraManager.Instance.currentCameraMode)
                {
                    case CameraManager.CameraMode.External:
                        {
                            break;
                        }
                    case CameraManager.CameraMode.Flight:
                        {
                            if (!externalTrackingEnabled) return;

                            if (activeTracker == (int)Trackers.Joystick)
                            {
                                Vector2 joyCamPos = new Vector3(0, 0);
                                ((JoystickTracker)tracker).GetFlightCamData(ref joyCamPos);
                                bool relative = true;
                                if (relative)
                                {
                                    FlightCamera.fetch.camPitch += -joyCamPos.x * pitchScaleFlight * Time.deltaTime;
                                    FlightCamera.fetch.camHdg += -joyCamPos.y * yawScaleFlight * Time.deltaTime;
                                }
                                else
                                {
                                    FlightCamera.fetch.camPitch = -joyCamPos.x * pitchScaleFlight;
                                    FlightCamera.fetch.camHdg = -joyCamPos.y * yawScaleFlight;
                                }
                            }
                            else
                            {
                                bool freeLook = true;
                                if (freeLook)
                                {
                                    pv = pitch * pitchScaleIVA + pitchOffsetIVA;
                                    yv = yaw * yawScaleIVA + yawOffsetIVA;
                                    rv = roll * rollScaleIVA + rollOffsetIVA;
                                    xp = x * xScale + xOffset;
                                    yp = y * yScale + yOffset;
                                    zp = z * -zScale + zOffset;
                                    // If tracker supports quaternions, use them directly.
                                    var qtracker = tracker as IQuatTracker;
                                    if (qtracker != null)
                                    {
                                        //var joystickRotation = FlightCamera.fetch.transform.localRotation;
                                        var quat = FlightCamera.fetch.transform.localRotation;
                                        qtracker.GetQuatData(ref quat);
                                        Quaternion deltaRot;
                                        if (lastRotation != Quaternion.identity)
                                            deltaRot = Quaternion.Inverse(lastRotation) * quat;
                                        else
                                            deltaRot = quat;
                                        lastRotation = quat;

                                        FlightCamera.fetch.transform.localRotation = FlightCamera.fetch.transform.localRotation * deltaRot;
                                    }
                                    else
                                    {
                                        FlightCamera.fetch.transform.localEulerAngles = new Vector3(pv, yv, rv);
                                    }
                                    //FlightCamera.fetch.transform.localPosition = new Vector3(xp, yp, zp);
                                    // Without setting the flight camera transform, the pod rotates about without changing the background.
                                    //FlightCamera.fetch.transform.rotation = InternalSpace.InternalToWorld(InternalCamera.Instance.transform.rotation);
                                    //FlightCamera.fetch.transform.position = InternalSpace.InternalToWorld(InternalCamera.Instance.transform.position);
                                    break;
                                }
                                else
                                {
                                    FlightCamera.fetch.camPitch = -pitch * pitchScaleFlight;
                                    FlightCamera.fetch.camHdg = -yaw * yawScaleFlight;
                                }
                            }
                            pv = pitch * pitchScaleFlight;
                            yv = yaw * yawScaleFlight;
                            break;
                        }
                    case CameraManager.CameraMode.Internal: // Window zoom cameras
                    case CameraManager.CameraMode.IVA: // Main IVA cameras
                        {
                            pv = pitch * pitchScaleIVA + pitchOffsetIVA;
                            yv = yaw * yawScaleIVA + yawOffsetIVA;
                            rv = roll * rollScaleIVA + rollOffsetIVA;
                            xp = x * xScale + xOffset;
                            yp = y * yScale + yOffset;
                            zp = z * -zScale + zOffset;
                            // If tracker supports quaternions, use them directly.
                            var qtracker = tracker as IQuatTracker;
                            if (qtracker != null)
                            {
                                var quat = InternalCamera.Instance.transform.localRotation;
                                qtracker.GetQuatData(ref quat);
                                InternalCamera.Instance.transform.localRotation = quat;
                            }
                            else
                            {
                                InternalCamera.Instance.transform.localEulerAngles = new Vector3(
                                    -Mathf.Clamp(pv, pitchMinIVA, pitchMaxIVA),
                                    -Mathf.Clamp(yv, yawMinIVA, yawMaxIVA),
                                    Mathf.Clamp(rv, rollMinIVA, rollMaxIVA));
                            }
                            InternalCamera.Instance.transform.localPosition = new Vector3(
                                Mathf.Clamp(xp, xMinIVA, xMaxIVA),
                                Mathf.Clamp(yp, yMinIVA, yMaxIVA),
                                Mathf.Clamp(zp, zMinIVA, zMaxIVA));
                            // Without setting the flight camera transform, the pod rotates about without changing the background.
                            FlightCamera.fetch.transform.rotation = InternalSpace.InternalToWorld(InternalCamera.Instance.transform.rotation);
                            FlightCamera.fetch.transform.position = InternalSpace.InternalToWorld(InternalCamera.Instance.transform.position);
                            break;
                        }
                    case CameraManager.CameraMode.Map:
                        {
                            if (!mapTrackingEnabled) return;
                            PlanetariumCamera.fetch.camPitch = -pitch * pitchScaleFlight;
                            PlanetariumCamera.fetch.camHdg = -yaw * yawScaleFlight;
                            pv = pitch * pitchScaleFlight;
                            yv = yaw * yawScaleFlight;
                            break;
                        }
                }
            }
        }
    }

    void LateUpdate()
    {
        var qtracker = tracker as IQuatTracker;
        if (qtracker != null)
        {
            //var joystickRotation = FlightCamera.fetch.transform.localRotation;
            var quat = FlightCamera.fetch.transform.localRotation;
            qtracker.GetQuatData(ref quat);
            Quaternion deltaRot;
            if (lastRotation != Quaternion.identity)
                deltaRot = Quaternion.Inverse(lastRotation) * quat;
            else
                deltaRot = quat;
            lastRotation = quat;

            FlightCamera.fetch.transform.localRotation = FlightCamera.fetch.transform.localRotation * deltaRot;
        }
    }
}
