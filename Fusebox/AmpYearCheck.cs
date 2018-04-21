using KSP.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KSP.UI.Screens;

using ClickThroughFix;

namespace Ratzap
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class AmpYearCheck : MonoBehaviour
    {
        const int ampYearWidth = 300;    // ampYear window width
        const int ampYearHeight = 200;   // ampYear window height
        Rect ampYearWin = new Rect((Screen.width - ampYearWidth) / 2, (Screen.height - ampYearHeight) / 2, ampYearWidth, ampYearHeight);  // ampYear window position and size
        bool ampYearDetected = false;
        int ampYearWINID = 2384576;
        GUIStyle window;

        public void Start()
        {
            if (AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "AmpYear"))
            {
                Log.Info("AmpYear detected, disabling FuseBox");
                ampYearDetected = true;
                window = new GUIStyle(HighLogic.Skin.window);
                //window.normal.background.SetPixels( new[] { new Color(0.5f, 0.5f, 0.5f, 1f) });
                window.active.background = window.normal.background;

                Texture2D tex = window.normal.background; //.CreateReadable();

                var pixels = tex.GetPixels32();

                for (int i = 0; i < pixels.Length; ++i)
                    pixels[i].a = 255;

                tex.SetPixels32(pixels); tex.Apply();

                // one of these apparently fixes the right thing
                // window.onActive.background =
                // window.onFocused.background =
                // window.onNormal.background =
                //window.onHover.background =
                window.active.background =
                window.focused.background =
                //window.hover.background =
                window.normal.background = tex;
            }
        }

        private void OnGUI()
        {
            GUI.color = Color.grey;
            if (ampYearDetected)
                ampYearWin = ClickThruBlocker.GUILayoutWindow(ampYearWINID, ampYearWin, drawAmpyearbox, "Fusebox", window);
        }

        void drawHLine(string s)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(s);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        private void drawAmpyearbox(int windowID)
        {
            GUILayout.BeginVertical();

            drawHLine("FuseBox is incompatible with AmpYear");
            drawHLine("AmpYear has been detected");

            drawHLine("Please remove either FuseBox");
            drawHLine("or AmpYear before proceeding");

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("OK"))
                ampYearDetected = false;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}