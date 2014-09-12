using System;
using UnityEngine;
using System.Runtime.InteropServices;

class OVRTracker : ITracker
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector3f {
		public float x, y, z;
	}

	[DllImport("Oculus OVR PosRotWrapper")]
	public static extern IntPtr wrapper_init();
	[DllImport("Oculus OVR PosRotWrapper")]
	public static extern void wrapper_destroy(IntPtr ptr);
	[DllImport("Oculus OVR PosRotWrapper")]
	public static extern void wrapper_start_sensor(IntPtr ptr);
	[DllImport("Oculus OVR PosRotWrapper")]
	public static extern Vector3f wrapper_get_rotation(IntPtr ptr);
	[DllImport("Oculus OVR PosRotWrapper")]
	public static extern Vector3f wrapper_get_position(IntPtr ptr);
	
	public IntPtr wrapper;
	public OVRTracker()
	{
		Debug.Log("KerbTrack: initialize OVR");
		wrapper = wrapper_init();
		Debug.Log("KerbTrack: start sensor");
		wrapper_start_sensor(wrapper);
        Debug.Log("KerbTrack: OVR initialized");
	}
	public Vector3d getRotation()
	{
		Vector3f v = wrapper_get_rotation(wrapper);
		return new Vector3d(v.x * 100, v.y * 100, v.z * 100);
	}
	public Vector3d getPosition()
	{
		Vector3f v = wrapper_get_position(wrapper);
		return new Vector3d(v.x, v.y, v.z);
	}
}
