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

namespace KerbTrack
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class KerbTrack : MonoBehaviour
    {
        public static bool trackerEnabled = true;
        public static ITracker tracker;

        // [...]GameData\KerbTrack\Plugins\PluginData\KerbTrack\settings.cfg
        private string savePath = System.IO.Path.Combine(
            AssemblyLoader.loadedAssemblies.GetPathByType(typeof(KerbTrack)), "settings.cfg");

        public string GetTrackerName(Enums.Trackers t)
        {
            return Enum.GetName(t.GetType(), t);
        }

        public static void ChangeTracker(Enums.Trackers t)
        {
            try
            {
                if(tracker != null)
                    tracker.Stop();

                switch (t)
                {
                    /*case Enums.Trackers.FreeTrack:
                        {
                            Debug.Log("[KerbTrack] Using FreeTrack");
                            tracker = new FreeTrackTracker();
                            break;
                        }*/
                    case Enums.Trackers.TrackIR:
                        {
                            Debug.Log("[KerbTrack] Using TrackIR");
                            tracker = new TrackIRTracker();
                            break;
                        }
                    /*case Enums.Trackers.OculusRift:
                        {
                            Debug.Log("[KerbTrack] Using Oculus Rift");
                            tracker = new OVRTracker();
                            break;
                        }*/
                    case Enums.Trackers.Joystick:
                        {
                            Debug.Log("KerbTrack: Using Joystick");
                            tracker = new JoystickTracker();
                            break;
                        }
                    case Enums.Trackers.OpentrackUdp:
                        {
                            Debug.Log("KerbTrack: Using OpentrackUdp");
                            tracker = new OpentrackUdpTracker();
                            break;
                        }
                }

                trackerEnabled = true;
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
            ChangeTracker((Enums.Trackers)activeTracker);
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
            KerbTrackGui.guiVisible = true;
        }

        public void OnUnPause()
        {
            KerbTrackGui.guiVisible = false;
        }

        public void OnGUI()
        {
            KerbTrackGui.OnGUI(GetInstanceID());
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
            settings.AddValue("tracker", GetTrackerName((Enums.Trackers)activeTracker));
            settings.AddValue("toggleEnabledKey", toggleEnabledKey.ToString());
            settings.AddValue("resetOrientationKey", resetOrientationKey.ToString());
            settings.AddValue("externalTrackingEnabled", externalTrackingEnabled.ToString());
            settings.AddValue("mapTrackingEnabled", mapTrackingEnabled.ToString());

            settings.AddValue("joystickId", joystickId.ToString());
            settings.AddValue("joyPitchAxisId", joyPitchAxisId.ToString());
            settings.AddValue("joyPitchInverted", joyPitchInverted.ToString());
            settings.AddValue("joyYawAxisId", joyYawAxisId.ToString());
            settings.AddValue("joyYawInverted", joyYawInverted.ToString());
            settings.AddValue("joyRollAxisId", joyRollAxisId.ToString());
            settings.AddValue("joyRollInverted", joyRollInverted.ToString());
            settings.AddValue("joyXAxisId", joyXAxisId.ToString());
            settings.AddValue("joyXInverted", joyXInverted.ToString());
            settings.AddValue("joyYAxisId", joyYAxisId.ToString());
            settings.AddValue("joyYInverted", joyYInverted.ToString());
            settings.AddValue("joyZAxisId", joyZAxisId.ToString());
            settings.AddValue("joyZInverted", joyZInverted.ToString());

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
                    activeTracker = (int)Enum.Parse(typeof(Enums.Trackers), t, true);
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
                if (settings.HasValue("joyXAxisId"))
                    joyXAxisId = Int32.Parse(settings.GetValue("joyXAxisId"));
                if (settings.HasValue("joyXInverted"))
                    joyXInverted = Boolean.Parse(settings.GetValue("joyXInverted"));
                if (settings.HasValue("joyYAxisId"))
                    joyYAxisId = Int32.Parse(settings.GetValue("joyYAxisId"));
                if (settings.HasValue("joyYInverted"))
                    joyYInverted = Boolean.Parse(settings.GetValue("joyYInverted"));
                if (settings.HasValue("joyZAxisId"))
                    joyZAxisId = Int32.Parse(settings.GetValue("joyZAxisId"));
                if (settings.HasValue("joyZInverted"))
                    joyZInverted = Boolean.Parse(settings.GetValue("joyZInverted"));
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

        public static int activeTracker = (int)Enums.Trackers.Joystick;

        [KSPField]
        public static KeyCode toggleEnabledKey = KeyCode.ScrollLock;
        [KSPField]
        public static KeyCode resetOrientationKey = KeyCode.Home;
        [KSPField]
        public static bool externalTrackingEnabled = true;
        [KSPField]
        public static bool mapTrackingEnabled = true;

        [KSPField]
        public static int joystickId = 0;
        [KSPField]
        public static int joyYawAxisId = 0;
        [KSPField]
        public static bool joyYawInverted = true;
        [KSPField]
        public static int joyPitchAxisId = 1;
        [KSPField]
        public static bool joyPitchInverted = true;
        [KSPField]
        public static int joyRollAxisId = -1;
        [KSPField]
        public static bool joyRollInverted = false;
        [KSPField]
        public static int joyXAxisId = 3;
        [KSPField]
        public static bool joyXInverted = false;
        [KSPField]
        public static int joyYAxisId = 4;
        [KSPField]
        public static bool joyYInverted = true;
        [KSPField]
        public static int joyZAxisId = 2;
        [KSPField]
        public static bool joyZInverted = false;
        [KSPField]
        public static int joyCamPitchAxisId = -1;
        [KSPField]
        public static bool joyCamPitchInverted = false;
        [KSPField]
        public static int joyCamOrbitAxisId = -1;
        [KSPField]
        public static bool joyCamOrbitInverted = false;

        [KSPField]
        public static float pitchScaleIVA = 0.3f, pitchOffsetIVA = 0.0f;
        [KSPField]
        public static float yawScaleIVA = 0.3f, yawOffsetIVA = 0.0f;
        [KSPField]
        public static float rollScaleIVA = 0.15f, rollOffsetIVA = 0.0f;
        [KSPField]
        public static float xScale = 0.1f, xOffset = 0.0f;
        [KSPField]
        public static float yScale = 0.1f, yOffset = 0.0f;
        [KSPField]
        public static float zScale = 0.1f, zOffset = 0.0f;
        [KSPField]
        public static float pitchScaleFlight = 0.01f;
        [KSPField]
        public static float yawScaleFlight = 0.01f;
        [KSPField]
        public static float pitchScaleMap = 0.01f;
        [KSPField]
        public static float yawScaleMap = 0.01f;

        // Ignore the built-in max/min values.
        [KSPField]
        public static float pitchMaxIVA = 120f;
        [KSPField]
        public static float pitchMinIVA = -90f;
        [KSPField]
        public static float yawMaxIVA = 135f;
        [KSPField]
        public static float yawMinIVA = -135f;
        [KSPField]
        public static float rollMaxIVA = 90f;
        [KSPField]
        public static float rollMinIVA = -90f;
        [KSPField]
        public static float xMaxIVA = 0.15f;
        [KSPField]
        public static float xMinIVA = -0.15f;
        [KSPField]
        public static float yMaxIVA = 0.1f;
        [KSPField]
        public static float yMinIVA = -0.1f;
        [KSPField]
        public static float zMaxIVA = 0.1f;
        [KSPField]
        public static float zMinIVA = -0.15f;

        // Values after scaling.
        public static float pv = 0f;
        public static float yv = 0f;
        public static float rv = 0f;
        public static float xp = 0f;
        public static float yp = 0f;
        public static float zp = 0f;

        Quaternion lastRotation = Quaternion.identity;

        void Update()
        {
            if (Input.GetKeyDown(toggleEnabledKey))
                trackerEnabled = !trackerEnabled;
            if (Input.GetKeyDown(resetOrientationKey))
                tracker.ResetOrientation();

            if (!trackerEnabled)
                return;

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
                    Debug.Log("[KerbTrack] " + GetTrackerName((Enums.Trackers)activeTracker) + " error: " + e.Message + "\n" + e.StackTrace);
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

                            if (activeTracker == (int)Enums.Trackers.Joystick)
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
                                        pv = pitch * pitchScaleIVA + pitchOffsetIVA;
                                        yv = yaw * yawScaleIVA + yawOffsetIVA;
                                        rv = roll * rollScaleIVA + rollOffsetIVA;
                                        xp = x * xScale + xOffset;
                                        yp = y * yScale + yOffset;
                                        zp = z * -zScale + zOffset;
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
                                    // Orbit around the vessel in the same way as the stock camera.
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
                            PlanetariumCamera.fetch.camPitch = -pitch * pitchScaleMap;
                            PlanetariumCamera.fetch.camHdg = -yaw * yawScaleMap;
                            pv = pitch * pitchScaleMap;
                            yv = yaw * yawScaleMap;
                            break;
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
}
