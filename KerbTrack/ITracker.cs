using UnityEngine;

public interface ITracker
{
    void GetData(ref Vector3 rot, ref Vector3 pos);

    // Separate reads of rotation and position cause FreeTrack to show no data for the second read.
    //Vector3d GetRotation();
    //Vector3d GetPosition();

    void ResetOrientation();

    void Stop();
}

// Only use this if your rotation data is authoritive!
public interface IQuatTracker
{
    void GetQuatData(ref Quaternion rot);
}
