using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbTrack
{
    public class JoystickTracker : ITracker
    {
        public JoystickTracker()
        {
            Debug.Log("[KerbTrack] Initialising Joystick tracker...");
            string[] joysticks = Input.GetJoystickNames();
            Debug.Log("Joysticks available: ");
            for (int i = 0; i < joysticks.Length; i++)
            {
                Debug.Log(i + " - " + joysticks[i]);
            }
        }

        public void GetData(ref Vector3 rot, ref Vector3 pos)
        {
            if (KerbTrack.joyPitchAxisId != -1)
            {
                string pitchAxis = "joy" + KerbTrack.joystickId + "." + KerbTrack.joyPitchAxisId;
                rot.x = Input.GetAxis(pitchAxis) * 200;
                if (KerbTrack.joyPitchInverted)
                    rot.x *= -1;
            }
            if (KerbTrack.joyYawAxisId != -1)
            {
                string yawAxis = "joy" + KerbTrack.joystickId + "." + KerbTrack.joyYawAxisId;
                rot.y = Input.GetAxis(yawAxis) * 200;
                if (KerbTrack.joyYawInverted)
                    rot.y *= -1;
            }
            if (KerbTrack.joyRollAxisId != -1)
            {
                string rollAxis = "joy" + KerbTrack.joystickId + "." + KerbTrack.joyRollAxisId;
                rot.z = Input.GetAxis(rollAxis) * 200;
                if (KerbTrack.joyRollInverted)
                    rot.z *= -1;
            }
            if (KerbTrack.joyXAxisId != -1)
            {
                string xAxis = "joy" + KerbTrack.joystickId + "." + KerbTrack.joyXAxisId;
                pos.x = Input.GetAxis(xAxis);
                if (KerbTrack.joyXInverted)
                    pos.x *= -1;
            }
            if (KerbTrack.joyYAxisId != -1)
            {
                string yAxis = "joy" + KerbTrack.joystickId + "." + KerbTrack.joyYAxisId;
                pos.y = Input.GetAxis(yAxis);
                if (KerbTrack.joyYInverted)
                    pos.y *= -1;
            }
            if (KerbTrack.joyZAxisId != -1)
            {
                string zAxis = "joy" + KerbTrack.joystickId + "." + KerbTrack.joyZAxisId;
                pos.z = Input.GetAxis(zAxis);
                if (KerbTrack.joyZInverted)
                    pos.z *= -1;
            }
        }

        public void GetFlightCamData(ref Vector2 rot)
        {
            if (KerbTrack.joyCamPitchAxisId != -1)
            {
                string pitchAxis = "joy" + KerbTrack.joystickId + "." + KerbTrack.joyCamPitchAxisId;
                rot.x = Deadzone(Input.GetAxis(pitchAxis)) * 200;

                if (KerbTrack.joyCamPitchInverted)
                    rot.x *= -1;
            }
            if (KerbTrack.joyCamOrbitAxisId != -1)
            {
                string orbitAxis = "joy" + KerbTrack.joystickId + "." + KerbTrack.joyCamOrbitAxisId;
                rot.y = Deadzone(Input.GetAxis(orbitAxis)) * 200;
                if (KerbTrack.joyCamOrbitInverted)
                    rot.y *= -1;
            }
        }

        private float Deadzone(float val)
        {
            //Debug.Log("Testing " + val);
            if (val > -0.2f && val < 0.2f)
            {
                //Debug.Log("In deadzone");
                return 0f;
            }
            //Debug.Log("Out of deadzone");
            return val;
        }

        public void ResetOrientation() { }
        public void Stop() { }
    }
}
