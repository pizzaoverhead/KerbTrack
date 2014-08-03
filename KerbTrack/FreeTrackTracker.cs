﻿using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

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

	[DllImport("FreeTrackClient")]
	public static extern bool FTGetData(ref FreeTrackData data);
	[DllImport("FreeTrackClient")]
	public static extern string FTGetDllVersion();
	[DllImport("FreeTrackClient")]
	public static extern void FTReportID(Int32 name);
	[DllImport("FreeTrackClient")]
	public static extern string FTProvider();

	public Vector3d getRotation()
	{
		Vector3d rot = new Vector3d(0, 0, 0);
		FreeTrackData trackData = new FreeTrackData();
		if (FTGetData(ref trackData))
		{
			rot.x = trackData.Pitch;
			rot.y = trackData.Yaw;
			rot.z = trackData.Roll;
		}
		return rot;
	}
	public Vector3d getPosition()
	{
		Vector3d pos = new Vector3d(0, 0, 0);
		FreeTrackData trackData = new FreeTrackData();
		if (FTGetData(ref trackData))
		{
			pos.x = trackData.X;
			pos.y = trackData.Y;
			pos.z = trackData.Z;
		}
		return pos;
	}
}
