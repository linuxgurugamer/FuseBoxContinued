using System;
using KSP.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KSP.UI.Screens;
using TrackResource;

namespace Ratzap
{
    partial class FuseBox_Core : MonoBehaviour
    {
        public void Update()
        {
            sumDelta += Time.deltaTime;  // Only update FB 3 times a second max.

            if (sumDelta > 0.33)
            {
                doOneFrame = true;
                sumDelta = 0;
            }
        }

        public void checkWinBounds()
        {

            // Make sure the windows  aren't dragged off screen
            if (mainWin.x < 0)
                mainWin.x = 0;
            if (mainWin.y < 0)
                mainWin.y = 0;
            if (mainWin.x > Screen.width - mainWidth)
                mainWin.x = Screen.width - mainWidth;
            if (mainWin.y > Screen.height - mainHeight)
                mainWin.y = Screen.height - mainHeight;
            if (filterWin.x < 0)
                filterWin.x = 0;
            if (filterWin.y < 0)
                filterWin.y = 0;
            if (filterWin.x > Screen.width - filtWidth)
                filterWin.x = Screen.width - filtWidth;
            if (filterWin.y > Screen.height - filtHeight)
                filterWin.y = Screen.height - filtHeight;
            if (setWin.x < 0)
                setWin.x = 0;
            if (setWin.y < 0)
                setWin.y = 0;
            if (setWin.x > Screen.width - setWidth)
                setWin.x = Screen.width - setWidth;
            if (setWin.y > Screen.height - setHeight)
                setWin.y = Screen.height - setHeight;
            // check if win needs to shrink
            if (shrinkMain)
            {
                shrinkMain = false;
                mainWin.height = mainHeight;
            }
        }

        public void displayWindows()
        {
            if (uiActive)
            {
                checkWinBounds();
                if (useSmokeSkin)
                    GUI.skin = UnityEngine.GUI.skin;
                else
                    GUI.skin = HighLogic.Skin;

                if (am_max > 0)
                {
                    mainWin = GUILayout.Window(MAINWINID, mainWin, drawFusebox, "Fusebox", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                    if (showFilters)
                        filterWin = GUILayout.Window(FILTWINID, filterWin, drawFilters, "Filters", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                    if (showSettings)
                        setWin = GUILayout.Window(SETWINID, setWin, drawSettings, "Settings", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                    if (showDarkTime)
                    {
                        drkWin = GUILayout.Window(DRKWINID, drkWin, drawDark, "Darkness Time", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                        if (pickBod)
                        {
                            GUIStyle listStyle = new GUIStyle(GUI.skin.label);
                            listStyle.fontStyle = FSGlobal;
                            listStyle.normal.textColor = OtherCol;
                            listStyle.hover.textColor = Color.yellow;
                            listStyle.onHover.background =
                            listStyle.hover.background = new Texture2D(2, 2);
                            listStyle.padding.top = 1;
                            listStyle.padding.bottom = 1;

                            pickShow = true;
                            pickBod = !Popup.List(popupWin, ref pickShow, ref pickedBodIdx, new GUIContent("Pick Body"), allBodNames, "button", "box", listStyle);
                            if (allBodNames[pickedBodIdx] != pickedBod)
                                pickedBod = allBodNames[pickedBodIdx];
                        }
                    }
                }
            }
            else
            {
                if (mainWin.height > mainHeight)
                    mainWin.height = mainHeight;
            }
        }



        private void drawFusebox(int windowID)
        {

            // Draw a window with charge details if charge > 0

            GUIStyle FBOKStyle = new GUIStyle(GUI.skin.label);
            FBOKStyle.fontStyle = FSGlobal;
            FBOKStyle.alignment = TextAnchor.UpperLeft;
            FBOKStyle.normal.textColor = GoodCol;

            GUIStyle FBDrainStyle = new GUIStyle(GUI.skin.label);
            FBDrainStyle.fontStyle = FSGlobal;
            FBDrainStyle.alignment = TextAnchor.UpperLeft;
            FBDrainStyle.normal.textColor = BadCol;

            if (am_cur > am_max)
                am_cur = am_max;

            GUILayout.BeginVertical();
            GUILayout.Label(string.Concat("Bat max: ", am_max.ToString("F0")), FBOKStyle, GUILayout.ExpandWidth(true));

            String precString = "F" + decPlaces.ToString();

            if (am_use > am_prod)
            {
                TimeSpan drainTime = TimeSpan.FromSeconds((int)(am_cur / (am_use - am_prod)));
                int years = 0;
                if (!wasDraining)
                {
                    wasDraining = true;
                    frmCount = 1;
                }

                if (am_cur == 0)
                    GUILayout.Label("Bat cur: 0.0%", FBDrainStyle, GUILayout.ExpandWidth(true));
                else
                    GUILayout.Label(string.Concat("Bat cur: ", (am_cur / am_max).ToString("0.0%")), FBDrainStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label(string.Concat("Gen: ", am_prod.ToString(precString)), FBOKStyle, GUILayout.ExpandWidth(true));
                GUILayout.Label(string.Concat("Drain: ", am_use.ToString(precString)), FBDrainStyle, GUILayout.ExpandWidth(true));

                //GUILayout.Label(string.Concat("Gen2: ", (am_prod2 / timeIntervalMeasured).ToString(precString)), FBOKStyle, GUILayout.ExpandWidth(true));
                //GUILayout.Label(string.Concat("Drain2: ", (am_use2 / timeIntervalMeasured).ToString(precString)), FBDrainStyle, GUILayout.ExpandWidth(true));



                if (drainTime.Days > 365)
                    years = (int)drainTime.Days / 365;
                if (drainTime.Days > 0)
                    if (drainTime.Days > 365)
                    {
                        GUILayout.Label(string.Concat("0 in ", string.Format("{0:D2}y:{1:D2}d", years, (drainTime.Days - (years * 365)))), FBDrainStyle, GUILayout.ExpandWidth(true));
                    }
                    else
                    {
                        GUILayout.Label(string.Concat("0 in ", string.Format("{0:D2}d:{1:D2}h", drainTime.Days, drainTime.Hours)), FBDrainStyle, GUILayout.ExpandWidth(true));
                    }
                else if (drainTime.Hours > 0)
                    GUILayout.Label(string.Concat("0 in ", string.Format("{0:D2}h:{1:D2}m", drainTime.Hours, drainTime.Minutes)), FBDrainStyle, GUILayout.ExpandWidth(true));
                else if (drainTime.Minutes > 0)
                    GUILayout.Label(string.Concat("0 in ", string.Format("{0:D2}m:{1:D2}s", drainTime.Minutes, drainTime.Seconds)), FBDrainStyle, GUILayout.ExpandWidth(true));
                else if (drainTime.Seconds > 0)
                    GUILayout.Label(string.Concat("0 in ", string.Format("{0:D2}s", drainTime.Seconds)), FBDrainStyle, GUILayout.ExpandWidth(true));
                else
                    GUILayout.Label("Empty!", FBDrainStyle, GUILayout.ExpandWidth(true));
            }
            else
            {
                //				isCharging = am_max > am_cur;
                if (am_max == am_cur)
                    GUILayout.Label("Bat cur: 100%", FBOKStyle);
                else if (am_cur == 0)
                    GUILayout.Label("Bat cur: 0%", FBOKStyle);
                else
                    GUILayout.Label("Bat cur: " + (am_cur / am_max).ToString("0.0%"), FBOKStyle);
                GUILayout.Label("Gen: " + am_prod.ToString(precString), FBOKStyle);
                GUILayout.Label("Drain: " + am_use.ToString(precString), FBOKStyle);

                //GUILayout.Label("Gen2: " + (am_prod2 / timeIntervalMeasured).ToString(precString), FBOKStyle);
                //GUILayout.Label("Drain2: " + (am_use2 / timeIntervalMeasured).ToString(precString), FBOKStyle);


                if (wasDraining)
                {
                    wasDraining = false;
                    shrinkMain = true;
                    frmCount = 1;
                }
            }

            if (haltTriggered && (((am_cur / am_max) * 100) > haltWarpThresh))
                haltTriggered = false;

            //			Debug.Log("FB - " + minimize.ToString() + " " + mainWin.x.ToString() + " " + mainWin.y.ToString() + " " + mainWin.width.ToString() + " " + mainWin.height.ToString());

            //			GUILayout.BeginHorizontal();
            //			// Draw the filter button
            if (GUILayout.Button("Filter"))
                showFilters = !showFilters;
            //			
            // Draw the settings button
            if (GUILayout.Button("Settings"))
                showSettings = !showSettings;

            // Draw the dark time button
            if (GUILayout.Button("Dark Time"))
                showDarkTime = !showDarkTime;
            //
            //			GUILayout.EndHorizontal();
            //			
            GUILayout.EndVertical();
            if (GUILayout.Button("Close"))
                uiActive = false;
            //
            // Make window draggable
            GUI.DragWindow();
        }

        HashSet<string> filterList = new HashSet<string>();

        bool[] typeArrCopy;
        private void drawFilters(int windowID)
        {
            typeArrCopy = (bool[])typeArr.Clone();

            GUIStyle filterOff = new GUIStyle(GUI.skin.toggle);
            filterOff.normal.textColor = filterOff.onNormal.textColor = BadCol;

            GUIStyle filterOn = new GUIStyle(filterOff);
            filterOn.normal.textColor = filterOn.onNormal.textColor = GoodCol;

            // Draw window and populate with type filter list and toggles

            GUILayout.BeginVertical();

            typeArr[0] = GUILayout.Toggle(typeArr[0], "Solar panels", typeArr[0] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            typeArr[1] = GUILayout.Toggle(typeArr[1], "RTG/Gens", typeArr[1] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            typeArr[2] = GUILayout.Toggle(typeArr[2], "Vehicle Wheels", typeArr[2] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            typeArr[3] = GUILayout.Toggle(typeArr[3], "Pods/probes", typeArr[3] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            typeArr[4] = GUILayout.Toggle(typeArr[4], "Lights", typeArr[4] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            typeArr[5] = GUILayout.Toggle(typeArr[5], "Antennae", typeArr[5] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            typeArr[6] = GUILayout.Toggle(typeArr[6], "Reaction Wheels", typeArr[6] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            typeArr[7] = GUILayout.Toggle(typeArr[7], "Engines", typeArr[7] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            typeArr[8] = GUILayout.Toggle(typeArr[8], "Alternator", typeArr[8] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            typeArr[9] = GUILayout.Toggle(typeArr[9], "Misc", typeArr[9] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            typeArr[10] = GUILayout.Toggle(typeArr[10], "Clamps", typeArr[10] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (RT2Present)
                typeArr[11] = GUILayout.Toggle(typeArr[11], "RemT2 Antennae", typeArr[11] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (NFEPresent || NFSPresent)
                typeArr[12] = GUILayout.Toggle(typeArr[12], "Near Future", typeArr[12] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (ScSPresent)
                typeArr[13] = GUILayout.Toggle(typeArr[13], "SCANsat", typeArr[13] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (TACLPresent)
                typeArr[14] = GUILayout.Toggle(typeArr[14], "TAC Life Sup", typeArr[14] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            

            //            if (KISPPresent)
            //                typeArr[15] = GUILayout.Toggle(typeArr[15], "Interstellar", typeArr[15] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)); 
            if (KASPresent)
                typeArr[16] = GUILayout.Toggle(typeArr[16], "KAS", typeArr[16] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (DeepFreezePresent)
            	typeArr[17] = GUILayout.Toggle(typeArr[17], "Deep Freeze", typeArr[18] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            //			if (KarPresent)
            //				typeArr[18] = GUILayout.Toggle(typeArr[18], "Karbonite", typeArr[18] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (KSPWheelPresent)
                typeArr[19] = GUILayout.Toggle(typeArr[19], "Repulsors", typeArr[18] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            bool b = false;
            for (int i = 0; i < 20; i++)
                if (typeArr[i] != typeArrCopy[i])
                {
                    b = true;
                    break;
                }
            //if (typeArrCopy.SequenceEqual(typeArr))
            if (b)
            {
                Log.Info("typeArr changed");
                //
                // Build a list of all modules to ignore
                //
                filterList.Clear();
                if (!typeArr[0]) filterList.Add("ModuleDeployableSolarPanel");
                if (!typeArr[1])
                {
                    filterList.Add("ModuleGenerator");
                    filterList.Add("ModuleResourceConverter");
                    filterList.Add("FissionReactor");
                    filterList.Add("ModuleResourceHarvester");
                }
                if (!typeArr[2])
                {
                    filterList.Add("ModuleWheel");
                    filterList.Add("KSPWheelMotor");                    
                }
                if (!typeArr[3])
                {
                    filterList.Add("ModuleCommand");
                    //filterList.Add("BTSMModuleProbePower");
                }
                if (!typeArr[4])
                {
                    filterList.Add("ModuleLight");
                    filterList.Add("ModuleNavLight");
                }
                if (!typeArr[5])
                {
                    filterList.Add("ModuleDataTransmitter");
                    filterList.Add("ModuleLimitedDataTransmitter");
                    // filterList.Add("TelemachusPowerDrain");
                    filterList.Add("kOSProcessor");
                }
                if (!typeArr[6]) filterList.Add("ModuleReactionWheel");
                if (!typeArr[7])
                {
                    filterList.Add("ModuleEngines");
                    filterList.Add("ModuleEnginesFX");
                }
                if (!typeArr[8])
                {
                    filterList.Add("ModuleAlternator");
                    filterList.Add("KFAPUController");
                }
                if (!typeArr[9])
                {
                    filterList.Add("ModuleScienceLab");
                    //filterList.Add("Biomatic");
                }
                //if (!typeArr[10]) filterList.Add("");
                if (!typeArr[11]) filterList.Add("ModuleRTAntenna");
                if (!typeArr[12])
                {
                    filterList.Add("ModuleRadioisotopeGenerator");
                    filterList.Add("ModuleCurvedSolarPanel");
                    filterList.Add("FissionReactor");
                    filterList.Add("FissionGenerator");
                    filterList.Add("DischargeCapacitor");

                }
                if (!typeArr[13])
                {
                    filterList.Add("SCANsat");
                    filterList.Add("ModuleSCANresourceScanner");
                }
                if (!typeArr[14]) filterList.Add("TacGenericConverter");
                //if (!typeArr[15]) filterList.Add("");
                if (!typeArr[16])
                {
                    filterList.Add("KASModuleWinch");
                    filterList.Add("KASModuleMagnet");
                }
                if (!typeArr[17]) filterList.Add("DeepFreezer");
                //if (!typeArr[18]) filterList.Add("USI_ResourceConverter");
                if (!typeArr[19]) filterList.Add("KSPWheelRepulsor");
                if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)  // Check if in flight
                {
                    // foreach (var s in filterList)
                    //     Log.Info("Filtering: " + s);

                    VesselStatsManager.Instance.SetFilterList(filterList);
                }

            }

            if (mode == DisplayMode.editor)
            {
                GUILayout.Label(String.Format("Engine thr {0:P0}", currThrottle));
                currThrottle = GUILayout.HorizontalSlider(currThrottle, 0.0F, 1.0F);
                if (b)
                    updateAmValues();
            }

            if (GUILayout.Button("Close"))
                showFilters = false;

            GUILayout.EndVertical();

            // Make window draggable
            GUI.DragWindow();
        }

        private void drawSettings(int windowID)
        {
            GUIStyle filterOff = new GUIStyle(GUI.skin.toggle);
            filterOff.normal.textColor = filterOff.onNormal.textColor = Color.black;

            GUIStyle filterOn = new GUIStyle(filterOff);
            filterOn.normal.textColor = filterOn.onNormal.textColor = OtherCol;

            GUIStyle TextInp = new GUIStyle(GUI.skin.label);
            TextInp.fontStyle = FSGlobal;
            TextInp.alignment = TextAnchor.UpperLeft;

            // Draw window and populate with skin/toolbar choices
            //            	useksp2 = GUILayout.Toggle(useksp2, "Black opaque", useksp2 ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            //				if (useksp2)
            //				{
            //					useksp1 = false;
            //					skinChange = true;
            //				}
            //		
            //            	useksp1 = GUILayout.Toggle(useksp1, "Grey stock", useksp1 ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            //				if (useksp1)
            //				{
            //					useksp2 = false;
            //					skinChange = true;
            //				}

            useSmokeSkin = GUILayout.Toggle(useSmokeSkin, "Use Smoke Skin");

            haltWarp = GUILayout.Toggle(haltWarp, "Warp halt toggle", haltWarp ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (haltWarp)
            {
                float f = haltWarpThresh;
                GUILayout.Label("Emergency Warp threshold Halt: " + haltWarpThresh.ToString() + "%");
                f = GUILayout.HorizontalSlider(f, 5, 75);
                haltWarpThresh = (int)(f + 0.5f);
#if false
                GUILayout.Label("%age halt. 5-75", TextInp);
                string strThresh = haltWarpThresh.ToString("F2");
                int tempThresh = haltWarpThresh;
                strThresh = GUILayout.TextField(strThresh, 2);
                if (int.TryParse(strThresh, out tempThresh))
                {
                    if (tempThresh >= 5 && tempThresh <= 75)
                        haltWarpThresh = tempThresh;
                }
                else if (strThresh == "") haltWarpThresh = 30;
#endif
            }

            showCharge = GUILayout.Toggle(showCharge, "Show charge icon", showCharge ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            // Decimal places with +/-
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-"))
            {
                if (decPlaces > 2)
                    decPlaces--;
            }
            GUILayout.Label(string.Format(" {0:D1} Dec ", decPlaces));
            if (GUILayout.Button("+"))
            {
                if (decPlaces < 7)
                    decPlaces++;
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Close"))
                showSettings = false;
            // Make window draggable
            GUI.DragWindow();
        }

        private void drawDark(int windowID)
        {
            GUIStyle darkStyle = new GUIStyle(GUI.skin.label);
            darkStyle.fontStyle = FSGlobal;
            darkStyle.alignment = TextAnchor.UpperLeft;
            darkStyle.normal.textColor = OtherCol;

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(pickedBod);
            // Draw the body select button
            if (GUILayout.Button("Pick"))
            {
                popupWin.x = drkWin.x - 120;
                popupWin.y = drkWin.y + 120;
                pickBod = !pickBod;
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("Orb alt km:", darkStyle);
            string strOrbit = celOrbit.ToString("D");
            int tempOrbit = celOrbit;
            strOrbit = GUILayout.TextField(strOrbit, 6, darkStyle);
            if (int.TryParse(strOrbit, out tempOrbit))
            {
                if (tempOrbit > 1 && tempOrbit != celOrbit)
                    celOrbit = tempOrbit;
            }
            else if (strOrbit == "") celOrbit = 1;

            TimeSpan darkTime = GameSettings.KERBIN_TIME ? ConvToKerb(TimeSpan.FromSeconds((int)getDarkTime())) : TimeSpan.FromSeconds((int)getDarkTime());
            string darkStr = "";
            if (darkTime.Days > 0)
            {
                darkStr = string.Format("{0:D2}d:{1:D2}h", darkTime.Days, darkTime.Hours);
            }
            else if (darkTime.Hours > 0)
            {
                darkStr = string.Format("{0:D2}h:{1:D2}m", darkTime.Hours, darkTime.Minutes);
            }
            else if (darkTime.Minutes > 0)
            {
                darkStr = string.Format("{0:D2}m:{1:D2}s", darkTime.Minutes, darkTime.Seconds);
            }
            else
                darkStr = darkTime.Seconds.ToString() + "s";
            GUILayout.Label("Dark time: " + darkStr, darkStyle);

            TimeSpan darkTimeLanded = GameSettings.KERBIN_TIME ? ConvToKerb(TimeSpan.FromSeconds((int)getDarkTimeLanded())) : TimeSpan.FromSeconds((int)getDarkTimeLanded());
            if (darkTimeLanded.Days > 0)
            {
                darkStr = string.Format("{0:D2}d:{1:D2}h", darkTimeLanded.Days, darkTimeLanded.Hours);
            }
            else if (darkTimeLanded.Hours > 0)
            {
                darkStr = string.Format("{0:D2}h:{1:D2}m", darkTimeLanded.Hours, darkTimeLanded.Minutes);
            }
            else if (darkTime.Minutes > 0)
            {
                darkStr = string.Format("{0:D2}m:{1:D2}s", darkTimeLanded.Minutes, darkTimeLanded.Seconds);
            }
            else
                darkStr = darkTimeLanded.Seconds.ToString() + "s";

            GUILayout.Label("Land Dark: " + darkStr, darkStyle);

            if (GUILayout.Button("Close"))
                showDarkTime = false;

            GUILayout.EndVertical();

            // Make window draggable
            GUI.DragWindow();
        }


    }
}
