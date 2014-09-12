using System;
using UnityEngine;
using System.Runtime.InteropServices;

class OVRTracker : ITracker
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector3f {
		public float x, y, z;
	}

#if WIN32
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
#else
    [DllImport("Oculus OVR PosRotWrapper 64-bit")]
    public static extern IntPtr wrapper_init();
    [DllImport("Oculus OVR PosRotWrapper 64-bit")]
    public static extern void wrapper_destroy(IntPtr ptr);
    [DllImport("Oculus OVR PosRotWrapper 64-bit")]
    public static extern void wrapper_start_sensor(IntPtr ptr);
    [DllImport("Oculus OVR PosRotWrapper 64-bit")]
    public static extern Vector3f wrapper_get_rotation(IntPtr ptr);
    [DllImport("Oculus OVR PosRotWrapper 64-bit")]
    public static extern Vector3f wrapper_get_position(IntPtr ptr);
#endif
	
	public IntPtr wrapper;

	public OVRTracker()
	{
		Debug.Log("KerbTrack: Initialising OVR...");
		wrapper = wrapper_init();
		wrapper_start_sensor(wrapper);
        Debug.Log("KerbTrack: OVR initialised");
	}

    public void GetData(ref Vector3 rot, ref Vector3 pos)
    {
        Vector3f r = wrapper_get_rotation(wrapper);
        rot.x = r.x * 100;
        rot.y = r.y * 100;
        rot.z = r.z * 100;
        Vector3f p = wrapper_get_position(wrapper);
        pos.x = p.x;
        pos.y = p.y;
        pos.z = p.z;
    }

	public Vector3d GetRotation()
	{
		Vector3f v = wrapper_get_rotation(wrapper);
		return new Vector3d(v.x * 100, v.y * 100, v.z * 100);
	}

	public Vector3d GetPosition()
	{
		Vector3f v = wrapper_get_position(wrapper);
		return new Vector3d(v.x, v.y, v.z);
	}

    public void ResetOrientation()
    {

    }
}
