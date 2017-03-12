using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbTrack
{
    public class GuiUtils
    {
        public static void Slider(string label, ref float variable, float from, float to)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label + ": " + variable.ToString());
            GUILayout.FlexibleSpace();
            variable = GUILayout.HorizontalSlider(variable, from, to, GUILayout.Width(250));
            GUILayout.EndHorizontal();
        }

        public static void SliderScale(string label, ref float variable)
        {
            Slider(label, ref variable, 0, 1);
        }

        public static void SliderOffset(string label, ref float variable)
        {
            Slider(label, ref variable, -1, 1);
        }

        public static void LabelValue(string text, object obj)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text);
            GUILayout.FlexibleSpace();
            GUILayout.Label(obj.ToString(), GUILayout.Width(100));
            GUILayout.EndHorizontal();
        }

        public static int RadioButton(string[] labels, int selectedIndex)
        {
            int newSelection = selectedIndex;
            bool selectionChanged = false;
            GUILayout.BeginHorizontal();
            for (int i = 0; i < labels.Length; i++)
            {
                bool selected = GUILayout.Toggle(i == selectedIndex && !selectionChanged, labels[i]);
                if (selected && i != selectedIndex)
                {
                    selectionChanged = true;
                    newSelection = i;
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
            return newSelection;
        }
    }
}
