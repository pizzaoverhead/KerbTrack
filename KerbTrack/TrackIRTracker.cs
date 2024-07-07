using TrackIRUnity;
using UnityEngine;
using Microsoft.Win32;

public class TrackIRTracker : ITracker
{
    TrackIRClient trackIRclient;

    public TrackIRTracker()
    {
        Debug.Log("[KerbTrack] Initialising TrackIR...");

        // TrackIRUnity's init throws a NullRef if the DLL location isn't found.
        // Check this before starting.
        bool keyFound = false;
        RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\NaturalPoint\\NATURALPOINT\\NPClient Location", false);
        if (registryKey != null && registryKey.GetValue("Path") != null)
            keyFound = true;
        registryKey.Close();

        string status;
        if (keyFound)
        {
            trackIRclient = new TrackIRUnity.TrackIRClient();
            if (trackIRclient == null)
                status = "Failed to start.";
            else
                status = trackIRclient.TrackIR_Enhanced_Init();
        }
        else
            status = "TrackIR not installed";

        Debug.Log("[KerbTrack] TrackIR status: " + status);
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

    public void ResetOrientation() { }
    public void Stop() { }
}
