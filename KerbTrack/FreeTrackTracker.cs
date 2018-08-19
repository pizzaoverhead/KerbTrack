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

    private Vector3 rotationState;
    private Vector3 positionState;

    private void GetRawData()
    {
        FreeTrackData trackData = new FreeTrackData();
        if (FTGetData(ref trackData))
        {
            // FaceTrackNoIR occasionally returns values of all zero, causing jumping.
            // Ignore zeroed values.
            if (!(trackData.RawPitch == 0 && trackData.RawRoll == 0 && trackData.RawYaw == 0))
            {
                rotationState.x = trackData.Pitch * 100;
                rotationState.y = trackData.Yaw * 100;
                rotationState.z = trackData.Roll * 100;
                positionState.x = trackData.X / 1000;
                positionState.y = trackData.Y / 1000;
                positionState.z = trackData.Z / 1000;
            }
        }
    }
    public void GetData(ref Vector3 rot, ref Vector3 pos)
    {
        GetRawData();
        rot = rotationState;
        pos = positionState;
    }

    public Vector3d GetRotation()
    {
        GetRawData();
        return rotationState;
    }
    public Vector3d GetPosition()
    {
        GetRawData();
        return positionState;
    }

    public void ResetOrientation() { }
    public void Stop() { }
}
