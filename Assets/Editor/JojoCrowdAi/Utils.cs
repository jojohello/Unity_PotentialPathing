using UnityEngine;
using System.Collections;
using UnityEditor;

namespace JojoCrowdAi
{
    public class Utils
    {
        public static void DrawLine(Vector2 startPoint, Vector3 endPoint, Color color)
        {
            Handles.BeginGUI();
            Handles.color = color;
            Handles.DrawLine(startPoint, endPoint);
            Handles.EndGUI();
        }
    }
}

