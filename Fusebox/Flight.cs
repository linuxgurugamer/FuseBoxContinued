using System;
using KSP.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KSP.UI.Screens;
using TrackResource;

using ClickThroughFix;

// Ratzap, 09/09/13

namespace Ratzap
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class FuseBox_Flight : FuseBox_Core
    {

        new private void Start()
        {
            
            base.Start();
            updateAmValues();
        }

        private void OnGUI()
        {
            if (globalHidden)
                return;
            if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)  // Check if in flight
            {
                if (FlightGlobals.ActiveVessel.isEVA) // EVA kerbal, do nothing
                    return;
                checkMode(DisplayMode.inFlight);
                checkWarp();
            }
           
            else   // Not in flight, in editor or F2 pressed unset the mode and return
            {
                checkMode(DisplayMode.none);
                return;
            }
            if (doOneFrame)
            {
                doOneFrame = !doOneFrame;

                //updateAmValues();
                timeIntervalMeasured = Planetarium.GetUniversalTime() - lastTimeCheck;
                lastTimeCheck = Planetarium.GetUniversalTime();
                Log.Info("OnGUI 1");
                ResourceStats rs = VesselStatsManager.Instance.GetOrAdd(FlightGlobals.ActiveVessel);
                Log.Info("OnGUI 2");
                if (rs != null)
                {
                    Log.Info("OnGUI 3");
                    am_prod = -rs.GetGeneration("ElectricCharge") / timeIntervalMeasured;
                    am_use = rs.GetConsumption("ElectricCharge") / timeIntervalMeasured;
                    Log.Info("OnGUI.GetTotalConsumption: " + (rs.GetConsumption("ElectricCharge") / timeIntervalMeasured).ToString());
                    Log.Info("OnGUI: " + (rs.GetGeneration("ElectricCharge") / timeIntervalMeasured).ToString());
                    rs.Reset();
                }
                Log.Info("OnGUI 4");

                checkWinBounds();

               
                //if (ToolbarManager.ToolbarAvailable)
                    setTBIcon();
            }
            
            displayWindows();

        }



        private void checkWarp()
        {
            if (!haltTriggered)
            {
                if ((TimeWarp.CurrentRate > 0) && haltWarp && (((am_cur / am_max) * 100) < haltWarpThresh))
                {
                    TimeWarp.SetRate(0, true);
                    showWarn = true;
                    warnRead = false;
                    warnPopped = true;
                    haltWarp = false;
                }
            }
            if (showWarn)
            {
                warnWin = ClickThruBlocker.GUILayoutWindow(WARWINID, warnWin, stopAndWarn, "WARNING!", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            }
            else
            {
                if (warnRead && haltTriggered)
                    haltWarp = true;
            }
            showWarn = (warnPopped && !warnRead);
        }

        private void stopAndWarn(int windowID)
        {
            GUIStyle WarnStyle = new GUIStyle(GUI.skin.label);
            WarnStyle.fontStyle = FSGlobal;
            WarnStyle.alignment = TextAnchor.UpperLeft;
            WarnStyle.normal.textColor = BadCol;

            GUILayout.Label("Your ships charge has dropped below the Warp Halt threshold", WarnStyle);
            GUILayout.Label("If this is unavoidable, switch halting off in Fusebox settings", WarnStyle);
            GUILayout.Label("Or change the threshold to a different level. (5% to 75%)", WarnStyle);
            GUILayout.Label("This will not trigger again until battery charge > threshold", WarnStyle);
            if (GUILayout.Button("OK", GUILayout.MaxWidth(40)))
            {
                warnRead = true;
                warnPopped = false;
                haltTriggered = true;
            }
        }
    }


}