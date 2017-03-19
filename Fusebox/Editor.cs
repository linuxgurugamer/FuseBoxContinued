using System;
using KSP.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KSP.UI.Screens;


// Ratzap, 09/09/13

namespace Ratzap
{
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class Fusebox_Editor : FuseBox_Core
    {
        bool initted = false;

        new private void Start()
        {
            Log.Info("Fusebox_Editor.Start");
            GameEvents.onEditorShipModified.Add(onEditorShipModified);
            base.Start();
        }

        new private void OnDestroy()
        {
            Log.Info("Fusebox_Editor.OnDestroy");
            GameEvents.onEditorShipModified.Remove(onEditorShipModified);
            base.OnDestroy();
        }

        public void onEditorShipModified(ShipConstruct construct)
        {
            Log.Info("Fusebox_Editor.onEditorShipModified");

            updateAmValues();
        }

        private void OnGUI()
        {
            if (globalHidden)
                return;
            if (!initted)
            {
                initted = true;
                updateAmValues();               
            }
            checkMode(DisplayMode.editor);
            if (doOneFrame)
            {
                // updateAmValues();
                //if (ToolbarManager.ToolbarAvailable)
                    setTBIcon();
                doOneFrame = !doOneFrame;
            }

            displayWindows();
        }
    }
}