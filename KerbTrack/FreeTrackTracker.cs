using System;
using System.Runtime.InteropServices;
using UnityEngine;

class FreeTrackTracker : ITracker
{
	[StructLayout(LayoutKind.Sequential)]
	public struct FreeTrackData
	{
		public int dataid;
		public int camwidth, camheight;
		public float Yaw, Pitch, Roll, X, Y, Z;
		public float RawYaw, RawPitch, RawRoll;
		public float RawX, RawY, RawZ;
		public float x1, y1, x2, y2, x3, y3, x4, y4;
	}

#if WIN32
	[DllImport("FreeTrackClient")]
	public static extern bool FTGetData(ref FreeTrackData data);
	[DllImport("FreeTrackClient")]
	public static extern string FTGetDllVersion();
	[DllImport("FreeTrackClient")]
	public static extern void FTReportID(Int32 name);
	[DllImport("FreeTrackClient")]
	public static extern string FTProvider();
#else
    [DllImport("FreeTrackClient64")]
    public static extern bool FTGetData(ref FreeTrackData data);
    [DllImport("FreeTrackClient64")]
    public static extern string FTGetDllVersion();
    [DllImport("FreeTrackClient64")]
    public static extern void FTReportID(Int32 name);
    [DllImport("FreeTrackClient64")]
    public static extern string FTProvider();
#endif

    public void GetData(ref Vector3 rot, ref Vector3 pos)
    {
        FreeTrackData trackData = new FreeTrackData();
        if (FTGetData(ref trackData))
        {
            rot.x = trackData.Pitch * 100;
            rot.y = trackData.Yaw * 100;
            rot.z = trackData.Roll * 100;
            pos.x = trackData.X / 100;
            pos.y = trackData.Y / 100;
            pos.z = trackData.Z / 100;
        }
    }

	public Vector3d GetRotation()
	{
		Vector3d rot = new Vector3d(0, 0, 0);
		FreeTrackData trackData = new FreeTrackData();
		if (FTGetData(ref trackData))
		{
			rot.x = trackData.Pitch * 100;
			rot.y = trackData.Yaw * 100;
			rot.z = trackData.Roll * 100;
		}
		return rot;
	}
	public Vector3d GetPosition()
	{
		Vector3d pos = new Vector3d(0, 0, 0);
		FreeTrackData trackData = new FreeTrackData();
		if (FTGetData(ref trackData))
		{
			pos.x = trackData.X / 100;
			pos.y = trackData.Y / 100;
			pos.z = trackData.Z / 100;
		}
		return pos;
	}

    public void ResetOrientation() { }
}
