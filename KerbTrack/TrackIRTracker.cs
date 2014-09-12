using System;
using UnityEngine;
using System.Collections;
using TrackIRUnity;

class TrackIRTracker : ITracker
{
	TrackIRUnity.TrackIRClient trackIRclient;

	public TrackIRTracker()
    {
        Debug.Log("KerbTrack: Initialising TrackIR...");
		trackIRclient = new TrackIRUnity.TrackIRClient();
		string status = trackIRclient.TrackIR_Enhanced_Init();
		Debug.Log("KerbTrack: TrackIR status: " + status);
	}

    public void GetData(ref Vector3 rot, ref Vector3 pos)
    {
        if (trackIRclient != null)
        {
            TrackIRClient.LPTRACKIRDATA data = trackIRclient.client_HandleTrackIRData();
            rot.x = -data.fNPPitch / 100;
            rot.y = data.fNPYaw / 100;
            rot.z = data.fNPRoll / 100;

            pos.x = -data.fNPX / 10000;
            pos.y = data.fNPY / 10000;
            pos.z = data.fNPZ / 10000;
        }
    }

	public Vector3d GetRotation()
	{
		Vector3d rot = new Vector3d(0, 0, 0);
		if (trackIRclient != null)
		{
			TrackIRClient.LPTRACKIRDATA data = trackIRclient.client_HandleTrackIRData();
			rot.x = -data.fNPPitch / 100;
			rot.y = data.fNPYaw / 100;
			rot.z = data.fNPRoll / 100;
		}
		return rot;
	}

	public Vector3d GetPosition()
	{
		Vector3d pos = new Vector3d(0, 0, 0);
		if (trackIRclient != null)
		{
			TrackIRClient.LPTRACKIRDATA data = trackIRclient.client_HandleTrackIRData();
			pos.x = -data.fNPX / 10000;
			pos.y = data.fNPY / 10000;
			pos.z = data.fNPZ / 10000;
		}
		return pos;
	}

    public void ResetOrientation() { }
}

