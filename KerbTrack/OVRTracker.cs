using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class OVRTracker : ITracker, IQuatTracker
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3f
    {
        public float x, y, z;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct Quat
    {
        public float x, y, z, w;
    }

#if WIN32
    public const string DllName = "Oculus OVR PosRotWrapper";
#else
    public const string DllName = "Oculus OVR PosRotWrapper 64-bit";
#endif

    [DllImport(DllName)]
    public static extern IntPtr wrapper_init();
    [DllImport(DllName)]
    public static extern void wrapper_destroy(IntPtr ptr);
    [DllImport(DllName)]
    public static extern void wrapper_start_sensor(IntPtr ptr);
    [DllImport(DllName)]
    public static extern Vector3f wrapper_get_rotation(IntPtr ptr);
    [DllImport(DllName)]
    public static extern Vector3f wrapper_get_position(IntPtr ptr);
    [DllImport(DllName)]
    public static extern Quat wrapper_get_quat_rotation(IntPtr ptr);
    [DllImport(DllName)]
    public static extern void wrapper_reset_orientation(IntPtr ptr);

    public IntPtr wrapper;

    public OVRTracker()
    {
        Debug.Log("[KerbTrack] Initialising OVR...");
        wrapper = wrapper_init();
        wrapper_start_sensor(wrapper);
        Debug.Log("[KerbTrack] OVR initialised");
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

    public void GetQuatData(ref Quaternion res)
    {
        Quat quat = wrapper_get_quat_rotation(wrapper);
        // What the hell?
        res.x = -quat.x;
        res.y = -quat.y;
        res.z = quat.z;
        res.w = quat.w;
    }

    public void ResetOrientation()
    {
        wrapper_reset_orientation(wrapper);
    }

    public void Stop() { }
}
