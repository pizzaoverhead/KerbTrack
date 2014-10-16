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
using KSP.IO;

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class KerbTrack : MonoBehaviour
{
    public bool guiVisible = false;
    public bool trackerEnabled = true;
    public ITracker tracker;

    // [...]GameData\KerbTrack\Plugins\PluginData\KerbTrack\settings.cfg
    private string savePath = AssemblyLoader.loadedAssemblies.GetPathByType(typeof(KerbTrack)).Replace('/', '\\') + "\\settings.cfg";

    public enum Trackers
    {
        FreeTrack = 0,
        TrackIR = 1,
        OculusRift = 2
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
                        Debug.Log("KerbTrack: Using FreeTrack");
                        tracker = new FreeTrackTracker();
                        break;
                    }
                case Trackers.TrackIR:
                    {
                        Debug.Log("KerbTrack: Using TrackIR");
                        tracker = new TrackIRTracker();
                        break;
                    }
                case Trackers.OculusRift:
                    {
                        Debug.Log("KerbTrack: Using Oculus Rift");
                        tracker = new OVRTracker();
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
        Debug.Log("KerbTrack: Starting");
        GameEvents.onGamePause.Add(new EventVoid.OnEvent(OnPause));
        GameEvents.onGameUnpause.Add(new EventVoid.OnEvent(OnUnPause));
        RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));
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

    private void slider(string label, ref float variable, float from, float to)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label + ": " + variable.ToString());
        GUILayout.FlexibleSpace();
        variable = GUILayout.HorizontalSlider(variable, from, to, GUILayout.Width(100));
        GUILayout.EndHorizontal();
    }

    private void sliderScale(string label, ref float variable)
    {
        slider(label, ref variable, 0, 1);
    }

    private void sliderOffset(string label, ref float variable)
    {
        slider(label, ref variable, -1, 1);
    }

    private void label(string text, object obj)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(text);
        GUILayout.FlexibleSpace();
        GUILayout.Label(obj.ToString(), GUILayout.Width(100));
        GUILayout.EndHorizontal();
    }

    private int radioButton(string[] labels, int selectedIndex)
    {
        int newSelection = selectedIndex;
        bool selectionChanged = false;
        GUILayout.BeginHorizontal();
        for (int i = 0; i < labels.Length; i++)
        {
            bool selected = GUILayout.Toggle(i == selectedIndex && !selectionChanged, labels[i]);
            if (selected && i != selectedIndex)
            {
                selectionChanged = true;
                newSelection = i;
            }
        }
        GUILayout.EndHorizontal();
        return newSelection;
    }

    private static string[] trackerNames = Enum.GetNames(typeof(Trackers));

    private void mainGUI(int windowID)
    {
        GUILayout.BeginVertical();

        string statusText = (trackerEnabled ? "Enabled" : "Disabled") +
            " (" + Enum.GetName(toggleEnabledKey.GetType(), toggleEnabledKey) + ")";
        GUILayout.Label(statusText);

        if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal ||
            CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)
        {
            if (tracker is IQuatTracker)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Tracker exports quaternions; rotation cannot be adjusted.");
                GUILayout.EndHorizontal();
            }
            else
            {
                label("IVA Pitch", pv);
                label("IVA Yaw", yv);
                label("IVA Roll", rv);

                GUILayout.Label("Scale");
                sliderScale("IVA Pitch", ref pitchScaleIVA);
                sliderScale("IVA Yaw", ref yawScaleIVA);
                sliderScale("IVA Roll", ref rollScaleIVA);

                GUILayout.Label("Offset");
                sliderOffset("IVA Pitch", ref pitchOffsetIVA);
                sliderOffset("IVA Yaw", ref yawOffsetIVA);
                sliderOffset("IVA Roll", ref rollOffsetIVA);
            }

            label("IVA Left-Right", xp);
            label("IVA Up-Down", yp);
            label("IVA In-Out", zp);

            GUILayout.Label("Scale");
            sliderScale("Left/Right (X)", ref xScale);
            sliderScale("Up/Down (Y)", ref yScale);
            sliderScale("In/Out (Z)", ref zScale);

            GUILayout.Label("Offset");
            slider("Left/Right (X)", ref xOffset, xMinIVA, xMaxIVA);
            slider("Up/Down (Y)", ref yOffset, yMinIVA, yMaxIVA);
            slider("In/Out (Z)", ref zOffset, zMinIVA, zMaxIVA);
        }
        else
        {
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

        GUILayout.Space(10);

        mapTrackingEnabled = GUILayout.Toggle(mapTrackingEnabled, "Enabled in map view");
        externalTrackingEnabled = GUILayout.Toggle(externalTrackingEnabled, "Enabled in external view");

        int oldTracker = activeTracker;
        activeTracker = radioButton(trackerNames, activeTracker);
        if (oldTracker != activeTracker)
            ChangeTracker((Trackers)activeTracker);

        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    protected void drawGUI()
    {
        if (guiVisible)
            windowPos = GUILayout.Window(-5234628, windowPos, mainGUI, "KerbTrack", GUILayout.Width(250), GUILayout.Height(50));
    }

    #endregion GUI

    #region Persistence

    ConfigNode settings = null;
    public void SaveSettings()
    {
        Debug.Log("KerbTrack: Saving settings to " + savePath);
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
                    //rot = tracker.GetRotation();
                    //pos = tracker.GetPosition();
                }
                catch (Exception e)
                {
                    Debug.Log("KerbTrack: " + GetTrackerName((Trackers)activeTracker) + " error: " + e.Message + "\n" + e.StackTrace);
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
                            FlightCamera.fetch.camPitch = -pitch * pitchScaleFlight;
                            FlightCamera.fetch.camHdg = -yaw * yawScaleFlight;
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
}
