using System;
using UnityEditor;

namespace Cosmobot.Editor
{
    public static class IMGUIDebugger {

        [MenuItem("Window/IMGUI Debugger")]
        public static void Open()
        {
            Type imguiDebugger = Type.GetType("UnityEditor.GUIViewDebuggerWindow,UnityEditor");
            EditorWindow.GetWindow(imguiDebugger).Show();
        }
    }
}