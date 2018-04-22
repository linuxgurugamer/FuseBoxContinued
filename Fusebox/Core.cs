using System;
using KSP.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KSP.UI.Screens;
using TrackResource;

using ToolbarControl_NS;

namespace Ratzap
{
    public partial class FuseBox_Core : MonoBehaviour
    {
        protected static String VERSION = "1.5";  // Current version
        protected const int MAINWINID = 8631;  // Main window ID
        protected const int FILTWINID = 8632;  // Filter window ID
        protected const int SETWINID = 8633;  // Settings window ID
        protected const int WARWINID = 8634;  // Warp warning window ID
        protected const int DRKWINID = 8635;  // Darkness window ID
        protected const int POPWINID = 8636;  // Darkness body pick window ID
        protected static PluginConfiguration FBconf = null;   // Config file

        protected static int mainWidth = 120;    // Main window width
        protected static int mainHeight = 10;   // Main window height
        protected static int filtWidth = 150;    // Filt window width
        protected static int filtHeight = 10;   // Filt window height
        protected static int setWidth = 200;  // Settings window width
        protected static int setHeight = 10;    // Settings window height
        protected static int drkWidth = 160;    // Settings window width
        protected static int drkHeight = 10;    // Settings window height

        protected static Rect mainWin = new Rect(Screen.width / 2, Screen.height / 4, mainWidth, mainHeight);  // Main window position and size
        protected static Rect filterWin = new Rect(Screen.width / 2, Screen.height / 4, filtWidth, filtHeight);  // Filter window position and size
        protected static Rect setWin = new Rect(Screen.width / 2, Screen.height / 4, setWidth, setHeight);  // Filter window position and size
        protected static Rect drkWin = new Rect(Screen.width / 2, Screen.height / 4, drkWidth, drkHeight);  // Filter window position and size
        protected static Rect warnWin = new Rect(Screen.width / 2, Screen.height / 4, 460, 120);  // Warp warning window position and size
        protected static Rect popupWin = new Rect(Screen.width / 2, Screen.height / 4, 120, 23);  // Dark timer pick window position and size

        protected static bool uiActive = false;
        protected static bool useSmokeSkin;
        protected static bool doOneFrame = false;
        protected static bool shrinkMain = false;
        protected static bool showFilters = false;
        protected static bool showSettings = false;
        protected static bool showDarkTime = false;
        protected static bool globalHidden = false;

        protected static bool SLPresent = false;
        protected static bool ALPresent = false;
        protected static bool NFEPresent = false;
        protected static bool NFSPresent = false;
        protected static bool KASPresent = false;
        protected static bool RT2Present = false;
        protected static bool ScSPresent = false;
        protected static bool TelPresent = false;
        protected static bool TACLPresent = false;
        protected static bool kOSPresent = false;
        protected static bool DeepFreezePresent = false;
#if SSTU
        protected static bool SSTUToolsPresent = false;
#endif
        //		protected static bool BioPresent = false;
        protected static bool AntRPresent = false;
        //		protected static bool KarPresent = false;
        //		protected static bool BDSMPresent = false;
        protected static bool KSPWheelPresent = false;



        protected static bool wasDraining = false;
        //		protected static bool isCharging = false;
        //		protected static bool skinChange = false;
        protected static bool showCharge = true;

        protected static bool[] typeArr = new bool[20];

        public enum  DisplayMode : int { none = -1, inFlight = 0, editor = 1 };
        protected static DisplayMode mode = DisplayMode.none;

        // protected static int mode = -1;  // Display mode, currently  0 for In-Flight, 1 for Editor, -1 to hide

        protected static bool haltWarp = true;
        protected static bool haltTriggered = false;
        protected static bool warnRead = false;
        protected static bool warnPopped = false;
        protected static bool showWarn = false;
        protected static int haltWarpThresh = 30;
        protected static int decPlaces = 2;

        public static double am_max = 0;    // Bat max
        public static double am_cur = 0;    // Bat cur
        public static double am_prod = 0;   // Gen
        public static double am_use = 0;    // Drain
                                            //ublic static double am_prod2 = 0;   // Gen
                                            //ublic static double am_use2 = 0;    // Drain

        public static double timeIntervalMeasured = 0;
        public static double lastTimeCheck = 0;

        protected static double sumDelta = 0;
        protected static float currThrottle = 1.0F;

        protected static FontStyle FSGlobal = FontStyle.Normal;
        protected static Color GoodCol = Color.green;
        protected static Color BadCol = Color.red;
        protected static Color OtherCol = Color.white;

        // Toolbar stuff, cribbed from VOID
#if false
        protected static ApplicationLauncherButton appLauncherButton;
        protected static IButton ToolbarButton;
#endif
        static ToolbarControl toolbarControl;

#if false
        protected static Texture2D FB_TB_full;
        protected static Texture2D FB_TB_pos2b;
        protected static Texture2D FB_TB_pos1b;
        protected static Texture2D FB_TB_dr3b;
        protected static Texture2D FB_TB_dr2b;
        protected static Texture2D FB_TB_dr1b;
        protected static Texture2D FB_TB_empty;
        protected static Texture2D FB_TB_posgen;
        protected static Texture2D FB_TB_drain;
#endif

        protected static int frmCount = 1;
        protected string FB_TB_full_P =  "FuseboxContinued/TB_icons/3of3green";
        protected string FB_TB_pos2b_P = "FuseboxContinued/TB_icons/2of3green";
        protected string FB_TB_pos1b_P = "FuseboxContinued/TB_icons/1of3green";
        protected string FB_TB_dr3b_P = "FuseboxContinued/TB_icons/3of3red";
        protected string FB_TB_dr2b_P = "FuseboxContinued/TB_icons/2of3red";
        protected string FB_TB_dr1b_P = "FuseboxContinued/TB_icons/1of3red";
        protected string FB_TB_empty_P = "FuseboxContinued/TB_icons/emptyred";
        protected string FB_TB_posgen_P = "FuseboxContinued/TB_icons/posgen";
        protected string FB_TB_drain_P = "FuseboxContinued/TB_icons/draining";

        // Darkness calc bits
        public static List<celBody> allBodies = new List<celBody>();
        public static string[] allBodNames;
        protected static bool pickBod = false;
        protected static string pickedBod = "";
        protected static int pickedBodIdx = 3;
        protected static int celOrbit = 100;
        protected static bool pickShow = false;


        protected static List<Part> parts;
        protected static PartResourceDefinition definition;
        protected static PartResourceDefinition storedChargeDefinition;

        public void Awake()
        {
           // GameEvents.onGUIApplicationLauncherUnreadifying.Add(DestroyLauncher);
        }

        static bool initted = false;
        public void Start()
        {
            if (!initted)
            {
                initted = true;
                // Create or load config file
                FBconf = KSP.IO.PluginConfiguration.CreateForType<FuseBox_Core>(null);
                FBconf.load();
                for (int i = 0; i < typeArr.Length; i++)
                {
                    typeArr[i] = true;
                }
                if (AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "AmpYear"))
                {
                    Log.Info("AmpYear detected, disabling FuseBox");
                }

                // Find out which mods are present

                ALPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "AviationLights");
                SLPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "SurfaceLights");
                NFEPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "NearFutureElectrical");
                NFSPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "NearFutureSolar");
                KASPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "KAS");
                RT2Present = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "RemoteTech");
                ScSPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "SCANsat");
                TelPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "Telemachus");
                TACLPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "TacLifeSupport");
                AntRPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "AntennaRange");
                kOSPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "kOS");
                DeepFreezePresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "DeepFreeze");
                KSPWheelPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "KSPWheel");
#if SSTU
                SSTUToolsPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "SSTUTools");
#endif

                //			BDSMPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "BTSM");

                //			Debug.Log("FB - Checked for mods");
                //			if (ALPresent)
                //				Debug.Log("FB - Aviation Lights present");
                //			if (NFEPresent)
                //				Debug.Log("FB - Near Future Electric present");
                //			if (NFSPresent)
                //				Debug.Log("FB - Near Future Solar present");
                //			if (KASPresent)
                //				Debug.Log("FB - KAS present");
                //			if (RT2Present)
                //				Debug.Log("FB - RT2 present");
                //			if (ScSPresent)
                //				Debug.Log("FB - SCANSat present");
                //			if (TelPresent)
                //				Debug.Log("FB - Telemachus present");
                //			if (TACLPresent)
                //				Debug.Log("FB - TAC LS present");
                //			if (AntRPresent)
                //				Debug.Log("FB - AntennaRange present");
                //			if (kOSPresent)
                //				Debug.Log("FB - kOS present");
                /*			if (KISPPresent)
                                Debug.Log("FB - Interstellar present");
                            if (BioPresent)
                                Debug.Log("FB - Biomatic present");
                            if (KarPresent)
                                Debug.Log("FB - Karbonite present");
                            if (BDSMPresent)
                                Debug.Log("FB - btsm present"); */

                if (allBodies.Count == 0)
                {
                    // Get solar system bodies and add to list
                    List<CelestialBody> solSys = FlightGlobals.Bodies;
                    foreach (CelestialBody body in solSys)
                    {
                        if (body.isHomeWorld)
                            pickedBod = body.name;
                        allBodies.Add(new celBody(body.name, body.Radius / 1000, body.gravParameter / 1000000000, (int)body.rotationPeriod));
                        Log.Info("FB - " + body.name + " - " + body.Radius / 1000 + " - " + body.gravParameter / 1000000000 + " - " + (int)body.rotationPeriod);

                    }
                    Log.Info("FB - homeworld is " + pickedBod);
                    allBodNames = allBodies.Select(x => x.CelName).ToArray();
                    Array.Sort<string>(allBodNames);
                    pickedBodIdx = allBodNames.IndexOf(pickedBod);
                }

          
                definition = PartResourceLibrary.Instance.GetDefinition("ElectricCharge");
                if (NFEPresent)
                    storedChargeDefinition = PartResourceLibrary.Instance.GetDefinition("StoredCharge");
            }
            CreateLauncher();
            //Hide/show UI event addition
            GameEvents.onHideUI.Add(HideUI);
            GameEvents.onShowUI.Add(ShowUI);
        }

        protected void OnDestroy()
        {
           DestroyLauncher();
           // GameEvents.onGUIApplicationLauncherReady.Remove(CreateLauncher);
        }

        protected void CreateLauncher()
        {
            Log.Info("CreateLauncher");
#if false
            FB_TB_posgen = GameDatabase.Instance.GetTexture(FB_TB_posgen_P, false);
            if (ToolbarManager.ToolbarAvailable && HighLogic.CurrentGame.Parameters.CustomParams<Fusebox>().blizzy)
            {
                // Load toolbar icons
                FB_TB_full = GameDatabase.Instance.GetTexture(FB_TB_full_P, false);
                FB_TB_pos2b = GameDatabase.Instance.GetTexture(FB_TB_pos2b_P, false);
                FB_TB_pos1b = GameDatabase.Instance.GetTexture(FB_TB_pos1b_P, false);
                FB_TB_dr3b = GameDatabase.Instance.GetTexture(FB_TB_dr3b_P, false);
                FB_TB_dr2b = GameDatabase.Instance.GetTexture(FB_TB_dr2b_P, false);
                FB_TB_dr1b = GameDatabase.Instance.GetTexture(FB_TB_dr1b_P, false);
                FB_TB_empty = GameDatabase.Instance.GetTexture(FB_TB_empty_P, false);
                //				FB_TB_posgen = GameDatabase.Instance.GetTexture (FB_TB_posgen_P, false);
                FB_TB_drain = GameDatabase.Instance.GetTexture(FB_TB_drain_P, false);
                // init button, add icons etc
                ToolbarButton = ToolbarManager.Instance.add(this.GetType().Name, "FBToggle");
                ToolbarButton.Text = "Fusebox";
                ToolbarButton.TexturePath = this.FB_TB_full_P;
                ToolbarButton.Visible = true;
                ToolbarButton.OnClick += (
                    (e) => uiActive = !uiActive
                );
            }
            else if (appLauncherButton == null)
            {
                appLauncherButton = ApplicationLauncher.Instance.AddModApplication(
                    delegate
                    {
                        uiActive = true;
                    },
                    delegate
                    {
                        uiActive = false;
                    },
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.FLIGHT |
                    ApplicationLauncher.AppScenes.MAPVIEW |
                    ApplicationLauncher.AppScenes.SPH |
                    ApplicationLauncher.AppScenes.VAB,
                    GameDatabase.Instance.GetTexture(FB_TB_posgen_P + "-38", false)
                );
            }
#endif
            if (toolbarControl == null)
            {
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(OnClick, OnClick,
                    ApplicationLauncher.AppScenes.FLIGHT |
                        ApplicationLauncher.AppScenes.MAPVIEW |
                        ApplicationLauncher.AppScenes.SPH |
                        ApplicationLauncher.AppScenes.VAB,
                    MODID,
                    "fuseBoxButton",
                    FB_TB_posgen_P + "-38",
                    FB_TB_posgen_P,
                    MODNAME
                );
            }
        }
        internal const string MODID = "FuseBox_NS";
        internal const string MODNAME = "FuseBox";

        void OnClick()
        {
            uiActive = !uiActive;
        }
        protected void DestroyLauncher()
        {
            Log.Info("DestroyLauncher");
#if false
            if (appLauncherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
                appLauncherButton = null;
            }

            if (ToolbarButton != null && HighLogic.CurrentGame.Parameters.CustomParams<Fusebox>().blizzy)
            {
                ToolbarButton.Destroy();
                ToolbarButton = null;
            }
#endif
            toolbarControl.OnDestroy();
            Destroy(toolbarControl);
            toolbarControl = null;
        }


        //Event when the UI is hidden (F2)
        protected void HideUI()
        {
            globalHidden = true;
        }

        //Event when the UI is shown (F2)
        protected void ShowUI()
        {
            globalHidden = false;
        }


        void checkLibraryVersions()
        {
            if (TACLPresent)
                try
                {
                    checkTACL();
                }
                catch
                {
                    Debug.Log("FB - Wrong TAC LS library version - disabled.");
                    TACLPresent = false;
                }

        }
        protected void updateAmValues()
        {
            am_max = 0;
            am_cur = 0;
            am_prod = 0;
            am_use = 0;

            checkLibraryVersions();

            if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)  // Check if in flight
            {
                try
                {
                    parts = FlightGlobals.ActiveVessel.parts; // Saw a comment by Sarbian, I don't need a sorted list anyway.
                    if (parts == null)
                        return;
                }
                catch (NullReferenceException e)
                {
                    if (e.Source != null)
                        return;
                }
            }

            if (EditorLogic.fetch != null) // Check if in editor
                try
                {
                    parts = EditorLogic.fetch.ship.parts; // Saw a comment by Sarbian, I don't need a sorted list anyway.
                    if (parts == null)
                        return;
                }
                catch (NullReferenceException e)
                {
                    if (e.Source != null)
                        return;
                }

            foreach (Part p in parts)
            {
                if (!typeArr[10] && p.Modules[0].moduleName == "LaunchClamp")
                    continue; // skip clamps if filtered

                foreach (PartResource r in p.Resources)
                {
                    if (r.info.id == definition.id)
                    {
                        am_max += r.maxAmount;
                        am_cur += r.amount;
                    }

                }

                bool currentEngActive = false;
                double alt_rate = 0;

                if (p.Modules.Count < 1)
                    continue;

                foreach (PartModule tmpPM in p.Modules)
                {
                    switch (tmpPM.moduleName)
                    {
                        case "ModuleDeployableSolarPanel":
                        case "KopernicusSolarPanel":
                            if (typeArr[0])
                            {
                                ModuleDeployableSolarPanel tmpSol = (ModuleDeployableSolarPanel)tmpPM;
                                if (mode == DisplayMode.inFlight)
                                    am_prod += tmpSol.flowRate;
                                else
                                    am_prod += tmpSol.chargeRate;
                            }
                            break;
                        case "ModuleGenerator":
                            if (typeArr[1])
                            {
                                ModuleGenerator tmpGen = (ModuleGenerator)tmpPM;
                                //								foreach (ModuleGenerator.GeneratorResource outp in tmpGen.outputList)
                                foreach (ModuleResource outp in tmpGen.resHandler.outputResources)
                                {
                                    if (outp.name == "ElectricCharge")
                                        if (mode == DisplayMode.editor)
                                            am_prod += outp.rate;
                                        else
                                            if (tmpGen.isAlwaysActive || tmpGen.generatorIsActive)
                                            am_prod += outp.rate;
                                }
                                //								foreach (ModuleGenerator.GeneratorResource inp in tmpGen.inputList)
                                foreach (ModuleResource inp in tmpGen.resHandler.inputResources)
                                {
                                    if (inp.name == "ElectricCharge")
                                        if (mode == DisplayMode.editor)
                                            am_use += inp.rate;
                                        else
                                            if (tmpGen.isAlwaysActive || tmpGen.generatorIsActive)
                                            am_use += inp.rate;
                                }
                            }
                            break;
                        case "ModuleResourceConverter":
                        case "FissionReactor":
                        case "KFAPUController":
                            if (typeArr[1])
                            {
                                ModuleResourceConverter tmpGen = (ModuleResourceConverter)tmpPM;
                                foreach (ResourceRatio outp in tmpGen.outputList)
                                {
                                    if (outp.ResourceName == "ElectricCharge")
                                        if (mode == DisplayMode.editor)
                                            am_prod += outp.Ratio;
                                        else
                                        if (tmpGen.AlwaysActive || tmpGen.IsActivated)
                                            am_prod += outp.Ratio; // might need efficiency in flight
                                }
                                foreach (ResourceRatio inp in tmpGen.inputList)
                                {
                                    if (inp.ResourceName == "ElectricCharge")
                                        if (mode == DisplayMode.editor)
                                            am_use += inp.Ratio;
                                        else
                                        if (tmpGen.AlwaysActive || tmpGen.IsActivated)
                                            am_use += inp.Ratio; // might need efficiency in flight
                                }
                            }
                            break;
                        case "ModuleResourceHarvester":
                            if (typeArr[1])
                            {
                                ModuleResourceHarvester tmpHar = (ModuleResourceHarvester)tmpPM;
                                foreach (ResourceRatio outp in tmpHar.outputList)
                                {
                                    if (outp.ResourceName == "ElectricCharge")
                                        if (mode == DisplayMode.editor)
                                            am_prod += outp.Ratio;
                                        else
                                        if (tmpHar.AlwaysActive || tmpHar.IsActivated)
                                            am_prod += outp.Ratio; // might need efficiency in flight
                                }
                                foreach (ResourceRatio inp in tmpHar.inputList)
                                {
                                    if (inp.ResourceName == "ElectricCharge")
                                        if (mode == DisplayMode.editor)
                                            am_use += inp.Ratio;
                                        else
                                        if (tmpHar.AlwaysActive || tmpHar.IsActivated)
                                            am_use += inp.Ratio; // might need efficiency in flight
                                }
                            }
                            break;
                        case "ModuleWheel":
                            if (typeArr[2])
                            {
                                ModuleWheels.ModuleWheelMotor tmpWheel = (ModuleWheels.ModuleWheelMotor)tmpPM;
                                if (tmpWheel.GetConsumedResources().Find(r => r.name == "ElectricCharge") != null)
                                {
                                    //									if (mode == DisplayMode.inFlight && tmpWheel.drive != 0 )
                                    //										am_use += tmpWheel.resourceConsumptionRate;
                                    //									if (mode == DisplayMode.editor)
                                    am_use += tmpWheel.avgResRate;
                                }
                            }
                            break;
                        case "ModuleCommand":
                            if (typeArr[3])
                            {
                                ModuleCommand tmpPod = (ModuleCommand)tmpPM;
                                //								foreach (ModuleResource r in tmpPod.inputResources)
                                foreach (ModuleResource r in tmpPod.resHandler.inputResources)
                                {
                                    if (r.id == definition.id)
                                    {
                                        am_use += r.rate;
                                    }
                                }
                            }
                            break;
                        case "ModuleLight":
                            if (typeArr[4])
                            {
                                ModuleLight tmpLight = (ModuleLight)tmpPM;
                                if (mode == DisplayMode.editor || (mode == DisplayMode.inFlight && tmpLight.isOn))
                                    am_use += tmpLight.resourceAmount;
                            }
                            break;
                        case "ModuleDataTransmitter":
                            if (typeArr[5])
                            {
                                ModuleDataTransmitter tmpAnt = (ModuleDataTransmitter)tmpPM;
                                if (mode == DisplayMode.editor || (mode == DisplayMode.inFlight && tmpAnt.IsBusy()))
                                    am_use += tmpAnt.DataResourceCost * (1 / tmpAnt.packetInterval);
                            }
                            break;
                        case "ModuleReactionWheel":
                            if (typeArr[6])
                            {
                                ModuleReactionWheel tmpRW = (ModuleReactionWheel)tmpPM;
                                //								foreach (ModuleResource r in tmpRW.inputResources)
                                foreach (ModuleResource r in tmpRW.resHandler.inputResources)
                                {
                                    if (r.id == definition.id)
                                    {
                                        if (mode == DisplayMode.editor)
                                            am_use += r.rate * 3; // * tmpRW.PitchTorque;  // rough guess for VAB
                                        if (mode == DisplayMode.inFlight)
                                            am_use += r.currentAmount * (tmpRW.PitchTorque + tmpRW.RollTorque + tmpRW.YawTorque);
                                    }
                                }
                            }
                            break;
                        case "ModuleEngines":
                            ModuleEngines tmpEng = (ModuleEngines)tmpPM;
                            if (typeArr[7])
                            {
                                const float grav = 9.81f;
                                bool usesCharge = false;
                                float sumRD = 0;
                                Single ecratio = 0;
                                foreach (Propellant prop in tmpEng.propellants)
                                {
                                    if (prop.name == "ElectricCharge")
                                    {
                                        usesCharge = true;
                                        ecratio = prop.ratio;
                                    }
                                    sumRD += prop.ratio * PartResourceLibrary.Instance.GetDefinition(prop.id).density;
                                }
                                if (usesCharge)
                                {
                                    float massFlowRate;
                                    if (mode == DisplayMode.inFlight && tmpEng.isOperational && tmpEng.currentThrottle > 0)
                                    {
                                        massFlowRate = (tmpEng.currentThrottle * tmpEng.maxThrust) / (tmpEng.atmosphereCurve.Evaluate(0) * grav);
                                        am_use += (ecratio * massFlowRate) / sumRD;
                                    }
                                    if (mode == DisplayMode.editor && currThrottle > 0.0)
                                    {
                                        massFlowRate = (currThrottle * tmpEng.maxThrust) / (tmpEng.atmosphereCurve.Evaluate(0) * grav);
                                        am_use += (ecratio * massFlowRate) / sumRD;
                                    }
                                }
                            }
                            currentEngActive = tmpEng.isOperational && (tmpEng.currentThrottle > 0);
                            if (alt_rate > 0 && typeArr[7] && currentEngActive)
                                am_prod += alt_rate;
                            break;
                        case "ModuleEnginesFX":
                            ModuleEnginesFX tmpEngFX = (ModuleEnginesFX)tmpPM;
                            if (typeArr[7])
                            {
                                const float grav = 9.81f;
                                bool usesCharge = false;
                                float sumRD = 0;
                                Single ecratio = 0;
                                foreach (Propellant prop in tmpEngFX.propellants)
                                {
                                    if (prop.name == "ElectricCharge")
                                    {
                                        usesCharge = true;
                                        ecratio = prop.ratio;
                                    }
                                    sumRD += prop.ratio * PartResourceLibrary.Instance.GetDefinition(prop.id).density;
                                }
                                if (usesCharge)
                                {
                                    float massFlowRate;
                                    if (mode == DisplayMode.inFlight && tmpEngFX.isOperational && tmpEngFX.currentThrottle > 0)
                                    {
                                        massFlowRate = (tmpEngFX.currentThrottle * tmpEngFX.maxThrust) / (tmpEngFX.atmosphereCurve.Evaluate(0) * grav);
                                        am_use += (ecratio * massFlowRate) / sumRD;
                                    }
                                    if (mode == DisplayMode.editor && currThrottle > 0.0)
                                    {
                                        massFlowRate = (currThrottle * tmpEngFX.maxThrust) / (tmpEngFX.atmosphereCurve.Evaluate(0) * grav);
                                        am_use += (ecratio * massFlowRate) / sumRD;
                                    }
                                }
                            }
                            currentEngActive = tmpEngFX.isOperational && (tmpEngFX.currentThrottle > 0);
                            if (alt_rate > 0 && typeArr[7] && currentEngActive)
                                am_prod += alt_rate;
                            break;
                        case "ModuleAlternator":
                            if (typeArr[8])
                            {
                                ModuleAlternator tmpAlt = (ModuleAlternator)tmpPM;
                                //								foreach (ModuleResource r in tmpAlt.outputResources)
                                foreach (ModuleResource r in tmpAlt.resHandler.outputResources)
                                {
                                    if (r.name == "ElectricCharge")
                                    {
                                        if (mode == DisplayMode.editor || (mode == DisplayMode.inFlight && currentEngActive))
                                            am_prod += r.rate;
                                        else
                                            alt_rate = r.rate;
                                    }
                                }
                            }
                            break;
                        case "ModuleScienceLab":
                            if (typeArr[9])
                            {
                                if (mode == DisplayMode.inFlight)
                                {
                                    ModuleScienceLab tmpLab = (ModuleScienceLab)tmpPM;
                                    foreach (ModuleResource r in tmpLab.processResources)
                                    {
                                        if (r.name == "ElectricCharge" && tmpLab.IsOperational())
                                            am_prod += r.rate;
                                    }
                                }
                            }
                            break;
                    }

                    if (ALPresent)
                        try
                        {
                            checkAv(tmpPM);
                        }
                        catch
                        {
                            Debug.Log("FB - Wrong Aviation Lights library version - disabled.");
                            ALPresent = false;
                        }
                    if (SLPresent)
                        try
                        {
                            checkSv(tmpPM);
                        }
                        catch
                        {
                            Debug.Log("FB - Wrong Surface Lights library version - disabled.");
                            SLPresent = false;
                        }
                    if (KASPresent)
                        try
                        {
                            checkKAS(tmpPM);
                        }
                        catch
                        {
                            Debug.Log("FB - Wrong KAS library version - disabled.");
                            KASPresent = false;
                        }

                    if (NFEPresent)
                        try
                        {
                            checkNFE(tmpPM);
                        }
                        catch
                        {
                            Debug.Log("FB - Wrong Near Future Electric library version - disabled.");
                            NFEPresent = false;
                        }
                    if (NFSPresent)
                        try
                        {
                            checkNFS(tmpPM);
                        }
                        catch
                        {
                            Debug.Log("FB - Wrong Near Future Solar library version - disabled.");
                            NFSPresent = false;
                        }

                    if (RT2Present)
                        try
                        {
                            checkRT2(tmpPM);
                        }
                        catch
                        {
                            Debug.Log("FB - Wrong Remote Tech 2 library version - disabled.");
                            RT2Present = false;
                        }

                    if (ScSPresent)
                        try
                        {
                            checkSCANsat(tmpPM);
                        }
                        catch
                        {
                            Debug.Log("FB - Wrong SCANsat library version - disabled.");
                            ScSPresent = false;
                        }

                    if (TelPresent)
                        try
                        {
                            checkTel(tmpPM);
                        }
                        catch
                        {
                            Debug.Log("FB - Wrong Telemachus library version - disabled.");
                            TelPresent = false;
                        }

                    if (AntRPresent)
                        try
                        {
                            checkAntR(tmpPM);
                        }
                        catch
                        {
                            Debug.Log("FB - Wrong AntennaRange library version - disabled.");
                            AntRPresent = false;
                        }

                    if (kOSPresent)
                        try
                        {
                            checkkOS(tmpPM);
                        }
                        catch
                        {
                            Debug.Log("FB - Wrong kOS library version - disabled.");
                            kOSPresent = false;
                        }
                    if (DeepFreezePresent)
                        try
                        {
                            checkDF(tmpPM);
                        }
                        catch
                        {
                            Debug.Log("FB - Wrong DeepFreeze library version - disabled.");
                            DeepFreezePresent = false;
                        }
                    if (KSPWheelPresent)
                        try
                        {
                            checkKSPWheel(tmpPM);
                        }
                        catch
                        {
                            Debug.Log("FB - Wrong KSPWheelPresent library version - disabled.");
                            KSPWheelPresent = false;
                        }
#if SSTU
                    if (SSTUToolsPresent)
                        try
                        {
                            checkSSTUTools(tmpPM);
                        }
                        catch
                        {
                            Debug.Log("FB - Wrong SSTUTools library version - disabled.");
                            SSTUToolsPresent = false;
                        }

                    /*						if (BioPresent)
                                                try
                                            {
                                                checkBio(tmpPM);
                                            }
                                            catch
                                            {
                                                Debug.Log("FB - Wrong Biomatic library version - disabled.");
                                                BioPresent = false;
                                            }

                                            if (KarPresent)
                                                try
                                            {
                                                checkKar(tmpPM);
                                            }
                                            catch
                                            {
                                                Debug.Log("FB - Wrong Karbonite library version - disabled.");
                                                KarPresent = false;
                                            }

                                            if (BDSMPresent)
                                                try
                                            {
                                                checkBDSM(tmpPM);
                                            }
                                            catch
                                            {
                                                Debug.Log("FB - Wrong BTSM library version - disabled.");
                                                BDSMPresent = false;
                                            } */
#endif
                }
            }

            //Log.Info("am_max: " + am_max.ToString());
            //Log.Info("am_cur: " + am_cur.ToString());
            //Log.Info("am_prod: " + am_prod.ToString());
            //Log.Info("am_use: " + am_use.ToString());
            if (mode == DisplayMode.inFlight)
            {
                timeIntervalMeasured = Planetarium.GetUniversalTime() - lastTimeCheck;
                lastTimeCheck = Planetarium.GetUniversalTime();
                ResourceStats rs = VesselStatsManager.Instance.GetOrAdd(FlightGlobals.ActiveVessel);
                if (rs != null)
                {
                    //m_prod2 = rs.GetGeneration("ElectricCharge");
                    //m_use2 = rs.GetConsumption("ElectricCharge");
                    // Log.Info("ResourceStats.GetTotalConsumption: " + (rs.GetConsumption("ElectricCharge") / timeIntervalMeasured).ToString());
                    // Log.Info("ResourceStatsGetTotalGeneration: " + (rs.GetGeneration("ElectricCharge") / timeIntervalMeasured).ToString());

                    rs.Reset();
                }
            }
        }
        protected void checkAv(PartModule tmpPM)
        {
            switch (tmpPM.moduleName)
            {
                case "ModuleNavLight":
                    if (typeArr[4])
                    {
                        global::AviationLights.ModuleNavLight tmpLight = (global::AviationLights.ModuleNavLight)tmpPM;
                        am_use += tmpLight.EnergyReq;
                    }
                    break;
            }
        }

        protected void checkSv(PartModule tmpPM)
        {
            switch (tmpPM.moduleName)
            {
                case "ModuleColoredLensLight":
                    if (typeArr[4])
                    {

                        global::SurfaceLights.ModuleColoredLensLight tmpLight = (global::SurfaceLights.ModuleColoredLensLight)tmpPM;
                        if (mode == DisplayMode.editor || (mode == DisplayMode.inFlight && tmpPM.isActiveAndEnabled))
                            am_use += tmpLight.resourceAmount;
                    }
                    break;
                case "ModuleMultiPointSurfaceLight":
                    if (typeArr[4])
                    {
                        global::KSP_Light_Mods.ModuleMultiPointSurfaceLight tmpLight = (global::KSP_Light_Mods.ModuleMultiPointSurfaceLight)tmpPM;
                        if (mode == DisplayMode.editor || (mode == DisplayMode.inFlight && tmpPM.isActiveAndEnabled))
                            am_use += tmpLight.resourceAmount;
                    }
                    break;
                    
                case "ModuleStockLightColoredLens":
                    if (typeArr[4])
                    {
                        global::SurfaceLights.ModuleStockLightColoredLens tmpLight = (global::SurfaceLights.ModuleStockLightColoredLens)tmpPM;
                        if (mode == DisplayMode.editor || (mode == DisplayMode.inFlight && tmpPM.isActiveAndEnabled))
                            am_use += tmpLight.resourceAmount;
                    }
                    break;
            }
        }

        protected void checkNFE(PartModule tmpPM)
        {
            switch (tmpPM.moduleName)
            {
                case "ModuleRadioisotopeGenerator":
                    if (typeArr[12])
                    {
                        global::NearFutureElectrical.ModuleRadioisotopeGenerator tmpGen = (global::NearFutureElectrical.ModuleRadioisotopeGenerator)tmpPM;
                        if (mode == DisplayMode.editor)
                            am_prod += tmpGen.BasePower;
                       else
                            am_prod += tmpGen.BasePower * (tmpGen.PercentPower / 100);                       
                    }
                    break;
#if false
                case "FissionReactor":
                    if (typeArr[12])
                    {
                        NearFutureElectrical.FissionReactor tmpGen = (global::NearFutureElectrical.FissionReactor)tmpPM;

                        if (mode != DisplayMode.editor)
                            am_prod += tmpGen.AvailablePower* (tmpGen.ActualPowerPercent / 100);
                        else
                            am_prod += tmpGen.AvailablePower;
                    }
                    break;

                                        // Following needed for Capaciters in Near Future Electrical
                    if (NFEPresent && r.info.id == storedChargeDefinition.id)
                    {
                        am_max += r.maxAmount;
                        am_cur += r.amount;
                    }

#endif


                case "FissionGenerator":
                    if (typeArr[12])
                    {
                        NearFutureElectrical.FissionGenerator tmpGen = (global::NearFutureElectrical.FissionGenerator)tmpPM;

                        if (mode != DisplayMode.editor)
                            am_prod += tmpGen.CurrentGeneration;
                        else
                            am_prod += tmpGen.PowerGeneration;
                    }
                    break;


                case "DischargeCapacitor":
                    if (typeArr[12])
                    {
                        global::NearFutureElectrical.DischargeCapacitor tmpGen = (global::NearFutureElectrical.DischargeCapacitor)tmpPM;
                        //if (mode == DisplayMode.inFlight && tmpGen.DischargeRate > 0.0)
                        if (mode != DisplayMode.editor)
                        {
                            foreach (PartResource r in tmpPM.part.Resources)
                            {
                                if (r.info.id == storedChargeDefinition.id)
                                {
                                    //am_use += tmpGen.dischargeActual;
                                    am_max += r.maxAmount;
                                    am_cur += r.amount;
                                }
                            }

                        }
                        else
                        {

                            foreach (PartResource r in tmpPM.part.Resources)
                            {
                                if (r.info.id == storedChargeDefinition.id)
                                {
                                    //am_use += tmpGen.DischargeRate;
                                    am_max += r.maxAmount;
                                    am_cur += r.maxAmount;
                                }
                            }
                        }

                    }
                    break;
            }
        }

        protected void checkNFS(PartModule tmpPM)
        {
            switch (tmpPM.moduleName)
            {
                case "ModuleCurvedSolarPanel":
                    if (typeArr[12])
                    {
                        global::NearFutureSolar.ModuleCurvedSolarPanel tmpSol = (global::NearFutureSolar.ModuleCurvedSolarPanel)tmpPM;
                        //					if (mode == DisplayMode.inFlight && (tmpSol.State.Equals(ModuleDeployableSolarPanel.panelStates.EXTENDED)))
                        if (mode == DisplayMode.inFlight && (tmpSol.State.Equals(ModuleDeployableSolarPanel.DeployState.EXTENDED)))
                            am_prod += tmpSol.energyFlow;
                        else if (mode == DisplayMode.editor)
                            am_prod += tmpSol.TotalEnergyRate;
                    }
                    break;
            }
        }

        protected void checkKAS(PartModule tmpPM)
        {
            switch (tmpPM.moduleName)
            {
                case "KASModuleWinch":
                    if (typeArr[16])
                    {
                        global::KAS.KASModuleWinch tmpKW = (global::KAS.KASModuleWinch)tmpPM;
                        if (mode == DisplayMode.inFlight && tmpKW.isActive)
                            am_use += tmpKW.powerDrain * tmpKW.motorSpeed;
                        if (mode == DisplayMode.editor)
                            am_use += tmpKW.powerDrain;
                    }
                    break;
                case "KASModuleMagnet":
                    if (typeArr[16])
                    {
                        global::KAS.KASModuleMagnet tmpHM = (global::KAS.KASModuleMagnet)tmpPM;
                        if (mode == DisplayMode.editor || (mode == DisplayMode.inFlight && tmpHM.MagnetActive))
                            am_use += tmpHM.powerDrain;
                    }
                    break;
            }
        }

        protected void checkRT2(PartModule tmpPM)
        {
            switch (tmpPM.moduleName)
            {
                case "ModuleRTAntenna":
                    if (typeArr[11])
                    {
                        global::RemoteTech.Modules.ModuleRTAntenna tmpAnt = (global::RemoteTech.Modules.ModuleRTAntenna)tmpPM;
                        if (mode == DisplayMode.inFlight && tmpAnt.Activated)
                            am_use += tmpAnt.Consumption;
                        if (mode == DisplayMode.editor)
                            am_use += tmpAnt.EnergyCost;
                    }
                    break;
            }
        }

        protected void checkSCANsat(PartModule tmpPM)
        {
            switch (tmpPM.moduleName)
            {
                case "SCANsat":
                    if (typeArr[13])
                    {
                        global::SCANsat.SCAN_PartModules.SCANsat tmpSS = (global::SCANsat.SCAN_PartModules.SCANsat)tmpPM;

                        foreach (ModuleResource r in tmpSS.resHandler.inputResources)
                        {
                            if (r.id == definition.id)
                            {
                                if ((mode == DisplayMode.editor) || (mode == DisplayMode.inFlight && (r.rate > 0.0 && tmpSS.scanningNow)))
                                    am_use += r.rate;
                            }
                        }
                    }
                    break;
                case "ModuleSCANresourceScanner":
                    if (typeArr[13])
                    {
                        global::SCANsat.SCAN_PartModules.ModuleSCANresourceScanner tmpSS = (global::SCANsat.SCAN_PartModules.ModuleSCANresourceScanner)tmpPM;
                        foreach (ModuleResource r in tmpSS.resHandler.inputResources)
                        {
                            if (r.id == definition.id)
                            {
                                if ((mode == DisplayMode.editor) || (mode == DisplayMode.inFlight && (r.rate > 0.0 && tmpSS.scanningNow)))
                                    am_use += r.rate;
                            }
                        }
                        //if ((mode == DisplayMode.editor) || (mode == DisplayMode.inFlight && (tmpSS.power > 0.0 && tmpSS.scanningNow())))
                        //    am_use += tmpSS.power;
                    }
                    break;
            }
        }

        protected void checkTel(PartModule tmpPM)
        {
            //			switch (tmpPM.moduleName)
            //			{
            //			case "TelemachusPowerDrain":
            //				if (typeArr[5])
            //				{
            //					if (mode == DisplayMode.inFlight && global::Telemachus.TelemachusPowerDrain.isActive)
            //						am_use += global::Telemachus.TelemachusPowerDrain.powerConsumption;
            //					if (mode == DisplayMode.editor)
            //						am_use += 0.01;
            //				}
            //				break;
            //			}
        }

        protected void checkTACL()
        {
            if (typeArr[14])
            {
                global::Tac.TacLifeSupport tmpTLS = Tac.TacLifeSupport.Instance;
                if (tmpTLS.Enabled)
                {
                    am_use += tmpTLS.BaseElectricityConsumptionRate;
                }
                // Add ElectricityConsumptionRate * crew member later
            }
        }

        protected void checkAntR(PartModule tmpPM)
        {
            switch (tmpPM.moduleName)
            {
                case "ModuleLimitedDataTransmitter":
                    if (typeArr[5])
                    {
                        ModuleDataTransmitter tmpAnt = (ModuleDataTransmitter)tmpPM;
                        if (mode == DisplayMode.editor || (mode == DisplayMode.inFlight && tmpAnt.IsBusy()))
                            am_use += tmpAnt.DataResourceCost * (1 / tmpAnt.packetInterval);
                    }
                    break;
            }
        }

        protected void checkkOS(PartModule tmpPM)
        {
            switch (tmpPM.moduleName)
            {
                case "kOSProcessor":
                    if (typeArr[5])
                    {
                        global::kOS.Module.kOSProcessor tmpkOS = (global::kOS.Module.kOSProcessor)tmpPM;
                        am_use += tmpkOS.RequiredPower;
                    }
                    break;
            }
        }
        protected void checkDF(PartModule tmpPM)
        {

            switch (tmpPM.moduleName)
            {
                case "DeepFreezer":
                    if (typeArr[17])
                    {
                        DF.DeepFreezer tmpDeepFreezer = (DF.DeepFreezer)tmpPM;
                        if ((mode == DisplayMode.inFlight && tmpDeepFreezer.isActiveAndEnabled) ||
                            mode == DisplayMode.editor)
                            am_use += tmpDeepFreezer.ChargeRequired;
                    }
                    break;
            }
        }
        protected void checkKSPWheel(PartModule tmpPM)
        {
            switch (tmpPM.moduleName)
            {
                case "KSPWheelMotor":
                    if (typeArr[2])
                    {
                        KSPWheel.KSPWheelMotor tmpKSPWheel = (KSPWheel.KSPWheelMotor)tmpPM;
                        if (mode == DisplayMode.inFlight && tmpKSPWheel.isActiveAndEnabled)
                            am_use += tmpKSPWheel.guiResourceUse;
                        else if (mode == DisplayMode.editor)
                            am_use += tmpKSPWheel.maxECDraw;
                    }
                    break;
                case "KSPWheelRepulsor":
                    if (typeArr[19])
                    {
                        KSPWheel.KSPWheelRepulsor tmpKSPWheel = (KSPWheel.KSPWheelRepulsor)tmpPM;
                        if (mode == DisplayMode.inFlight && tmpKSPWheel.isActiveAndEnabled)
                            am_use += tmpKSPWheel.guiEnergyUse;
                        if (mode == DisplayMode.editor)
                            am_use += tmpPM.vessel.GetTotalMass() * tmpKSPWheel.energyUse;
                    }
                    break;
#if false
                case "KFAPUController":
                    if (typeArr[2])
                    {
                        ModuleResourceConverter tmpGen = (ModuleResourceConverter)tmpPM;

                        if (mode == DisplayMode.inFlight)
                            am_prod += tmpGen.energyOutput * tmpGen.throttle;
                        if (mode == DisplayMode.editor)
                            am_prod += tmpGen.energyOutput;
                    }
                    break;
#endif
            }
        }

#if SSTU
        protected void checkSSTUTools(PartModule tmpPM)
        {

            switch (tmpPM.moduleName)
            {
                case "SSTUSolarPanelDeployable":
                    if (typeArr[0])
                    {
                        SSTUTools.SSTUSolarPanelDeployable tmpSol = (SSTUTools.SSTUSolarPanelDeployable)tmpPM;
                        if (mode == DisplayMode.editor || tmpSol.isActiveAndEnabled)
                            am_prod += tmpSol.resourceAmount;
                    }
                    break;
                case "SSTUSolarPanelStatic":
                    if (typeArr[0])
                    {
                        SSTUTools.SSTUSolarPanelStatic tmpSol = (SSTUTools.SSTUSolarPanelStatic)tmpPM;
                        if (mode == DisplayMode.editor || tmpSol.isActiveAndEnabled)
                            am_prod += tmpSol.resourceAmount;
                    }
                    break;
                    
            }
        }
#endif
        /*		protected void checkBio(PartModule tmpPM)
                {
                    switch (tmpPM.moduleName)
                    {
                    case "Biomatic":
                        if (typeArr[9])
                        {
                            const double biouse = 0.04;
                            am_use += biouse;
                        }
                        break;
                    }
                }

                protected void checkKar(PartModule tmpPM)
                {
                    switch (tmpPM.moduleName)
                    {
                    case "USI_ResourceConverter":
                        if (typeArr[18])
                        {
                            am_use += 1;
                        }
                        break;
                    }
                }

                protected void checkBDSM(PartModule tmpPM)
                {
                    switch (tmpPM.moduleName)
                    {
                    case "BTSMModuleProbePower":
                        if (typeArr[3])
                        {
                            switch (tmpPM.part.name)
                            {
                            case "probeCoreSphere":
                                am_use += 0.16666668;
                                break;
                            case "probeCoreCube":
                                am_use += 0.08333334;
                                break;
                            case "probeCoreHex":
                                am_use += 0.04166667;
                                break;
                            case "probeCoreOcto":
                                am_use += 0.033333336;
                                break;
                            default:
                                am_use += 0.02777778;
                                break;
                            }
                        }
                        break;
                    case "BTSMModuleLifeSupport":
                        if (typeArr[3])
                        {
                            switch (tmpPM.part.name)
                            {
                            case "Mark1-2Pod":
                                am_use += 0.20833335;
                                break;
                            case "landerCabinSmall":
                                am_use += 0.25000002;
                                break;
                            case "mark3Cockpit":
                                am_use += 0.19444446;
                                break;
                            default:
                                am_use += 0.2777778;
                                break;
                            }
                        }
                        break;
                    }
                } */

        protected void checkMode(DisplayMode curMode)
        {
            String vers;

            // Check if the mode has changed
            if (mode != curMode)
            {
                //Save the position of the window in the previous mode
                if (mode == DisplayMode.inFlight || mode == DisplayMode.editor)
                    saveConfig();

                // Change modes
                mode = curMode;
                shrinkMain = true;  // redraw minimal on scene change

                // Load the saved position of the new mode
                if (mode != DisplayMode.none)
                {
                    FBconf.load();
                    vers = FBconf.GetValue<String>("version", VERSION);
                    if (vers == VERSION)
                    {
                        mainWin = FBconf.GetValue<Rect>("mainWin" + mode, new Rect(Screen.width - mainWidth - 200, mode == DisplayMode.inFlight ? 1 : 50, mainWidth, mainHeight));
                        filterWin = FBconf.GetValue<Rect>("filterWin" + mode, new Rect(Screen.width - filtWidth - 200, mode == DisplayMode.inFlight ? 1 : 50, filtWidth, filtHeight));
                        setWin = FBconf.GetValue<Rect>("setWin" + mode, new Rect(Screen.width - setWidth - 200, mode == DisplayMode.inFlight ? 1 : 50, setWidth, setHeight));
                        drkWin = FBconf.GetValue<Rect>("drkWin" + mode, new Rect(Screen.width - setWidth - 200, mode == DisplayMode.inFlight ? 1 : 50, drkWidth, drkHeight));
                        haltWarp = FBconf.GetValue<bool>("warphalt", true);
                        haltWarpThresh = FBconf.GetValue<int>("warphaltthresh", 30);
                        showCharge = FBconf.GetValue<bool>("showcharge" + mode, true);
                        decPlaces = FBconf.GetValue<int>("precision" + mode, 2);
                        useSmokeSkin = FBconf.GetValue<bool>("useSmokeSkin", true);
                        try
                        {
                            string fntsty = "";
                            fntsty = FBconf.GetValue<string>("globalFontStyle" + mode, "Normal");
                            FSGlobal = (FontStyle)Enum.Parse(typeof(FontStyle), fntsty);
                        }
                        catch (ArgumentException)
                        {
                            FSGlobal = FontStyle.Normal;
                        }
                        GoodCol.r = (float)FBconf.GetValue<double>("goodColr" + mode, 0.0);
                        GoodCol.b = (float)FBconf.GetValue<double>("goodColb" + mode, 0.0);
                        GoodCol.g = (float)FBconf.GetValue<double>("goodColg" + mode, 1.0);
                        BadCol.r = (float)FBconf.GetValue<double>("badColr" + mode, 1.0);
                        BadCol.b = (float)FBconf.GetValue<double>("badColb" + mode, 0.0);
                        BadCol.g = (float)FBconf.GetValue<double>("badColg" + mode, 0.0);
                        OtherCol.r = (float)FBconf.GetValue<double>("otherColr" + mode, 1.0);
                        OtherCol.b = (float)FBconf.GetValue<double>("otherColb" + mode, 1.0);
                        OtherCol.g = (float)FBconf.GetValue<double>("otherColg" + mode, 1.0);
                        for (int i = 0; i < typeArr.Length; i++)
                        {
                            typeArr[i] = FBconf.GetValue<bool>("filtArr" + mode + i, true);
                        }
                    }
                }
            }
        }

        protected void saveConfig()
        {
            // Update the configuration file
            FBconf.SetValue("version", VERSION);
            if (mode >= 0)
            {
                FBconf.SetValue("mainWin" + mode, mainWin);
                FBconf.SetValue("filterWin" + mode, filterWin);
                FBconf.SetValue("setWin" + mode, setWin);
                FBconf.SetValue("drkWin" + mode, drkWin);
                FBconf.SetValue("warphalt", haltWarp);
                FBconf.SetValue("warphaltthresh", haltWarpThresh);
                FBconf.SetValue("showcharge" + mode, showCharge);
                FBconf.SetValue("precision" + mode, decPlaces);
                FBconf.SetValue("globalFontStyle" + mode, FSGlobal.ToString());
                FBconf.SetValue("goodColr" + mode, (double)GoodCol.r);
                FBconf.SetValue("goodColb" + mode, (double)GoodCol.b);
                FBconf.SetValue("goodColg" + mode, (double)GoodCol.g);
                FBconf.SetValue("badColr" + mode, (double)BadCol.r);
                FBconf.SetValue("badColb" + mode, (double)BadCol.b);
                FBconf.SetValue("badColg" + mode, (double)BadCol.g);
                FBconf.SetValue("otherColr" + mode, (double)OtherCol.r);
                FBconf.SetValue("otherColb" + mode, (double)OtherCol.b);
                FBconf.SetValue("otherColg" + mode, (double)OtherCol.g);
                FBconf.SetValue("useSmokeSkin", useSmokeSkin);
                for (int i = 0; i < typeArr.Length; i++)
                {
                    FBconf.SetValue("filtArr" + mode + i, typeArr[i]);
                }
            }
            FBconf.save();
        }

        protected void OnApplicationQuit()
        {
            // Save configuration file
            saveConfig();
        }



        protected void setTBIcon()
        {
            string newIconName = "";

            double am_perc = (am_cur / am_max) * 100;
            int segs = 1;

            if (am_perc < 33.0)
                segs = 1;
            if (am_perc > 33.0 && am_perc < 63.0)
                segs = 2;
            if (am_perc > 66.0)
                segs = 3;
            if (am_use > am_prod)
            {
                frmCount++;
                if (frmCount < 75)
                {
                    if (am_perc < 0.1)
                    {
                        newIconName = this.FB_TB_empty_P;
                    }
                    else
                    {
                        switch (segs)
                        {
                            case 1:
                                newIconName = this.FB_TB_dr1b_P;
                                break;
                            case 2:
                                newIconName = this.FB_TB_dr2b_P;
                                break;
                            case 3:
                                newIconName = this.FB_TB_dr3b_P;
                                break;
                        }
                    }
                }
                else
                {
                    if (frmCount == 75 && showCharge)
                        newIconName = this.FB_TB_drain_P;
                    if (frmCount == 150)
                    {
                        frmCount = 1;
                    }
                }
            }
            else
            {
                frmCount++;
                if (frmCount < 75)
                {
                    if (am_perc < 0.1)
                    {
                        newIconName = this.FB_TB_empty_P;
                    }
                    else
                    {
                        switch (segs)
                        {
                            case 1:
                                newIconName = this.FB_TB_pos1b_P;
                                break;
                            case 2:
                                newIconName = this.FB_TB_pos2b_P;
                                break;
                            case 3:
                                newIconName = this.FB_TB_full_P;
                                break;
                        }
                    }
                }
                else
                {
                    if ((frmCount == 75) && (am_perc < 99.0) && showCharge)
                        newIconName = this.FB_TB_posgen_P;
                    if (frmCount == 150)
                    {
                        frmCount = 1;
                    }
                }
            }
            Log.Info("setTBIcon 2");
            if (toolbarControl == null)
                Log.Info("toolbarControl is null");
            if (newIconName != "" && toolbarControl != null)
            {
#if false
                if (ToolbarManager.ToolbarAvailable && HighLogic.CurrentGame.Parameters.CustomParams<Fusebox>().blizzy)
                    ToolbarButton.TexturePath = newIconName;
                else
                {
                    if (appLauncherButton != null)
                        appLauncherButton.SetTexture(GameDatabase.Instance.GetTexture(newIconName + "-38", false));
                }
#endif
                toolbarControl.SetTexture(newIconName + "-38", newIconName);
            }
        }


        protected TimeSpan ConvToKerb(TimeSpan span)
        {
            const int dayMult = 4;
            const int hoursDay = 6;
            const int secMin = 60;
            int tmpDays = span.Days * dayMult;
            tmpDays += (int)span.Hours / hoursDay;
            int tmpHours = span.Hours % hoursDay;
            return TimeSpan.FromSeconds(((tmpDays * hoursDay * secMin * secMin) + (tmpHours * secMin * secMin) + (span.Minutes * secMin) + span.Seconds));
        }

        protected double getDarkTime()
        {
            celBody b = allBodies.Find(x => x.CelName == pickedBod);
            double xyrA = (b.CelRad + celOrbit);
            double xyH = Math.Sqrt(xyrA * b.CelGM);
            double dTime = 2 * xyrA * xyrA / xyH * (Math.Asin(b.CelRad / xyrA));
            //			Debug.Log ("FB - " + b.CelName + " - " + dTime.ToString() + " sec of darktime");
            return dTime;
        }

        protected double getDarkTimeLanded()
        {
            celBody b = allBodies.Find(x => x.CelName == pickedBod);
            return b.CelRot / 2;
        }
    }

    public class celBody
    {
        public celBody(string celName, double celRad, double celGM, int celRot)
        {
            CelName = celName;
            CelRad = celRad;
            CelGM = celGM;
            CelRot = celRot;
        }

        public string CelName { get; set; }
        public double CelRad { get; set; }
        public double CelGM { get; set; }
        public int CelRot { get; set; }
    }

    // Popup list created by Eric Haines
    // Popup list Extended by John Hamilton. john@nutypeinc.com
    public class Popup
    {
        static int popupListHash = "PopupList".GetHashCode();



        public static bool List(Rect position, ref bool showList, ref int listEntry, GUIContent buttonContent, object[] list,
                                 GUIStyle listStyle)
        {



            return List(position, ref showList, ref listEntry, buttonContent, list, "button", "box", listStyle);
        }

        public static bool List(Rect position, ref bool showList, ref int listEntry, GUIContent buttonContent, object[] list,
                                 GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle)
        {


            int controlID = GUIUtility.GetControlID(popupListHash, FocusType.Passive);
            bool done = false;
            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.mouseDown:
                    if (position.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.hotControl = controlID;
                        showList = true;
                    }
                    break;
                case EventType.mouseUp:
                    if (showList)
                    {
                        done = true;
                    }
                    break;
            }

            GUI.Label(position, buttonContent, buttonStyle);
            if (showList)
            {

                // Get our list of strings
                string[] text = new string[list.Length];
                // convert to string
                for (int i = 0; i < list.Length; i++)
                {
                    text[i] = list[i].ToString();
                }

                Rect listRect = new Rect(position.x, position.y + 25, position.width, list.Length * 25);
                GUI.Box(listRect, "", boxStyle);
                listEntry = GUI.SelectionGrid(listRect, listEntry, text, 1, listStyle);
            }
            if (done)
            {
                showList = false;
            }
            return done;
        }
    }


}
