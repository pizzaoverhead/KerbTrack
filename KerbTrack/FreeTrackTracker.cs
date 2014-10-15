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


    /* DaMichel: sometimes FaceTrackNoIR with the point tracker returns all zeros in
     * the angles. I see no other way but to check the numbers and return the last
     * "good" value. Therefore we store the last good value here. 
     */
    private Vector3 rot;
    private Vector3 pos;

    private void Update()
    {
        FreeTrackData trackData = new FreeTrackData();
        if (FTGetData(ref trackData))
        {
            // additional check if something went wrong
            if (!(trackData.RawPitch==0 && trackData.RawRoll==0 && trackData.RawYaw==0))
            {
                rot.x = trackData.Pitch * 100;
                rot.y = trackData.Yaw * 100;
                rot.z = trackData.Roll * 100;
                pos.x = trackData.X / 100;
                pos.y = trackData.Y / 100;
                pos.z = trackData.Z / 100;
            }
        }
    }

    public void GetData(ref Vector3 rot_, ref Vector3 pos_)
    {
        Update();
        rot_ = rot;
        pos_ = pos;
    }

    // DaMichel: no code duplication.
    public Vector3d GetRotation()
    {
        Update();
        return rot;
    }
    public Vector3d GetPosition()
    {
        Update();
        return pos;
    }

    public void ResetOrientation() { }
}
