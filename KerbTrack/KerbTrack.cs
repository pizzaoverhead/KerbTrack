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
using UnityEngine;
using System.Reflection;

[KSPAddon(KSPAddon.Startup.Flight, false)]
public class KerbTrack : MonoBehaviour
{

	public bool guiVisible = false;

	void Start()
	{
		GameEvents.onGamePause.Add(new EventVoid.OnEvent(OnPause));
		GameEvents.onGameUnpause.Add(new EventVoid.OnEvent(OnUnPause));
		RenderingManager.AddToPostDrawQueue(3, new Callback(drawGUI));
		LoadSettings();
		Debug.Log("Using TrackIR: " + useTrackIR);
		if (useTrackIR)
		{
			tracker = new TrackIRTracker();
		}
		else
		{
			tracker = new FreeTrackTracker();
		}
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

	protected Rect windowPos = new Rect(Screen.width / 2, Screen.height / 2, 10f, 10f);

	private void mainGUI(int windowID)
	{
		GUILayout.BeginVertical();

		string statusText = trackerEnabled ? "Enabled" : "Disabled";
		GUILayout.Label(statusText);

		if (CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.Internal ||
			CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("IVA Pitch");
			GUILayout.Label(pv.ToString());
			GUILayout.EndHorizontal();
			GUILayout.Label(pitchScaleIVA.ToString());
			pitchScaleIVA = GUILayout.HorizontalSlider(pitchScaleIVA, 0, 1);

			GUILayout.BeginHorizontal();
			GUILayout.Label("IVA Yaw");
			GUILayout.Label(yv.ToString());
			GUILayout.EndHorizontal();
			GUILayout.Label(yawScaleIVA.ToString());
			yawScaleIVA = GUILayout.HorizontalSlider(yawScaleIVA, 0, 1);

			GUILayout.BeginHorizontal();
			GUILayout.Label("IVA Roll");
			GUILayout.Label(rv.ToString());
			GUILayout.EndHorizontal();
			GUILayout.Label(rollScaleIVA.ToString());
			rollScaleIVA = GUILayout.HorizontalSlider(rollScaleIVA, 0, 1);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Left/Right (X)");
			GUILayout.Label(xScale.ToString());
			GUILayout.EndHorizontal();
			xScale = GUILayout.HorizontalSlider(xScale, 0, 1);
			GUILayout.Label(xp.ToString());

			GUILayout.BeginHorizontal();
			GUILayout.Label("Up/Down (Y)");
			GUILayout.Label(yp.ToString());
			GUILayout.EndHorizontal();
			GUILayout.Label(yScale.ToString());
			yScale = GUILayout.HorizontalSlider(yScale, 0, 1);

			GUILayout.BeginHorizontal();
			GUILayout.Label("In/Out (Z)");
			GUILayout.Label(zp.ToString());
			GUILayout.EndHorizontal();
			GUILayout.Label(zScale.ToString());
			zScale = GUILayout.HorizontalSlider(zScale, 0, 1);
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

		GUILayout.EndVertical();
		GUI.DragWindow();
	}

	protected void drawGUI()
	{
		if (guiVisible)
			windowPos = GUILayout.Window(-5234628, windowPos, mainGUI, "Camera debug", GUILayout.Width(250), GUILayout.Height(50));
	}

	#endregion GUI

	#region Persistence

	private ConfigNode settings;
	public void SaveSettings()
	{
		settings = new ConfigNode();
		settings.name = "SETTINGS";
		// save all [KSPField] public floats by reflection
		// foreach member field..
		foreach (FieldInfo f in GetType().GetFields()) {
			// if they're a [KSPField] float..
			if (Attribute.IsDefined(f, typeof(KSPField)) && f.FieldType.Equals(typeof(float))) {
				// add them
				settings.AddValue(f.Name, f.GetValue(this));
			}
		}
		settings.AddValue("useOVR", useOVR.ToString());
		settings.AddValue("useTrackIRSDK", useTrackIR.ToString());
		settings.AddValue("toggleEnabledKey", toggleEnabledKey.ToString());

		settings.Save(AssemblyLoader.loadedAssemblies.GetPathByType(typeof(KerbTrack)) + "/settings.cfg");
	}

	public void LoadSettings()
	{
		settings = new ConfigNode();
		settings = ConfigNode.Load(AssemblyLoader.loadedAssemblies.GetPathByType(typeof(KerbTrack)) + @"\settings.cfg".Replace('/', '\\'));

		if (settings != null)
		{
			// load all [KSPField] public floats by reflection
			// foreach member field..
			foreach (FieldInfo f in GetType().GetFields()) {
				// if they're a [KSPField] float..
				if (Attribute.IsDefined(f, typeof(KSPField)) && f.FieldType.Equals(typeof(float))) {
					// load them from the settings file.
					if (settings.HasValue(f.Name))
						f.SetValue(this, float.Parse(settings.GetValue(f.Name)));
				}
			}
			if (settings.HasValue("useOVR")) useOVR = bool.Parse(settings.GetValue("useOVR"));
			if (settings.HasValue("useTrackIRSDK")) useTrackIR = bool.Parse(settings.GetValue("useTrackIRSDK"));
			if (settings.HasValue("toggleEnabledKey")) toggleEnabledKey = (KeyCode)Enum.Parse(typeof(KeyCode), settings.GetValue("toggleEnabledKey"));
		}
	}

	#endregion Persistence

	public bool trackerEnabled = true;

	[KSPField]
	public KeyCode toggleEnabledKey = KeyCode.ScrollLock;
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
	public float rollMaxIVA = 45f;
	[KSPField]
	public float rollMinIVA = -45f;
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

	[KSPField]
	public bool useTrackIR = false;
	[KSPField]
	public ITracker tracker;

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

		if (trackerEnabled)
		{
			if(tracker != null) 
			{
				Vector3 rot = tracker.getRotation();
				float pitch = (float)rot.x;
				float yaw = (float)rot.y;
				float roll = (float)rot.z;
				Vector3 pos = tracker.getPosition();
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
							FlightCamera.fetch.camPitch = -pitch * pitchScaleFlight;
							FlightCamera.fetch.camHdg = -yaw * yawScaleFlight;
							pv = pitch * pitchScaleFlight;
							yv = yaw * yawScaleFlight;
							break;
						}
					case CameraManager.CameraMode.Internal: // Window zoom cameras
					case CameraManager.CameraMode.IVA: // Main IVA cameras
						{
							InternalCamera.Instance.transform.localEulerAngles = new Vector3(
								-Mathf.Clamp(pitch * pitchScaleIVA, pitchMinIVA, pitchMaxIVA),
								-Mathf.Clamp(yaw * yawScaleIVA, yawMinIVA, yawMaxIVA),
								Mathf.Clamp(roll * rollScaleIVA, rollMinIVA, rollMaxIVA));
							InternalCamera.Instance.transform.localPosition = new Vector3(
								Mathf.Clamp(x * xScale, xMinIVA, xMaxIVA),
								Mathf.Clamp(y * yScale, yMinIVA, yMaxIVA),
								Mathf.Clamp(z * -zScale, zMinIVA, zMaxIVA));
							pv = pitch * pitchScaleIVA;
							yv = yaw * yawScaleIVA;
							rv = roll * rollScaleIVA;
							xp = x * xScale;
							yp = y * yScale;
							zp = z * -zScale;
							// Without setting the flight camera transform, the pod rotates about without changing the background.
							FlightCamera.fetch.transform.rotation = InternalSpace.InternalToWorld(InternalCamera.Instance.transform.rotation);
							FlightCamera.fetch.transform.position = InternalSpace.InternalToWorld(InternalCamera.Instance.transform.position);
							break;
						}
					case CameraManager.CameraMode.Map:
						{
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