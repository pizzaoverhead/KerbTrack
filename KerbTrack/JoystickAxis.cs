using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class JoystickAxis
{
    // The index of the joystick this axis belongs to. Valid range: 0-9, 10 joysticks supported.
    public int JoystickId;
    // The index of the joystick axis. Valid range: 0-19, 20 axes supported.
    public int AxisId;
    // Whether this axis should be reversed.
    public bool Inverted;
    // Whether the movement of this axis is relative (hold to continue turning) or absolute (axis position corresponds to camera rotation).
    public bool Absolute;

    public JoystickAxis(int joystickId, int axisId, bool inverted, bool absolute)
    {
        JoystickId = joystickId;
        AxisId = axisId;
        Inverted = inverted;
        Absolute = absolute;
    }
}
