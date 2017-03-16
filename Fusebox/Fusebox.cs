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
	[KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
	public class Fusebox : MonoBehaviour
	{
	    private static String VERSION = "1.5";  // Current version
	    private const int MAINWINID = 8631;  // Main window ID
		private const int FILTWINID = 8632;  // Filter window ID
		private const int SETWINID = 8633;  // Settings window ID
		private const int WARWINID = 8634;  // Warp warning window ID
		private const int DRKWINID = 8635;  // Darkness window ID
		private const int POPWINID = 8636;  // Darkness body pick window ID
		private static PluginConfiguration FBconf = null;   // Config file
	
	    private static int mainWidth = 120;    // Main window width
	    private static int mainHeight = 10;   // Main window height
		private static int filtWidth = 120;    // Filt window width
		private static int filtHeight = 10;   // Filt window height
		private static int setWidth = 120;    // Settings window width
		private static int setHeight = 10;    // Settings window height
		private static int drkWidth = 160;    // Settings window width
		private static int drkHeight = 10;    // Settings window height

	    private Rect mainWin = new Rect(Screen.width / 2, Screen.height / 4, mainWidth, mainHeight);  // Main window position and size
		private Rect filterWin = new Rect(Screen.width / 2, Screen.height / 4, filtWidth, filtHeight);  // Filter window position and size
		private Rect setWin = new Rect(Screen.width / 2, Screen.height / 4, setWidth, setHeight);  // Filter window position and size
		private Rect drkWin = new Rect(Screen.width / 2, Screen.height / 4, drkWidth, drkHeight);  // Filter window position and size
		private Rect warnWin = new Rect(Screen.width / 2, Screen.height / 4, 460, 120);  // Warp warning window position and size
		private Rect popupWin = new Rect(Screen.width / 2, Screen.height / 4, 120, 23);  // Dark timer pick window position and size

		private bool uiActive = false;
		private bool useSmokeSkin;
		private bool doOneFrame = false;
		private bool shrinkMain = false;
		private bool showFilters = false;
		private bool showSettings = false;
		private bool showDarkTime = false;
		private bool globalHidden = false;
		private bool ALPresent = false;
		private bool NFEPresent = false;
		private bool NFSPresent = false;
		private bool KASPresent = false;
		private bool RT2Present = false;
		private bool ScSPresent = false;
		private bool TelPresent = false;
		private bool TACLPresent = false;
		private bool kOSPresent = false;
//		private bool BioPresent = false;
		private bool AntRPresent = false;
//		private bool KarPresent = false;
//		private bool BDSMPresent = false;
		private bool wasDraining = false;
//		private bool isCharging = false;
//		private bool skinChange = false;
		private bool showCharge = true;
		private bool[] typeArr = new bool[20];
	    private static int mode = -1;  // Display mode, currently  0 for In-Flight, 1 for Editor, -1 to hide

		private static bool haltWarp = true;
		private static bool haltTriggered = false;
		private static bool warnRead = false;
		private static bool warnPopped = false;
		private static bool showWarn = false;
		private static int haltWarpThresh = 30;
		private static int decPlaces = 2;
		
		internal static double am_max = 0;
		internal static double am_cur = 0;
		internal static double am_prod = 0;
		internal static double am_use = 0;
		private static double sumDelta = 0;
		private static float currThrottle = 1.0F;

		private static FontStyle FSGlobal = FontStyle.Normal;
		private static Color GoodCol = Color.green;
		private static Color BadCol = Color.red;
		private static Color OtherCol = Color.white;
		
		// Toolbar stuff, cribbed from VOID
		private ApplicationLauncherButton appLauncherButton;
		internal IButton ToolbarButton;
		protected Texture2D FB_TB_full;
		protected Texture2D FB_TB_pos2b;
		protected Texture2D FB_TB_pos1b;
		protected Texture2D FB_TB_dr3b;
		protected Texture2D FB_TB_dr2b;
		protected Texture2D FB_TB_dr1b;
		protected Texture2D FB_TB_empty;
		protected Texture2D FB_TB_posgen;
		protected Texture2D FB_TB_drain;
		private int frmCount = 1;
		protected string FB_TB_full_P = "Fusebox/TB_icons/3of3green";
		protected string FB_TB_pos2b_P = "Fusebox/TB_icons/2of3green";
		protected string FB_TB_pos1b_P = "Fusebox/TB_icons/1of3green";
		protected string FB_TB_dr3b_P = "Fusebox/TB_icons/3of3red";
		protected string FB_TB_dr2b_P = "Fusebox/TB_icons/2of3red";
		protected string FB_TB_dr1b_P = "Fusebox/TB_icons/1of3red";
		protected string FB_TB_empty_P = "Fusebox/TB_icons/emptyred";
		protected string FB_TB_posgen_P = "Fusebox/TB_icons/posgen";
		protected string FB_TB_drain_P = "Fusebox/TB_icons/draining";
		
		// Darkness calc bits
		public static List<celBody> allBodies = new List<celBody>();
		public static string[] allBodNames;
		private bool pickBod = false;
		private string pickedBod = "";
		private int pickedBodIdx = 3;
		private int celOrbit = 100;
		private bool pickShow = false;
		
		public void Awake()
	    {
	    }
	
	    public void Start()
	    {
	        // Create or load config file
	        FBconf = KSP.IO.PluginConfiguration.CreateForType<Fusebox>(null);
	        FBconf.load();
			for (int i = 0; i < typeArr.Length; i++)
			{
				typeArr[i] = true;
			}
			
			// Find out which mods are present

			ALPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "AviationLights");
			NFEPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "NearFutureElectrical");
			NFSPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "NearFutureSolar");
			KASPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "KAS");
			RT2Present = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "RemoteTech");
			ScSPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "SCANsat");
			TelPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "Telemachus");
			TACLPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "TacLifeSupport");
			AntRPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "AntennaRange");
			kOSPresent = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name == "kOS");
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

			// Get solar system bodies and add to list
			List<CelestialBody> solSys = FlightGlobals.Bodies;
			foreach (CelestialBody body in solSys)
			{
				if (body.isHomeWorld)
					pickedBod = body.name;
				allBodies.Add(new celBody(body.name, body.Radius/1000, body.gravParameter/1000000000, (int) body.rotationPeriod));
//				Debug.Log("FB - " + body.name + " - " + body.Radius/1000 + " - " + body.gravParameter/1000000000 + " - " + (int) body.rotationPeriod);
			}
//			Debug.Log("FB - homeworld is " + pickedBod);
			allBodNames = allBodies.Select(x=>x.CelName).ToArray();
			Array.Sort<string>(allBodNames);

			GameEvents.onGUIApplicationLauncherReady.Add(CreateLauncher);
			//Hide/show UI event addition
			GameEvents.onHideUI.Add(HideUI);
			GameEvents.onShowUI.Add(ShowUI);
	    }

		private void OnDestroy() {
			DestroyLauncher ();
			GameEvents.onGUIApplicationLauncherReady.Remove (CreateLauncher);
		}

		private void CreateLauncher() {
			FB_TB_posgen = GameDatabase.Instance.GetTexture (FB_TB_posgen_P, false);
			if (ToolbarManager.ToolbarAvailable) {
				// Load toolbar icons
				FB_TB_full = GameDatabase.Instance.GetTexture (FB_TB_full_P, false);
				FB_TB_pos2b = GameDatabase.Instance.GetTexture (FB_TB_pos2b_P, false);
				FB_TB_pos1b = GameDatabase.Instance.GetTexture (FB_TB_pos1b_P, false);
				FB_TB_dr3b = GameDatabase.Instance.GetTexture (FB_TB_dr3b_P, false);
				FB_TB_dr2b = GameDatabase.Instance.GetTexture (FB_TB_dr2b_P, false);
				FB_TB_dr1b = GameDatabase.Instance.GetTexture (FB_TB_dr1b_P, false);
				FB_TB_empty = GameDatabase.Instance.GetTexture (FB_TB_empty_P, false);
//				FB_TB_posgen = GameDatabase.Instance.GetTexture (FB_TB_posgen_P, false);
				FB_TB_drain = GameDatabase.Instance.GetTexture (FB_TB_drain_P, false);
				// init button, add icons etc
				ToolbarButton = ToolbarManager.Instance.add (this.GetType ().Name, "FBToggle");
				ToolbarButton.Text = "Fusebox";
				ToolbarButton.TexturePath = this.FB_TB_full_P;
				ToolbarButton.Visible = true;
				ToolbarButton.OnClick += (
				    (e) => this.uiActive = !this.uiActive
				);
			}
			else if (appLauncherButton == null)
			{
				appLauncherButton = ApplicationLauncher.Instance.AddModApplication (
					delegate {
						uiActive = true;	
					},
					delegate {
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
					FB_TB_posgen
				);
			}
		}

		private void DestroyLauncher() {
			if (appLauncherButton != null) {
				ApplicationLauncher.Instance.RemoveModApplication (appLauncherButton);
				appLauncherButton = null;
			}

			if (ToolbarButton != null) {
				ToolbarButton.Destroy ();
				ToolbarButton = null;
			}
		}

		public void Update()
		{
			sumDelta += Time.deltaTime;  // Only update FB 3 times a second max.
			if (sumDelta > 0.33)
			{
				doOneFrame = true;
				sumDelta = 0;
			}
		}

		//Event when the UI is hidden (F2)
		private void HideUI()
		{
			globalHidden = true;
		}
		
		//Event when the UI is shown (F2)
		private void ShowUI()
		{
			globalHidden = false;
		}

	    private void OnGUI()
	    {
			if (globalHidden)
				return;
			
			if (FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null)  // Check if in flight
			{
				if (FlightGlobals.ActiveVessel.isEVA) // EVA kerbal, do nothing
					return;
	            checkMode(0);
				checkWarp();
			}
	        else if (EditorLogic.fetch != null) // Check if in editor
	            checkMode(1);
	        else   // Not in flight, in editor or F2 pressed unset the mode and return
	        {
	            checkMode(-1);
	            return;
	        }

			if (doOneFrame)
			{
				doOneFrame = !doOneFrame;
	
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
				if (shrinkMain) {
					shrinkMain = false;
					mainWin.height = mainHeight;
				}

				am_max = 0;
				am_cur = 0;
				am_prod = 0;
				am_use = 0;

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

				List<Part> parts = new List<Part> {};
				if (mode == 0)
				{
					parts = FlightGlobals.ActiveVessel.Parts;
				}
				else
				{
					try {
						parts = EditorLogic.fetch.ship.parts; // Saw a comment by Sarbian, I don't need a sorted list anyway.
						if (parts == null)
							return;
					}
					catch (NullReferenceException e)
					{
						if (e.Source != null)
							return;
					}
				}

				PartResourceDefinition definition = PartResourceLibrary.Instance.GetDefinition("ElectricCharge");
	
				foreach (Part p in parts)
	            {
					if (!typeArr[10] && p.Modules[0].moduleName == "LaunchClamp")
						continue; // skip clamps if filtered

					foreach (PartResource r in p.Resources)
	                {
	                    if (r.info.id == definition.id) {
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
							if (typeArr[0])
							{
								ModuleDeployableSolarPanel tmpSol = (ModuleDeployableSolarPanel) tmpPM;
								if (mode == 0)
									am_prod += tmpSol.flowRate;
								else
									am_prod += tmpSol.chargeRate;
							}
							break;
						case "ModuleGenerator":
							if (typeArr[1])
							{
								ModuleGenerator tmpGen = (ModuleGenerator) tmpPM;
//								foreach (ModuleGenerator.GeneratorResource outp in tmpGen.outputList)
								foreach (ModuleResource outp in tmpGen.resHandler.outputResources)
								{
									if (outp.name == "ElectricCharge")
										if (mode == 1)
											am_prod += outp.rate;
										else
											if (tmpGen.isAlwaysActive || tmpGen.generatorIsActive)
												am_prod += outp.rate;
								}
//								foreach (ModuleGenerator.GeneratorResource inp in tmpGen.inputList)
								foreach (ModuleResource inp in tmpGen.resHandler.inputResources)
								{
									if (inp.name == "ElectricCharge")
										if (mode == 1)
											am_use += inp.rate;
										else
											if (tmpGen.isAlwaysActive || tmpGen.generatorIsActive)
												am_use += inp.rate;
								}
							}
							break;
						case "ModuleResourceConverter":
						case "FissionReactor":
							if (typeArr[1])
							{
								ModuleResourceConverter tmpGen = (ModuleResourceConverter) tmpPM;
								foreach (ResourceRatio outp in tmpGen.outputList)
								{
									if (outp.ResourceName == "ElectricCharge")
										if (mode == 1)
											am_prod += outp.Ratio;
									else
										if (tmpGen.AlwaysActive || tmpGen.IsActivated)
											am_prod += outp.Ratio; // might need efficiency in flight
								}
								foreach (ResourceRatio inp in tmpGen.inputList)
								{
									if (inp.ResourceName == "ElectricCharge")
										if (mode == 1)
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
								ModuleResourceHarvester tmpHar = (ModuleResourceHarvester) tmpPM;
								foreach (ResourceRatio outp in tmpHar.outputList)
								{
									if (outp.ResourceName == "ElectricCharge")
										if (mode == 1)
											am_prod += outp.Ratio;
									else
										if (tmpHar.AlwaysActive || tmpHar.IsActivated)
											am_prod += outp.Ratio; // might need efficiency in flight
								}
								foreach (ResourceRatio inp in tmpHar.inputList)
								{
									if (inp.ResourceName == "ElectricCharge")
										if (mode == 1)
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
								ModuleWheels.ModuleWheelMotor tmpWheel = (ModuleWheels.ModuleWheelMotor) tmpPM;
								if (tmpWheel.GetConsumedResources().Find(r => r.name == "ElectricCharge") != null)
								{
//									if (mode == 0 && tmpWheel.drive != 0 )
//										am_use += tmpWheel.resourceConsumptionRate;
//									if (mode == 1)
									am_use += tmpWheel.avgResRate;
								}
							}
							break;
						case "ModuleCommand":
							if (typeArr[3])
							{
								ModuleCommand tmpPod = (ModuleCommand) tmpPM;
//								foreach (ModuleResource r in tmpPod.inputResources)
								foreach (ModuleResource r in tmpPod.resHandler.inputResources)
		                		{
									if (r.id == definition.id) {
										am_use += r.rate;
									}
		                		}
							}
							break;
						case "ModuleLight":
							if (typeArr[4])
							{
								ModuleLight tmpLight = (ModuleLight) tmpPM;
								if (mode == 1 || (mode == 0 && tmpLight.isOn))
									am_use += tmpLight.resourceAmount;
							}
							break;
						case "ModuleDataTransmitter":
							if (typeArr[5])
							{
								ModuleDataTransmitter tmpAnt = (ModuleDataTransmitter) tmpPM;
								if (mode == 1 || (mode == 0 && tmpAnt.IsBusy()))
									am_use += tmpAnt.DataResourceCost * (1 / tmpAnt.packetInterval);
							}
							break;
						case "ModuleReactionWheel":
							if (typeArr[6])
							{
								ModuleReactionWheel tmpRW = (ModuleReactionWheel) tmpPM;
//								foreach (ModuleResource r in tmpRW.inputResources)
								foreach (ModuleResource r in tmpRW.resHandler.inputResources)
		                		{
									if (r.id == definition.id) {
										if (mode == 1)
											am_use += r.rate * tmpRW.PitchTorque;  // rough guess for VAB
										if (mode == 0)
											am_use += r.currentAmount * (tmpRW.PitchTorque + tmpRW.RollTorque + tmpRW.YawTorque);
									}
		                		}
							}
							break;
						case "ModuleEngines":
							ModuleEngines tmpEng = (ModuleEngines) tmpPM;
							if (typeArr[7])
							{
								const float grav = 9.81f;
								bool usesCharge = false;
								float sumRD = 0;
								Single ecratio = 0;
								foreach (Propellant prop in tmpEng.propellants )
								{
									if (prop.name == "ElectricCharge") {
										usesCharge = true;
										ecratio = prop.ratio;
									}
									sumRD += prop.ratio * PartResourceLibrary.Instance.GetDefinition(prop.id).density;
								}
								if (usesCharge)
								{
									float massFlowRate;
									if (mode == 0 && tmpEng.isOperational && tmpEng.currentThrottle > 0)
									{
										massFlowRate = (tmpEng.currentThrottle * tmpEng.maxThrust) / (tmpEng.atmosphereCurve.Evaluate(0) * grav);
										am_use += (ecratio * massFlowRate) / sumRD;
									}
									if (mode == 1 && currThrottle > 0.0)
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
							ModuleEnginesFX tmpEngFX = (ModuleEnginesFX) tmpPM;
							if (typeArr[7])
							{
								const float grav = 9.81f;
								bool usesCharge = false;
								float sumRD = 0;
								Single ecratio =0;
								foreach (Propellant prop in tmpEngFX.propellants )
								{
									if (prop.name == "ElectricCharge") {
										usesCharge = true;
										ecratio = prop.ratio;
									}
									sumRD += prop.ratio * PartResourceLibrary.Instance.GetDefinition(prop.id).density;
								}
								if (usesCharge)
								{
									float massFlowRate;
									if (mode == 0 && tmpEngFX.isOperational && tmpEngFX.currentThrottle > 0)
									{
										massFlowRate = (tmpEngFX.currentThrottle * tmpEngFX.maxThrust) / (tmpEngFX.atmosphereCurve.Evaluate(0) * grav);
										am_use += (ecratio * massFlowRate) / sumRD;
									}
									if (mode == 1 && currThrottle > 0.0)
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
								ModuleAlternator tmpAlt = (ModuleAlternator) tmpPM;
//								foreach (ModuleResource r in tmpAlt.outputResources)
								foreach (ModuleResource r in tmpAlt.resHandler.outputResources)
	                			{
									if (r.name == "ElectricCharge")
									{
										if (mode == 1 || (mode == 0 && currentEngActive))
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
								if (mode == 0)
								{
									ModuleScienceLab tmpLab = (ModuleScienceLab) tmpPM;
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
					}
				}
				if (ToolbarManager.ToolbarAvailable)
					setTBIcon ();
			}

			if (uiActive)
			{
				if (useSmokeSkin)
					GUI.skin = UnityEngine.GUI.skin;
				else
					GUI.skin = HighLogic.Skin;
				
				if (am_max > 0) {
					mainWin = GUILayout.Window(MAINWINID, mainWin, drawFusebox, "Fusebox", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

					if (showFilters)
						filterWin = GUILayout.Window(FILTWINID, filterWin, drawFilters, "Filters", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				
					if (showSettings)
						setWin = GUILayout.Window(SETWINID, setWin, drawSettings, "Settings", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
					
					if (showDarkTime) {
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
							pickBod = !Popup.List(popupWin, ref pickShow, ref pickedBodIdx, new GUIContent ("Pick Body"), allBodNames, "button", "box", listStyle);
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
	
		private void checkAv(PartModule tmpPM)
		{
			switch (tmpPM.moduleName)
			{
			case "ModuleNavLight":
				if (typeArr[4])
				{
					global::AviationLights.ModuleNavLight tmpLight = (global::AviationLights.ModuleNavLight) tmpPM;
					am_use += tmpLight.EnergyReq;
				}
				break;
			}
		}
		
		private void checkNFE(PartModule tmpPM)
		{
			switch (tmpPM.moduleName)
			{
			case "ModuleRadioisotopeGenerator":
				if (typeArr[12])
				{
					global::NearFutureElectrical.ModuleRadioisotopeGenerator tmpGen = (global::NearFutureElectrical.ModuleRadioisotopeGenerator) tmpPM;
					if (tmpGen.PercentPower > 0.0)
						am_prod += tmpGen.BasePower * (tmpGen.PercentPower / 100);
					else
						am_prod += tmpGen.BasePower;
				}
				break;
			}
		}
		
		private void checkNFS(PartModule tmpPM)
		{
			switch (tmpPM.moduleName)
			{
			case "ModuleCurvedSolarPanel":
				if (typeArr[12])
				{
					global::NearFutureSolar.ModuleCurvedSolarPanel tmpSol = (global::NearFutureSolar.ModuleCurvedSolarPanel) tmpPM;
//					if (mode == 0 && (tmpSol.State.Equals(ModuleDeployableSolarPanel.panelStates.EXTENDED)))
					if (mode == 0 && (tmpSol.State.Equals(ModuleDeployableSolarPanel.DeployState.EXTENDED)))
						am_prod += tmpSol.energyFlow;
					else if (mode == 1)
						am_prod += tmpSol.TotalEnergyRate;
				}
				break;
			}
		}

		private void checkKAS(PartModule tmpPM)
		{
			switch (tmpPM.moduleName)
			{
			case "KASModuleWinch":
				if (typeArr[16])
				{
					global::KAS.KASModuleWinch tmpKW = (global::KAS.KASModuleWinch) tmpPM;
					if (mode == 0 && tmpKW.isActive)
						am_use += tmpKW.powerDrain * tmpKW.motorSpeed;
					if (mode == 1)
						am_use += tmpKW.powerDrain;
				}
				break;
			case "KASModuleMagnet":
				if (typeArr[16])
				{
					global::KAS.KASModuleMagnet tmpHM = (global::KAS.KASModuleMagnet) tmpPM;
					if (mode == 1 || (mode == 0 && tmpHM.MagnetActive))
						am_use += tmpHM.powerDrain;
				}
				break;
			}
		}
		
		private void checkRT2(PartModule tmpPM)
		{
			switch (tmpPM.moduleName)
			{
			case "ModuleRTAntenna":
				if (typeArr[11])
				{
					global::RemoteTech.Modules.ModuleRTAntenna tmpAnt = (global::RemoteTech.Modules.ModuleRTAntenna) tmpPM;
					if (mode == 0 && tmpAnt.Activated)
						am_use += tmpAnt.Consumption;
					if (mode == 1)
						am_use += tmpAnt.EnergyCost;
				}
				break;
			}
		}

		private void checkSCANsat(PartModule tmpPM)
		{
//			switch (tmpPM.moduleName)
//			{
//			case "SCANsat":
//				if (typeArr[13])
//				{
//					global::SCANsat.SCAN_PartModules.SCANsat tmpSS = (global::SCANsat.SCAN_PartModules.SCANsat) tmpPM;
//					if ((mode == 1) || (mode == 0 && (tmpSS.power > 0.0 && tmpSS.scanningNow())))
//						am_use += tmpSS.power;
//				}
//				break;
//			case "ModuleSCANresourceScanner":
//				if (typeArr[13])
//				{
//					global::SCANsat.SCAN_PartModules.ModuleSCANresourceScanner tmpSS = (global::SCANsat.SCAN_PartModules.ModuleSCANresourceScanner) tmpPM;
//					if ((mode == 1) || (mode == 0 && (tmpSS.power > 0.0 && tmpSS.scanningNow())))
//						am_use += tmpSS.power;
//				}
//				break;
//			}
		}
		
		private void checkTel(PartModule tmpPM)
	    {
//			switch (tmpPM.moduleName)
//			{
//			case "TelemachusPowerDrain":
//				if (typeArr[5])
//				{
//					if (mode == 0 && global::Telemachus.TelemachusPowerDrain.isActive)
//						am_use += global::Telemachus.TelemachusPowerDrain.powerConsumption;
//					if (mode == 1)
//						am_use += 0.01;
//				}
//				break;
//			}
		}

		private void checkTACL()
		{
//			if (typeArr[14])
//			{
//				global::Tac.TacLifeSupport tmpTLS = Tac.TacLifeSupport.Instance;
//				if (tmpTLS.gameSettings.Enabled) {
//					am_use += tmpTLS.globalSettings.BaseElectricityConsumptionRate;
//				}
//				// Add ElectricityConsumptionRate * crew member later
//			}
		}

		private void checkAntR(PartModule tmpPM)
		{
			switch (tmpPM.moduleName)
			{
			case "ModuleLimitedDataTransmitter":
				if (typeArr[5])
				{
					ModuleDataTransmitter tmpAnt = (ModuleDataTransmitter) tmpPM;
					if (mode == 1 || (mode == 0 && tmpAnt.IsBusy()))
						am_use += tmpAnt.DataResourceCost * (1 / tmpAnt.packetInterval);
				}
				break;
			}
		}

		private void checkkOS(PartModule tmpPM)
		{
//			switch (tmpPM.moduleName)
//			{
//			case "kOSProcessor":
//				if (typeArr[5])
//				{
//					global::kOS.Module.kOSProcessor tmpkOS = (global::kOS.Module.kOSProcessor) tmpPM;
//					am_use += tmpkOS.RequiredPower;
//				}
//				break;
//			}
		}

/*		private void checkBio(PartModule tmpPM)
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

		private void checkKar(PartModule tmpPM)
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

		private void checkBDSM(PartModule tmpPM)
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
			
	    private void checkMode(int curMode)
	    {
			String vers;
			
	        // Check if the mode has changed
	        if (mode != curMode)
	        {
	            //Save the position of the window in the previous mode
				if (mode == 0 || mode == 1)
	            	saveConfig();
	
	            // Change modes
	            mode = curMode;
				shrinkMain = true;  // redraw minimal on scene change
	
	            // Load the saved position of the new mode
	            if (mode >= 0)
	            {
	                FBconf.load();
					vers = FBconf.GetValue<String>("version", VERSION);
					if (vers == VERSION)
					{
		                mainWin = FBconf.GetValue<Rect>("mainWin" + mode, new Rect(Screen.width - mainWidth - 200, mode == 0 ? 1 : 50, mainWidth, mainHeight));
						filterWin = FBconf.GetValue<Rect>("filterWin" + mode, new Rect(Screen.width - filtWidth - 200, mode == 0 ? 1 : 50, filtWidth, filtHeight));
						setWin = FBconf.GetValue<Rect>("setWin" + mode, new Rect(Screen.width - setWidth - 200, mode == 0 ? 1 : 50, setWidth, setHeight));
						drkWin = FBconf.GetValue<Rect>("drkWin" + mode, new Rect(Screen.width - setWidth - 200, mode == 0 ? 1 : 50, drkWidth, drkHeight));
						haltWarp = FBconf.GetValue<bool>("warphalt", true);
						haltWarpThresh = FBconf.GetValue<int>("warphaltthresh", 30);
						showCharge = FBconf.GetValue<bool>("showcharge" + mode, true);
						decPlaces = FBconf.GetValue<int>("precision" + mode, 2);
						useSmokeSkin = FBconf.GetValue<bool> ("useSmokeSkin", true);
						try {
							string fntsty = "";
							fntsty = FBconf.GetValue<string>("globalFontStyle"+mode, "Normal");
							FSGlobal = (FontStyle) Enum.Parse(typeof(FontStyle), fntsty);
						}
						catch (ArgumentException) {
							FSGlobal = FontStyle.Normal;
						}
						GoodCol.r = (float) FBconf.GetValue<double>("goodColr"+mode, 0.0);
						GoodCol.b = (float) FBconf.GetValue<double>("goodColb"+mode, 0.0);
						GoodCol.g = (float) FBconf.GetValue<double>("goodColg"+mode, 1.0);
						BadCol.r = (float) FBconf.GetValue<double>("badColr"+mode, 1.0);
						BadCol.b = (float) FBconf.GetValue<double>("badColb"+mode, 0.0);
						BadCol.g = (float) FBconf.GetValue<double>("badColg"+mode, 0.0);
						OtherCol.r = (float) FBconf.GetValue<double>("otherColr"+mode, 1.0);
						OtherCol.b = (float) FBconf.GetValue<double>("otherColb"+mode, 1.0);
						OtherCol.g = (float) FBconf.GetValue<double>("otherColg"+mode, 1.0);
						for (int i = 0; i < typeArr.Length; i++)
						{
							typeArr[i] = FBconf.GetValue<bool>("filtArr" + mode + i, true);
						}
					}
	            }
	        }
	    }

		private void saveConfig()
	    {
	        // Update the configuration file
	        FBconf.SetValue("version", VERSION);
	        if (mode >= 0) {
	            FBconf.SetValue("mainWin" + mode, mainWin);
				FBconf.SetValue("filterWin" + mode, filterWin);
				FBconf.SetValue("setWin" + mode, setWin);
				FBconf.SetValue("drkWin" + mode, drkWin);
				FBconf.SetValue("warphalt", haltWarp);
				FBconf.SetValue("warphaltthresh", haltWarpThresh);
				FBconf.SetValue("showcharge" + mode, showCharge);
				FBconf.SetValue("precision" + mode, decPlaces);
				FBconf.SetValue("globalFontStyle" + mode, FSGlobal.ToString());
				FBconf.SetValue("goodColr" + mode, (double) GoodCol.r);
				FBconf.SetValue("goodColb" + mode, (double) GoodCol.b);
				FBconf.SetValue("goodColg" + mode, (double) GoodCol.g);
				FBconf.SetValue("badColr" + mode, (double) BadCol.r);
				FBconf.SetValue("badColb" + mode, (double )BadCol.b);
				FBconf.SetValue("badColg" + mode, (double) BadCol.g);
				FBconf.SetValue("otherColr" + mode, (double) OtherCol.r);
				FBconf.SetValue("otherColb" + mode, (double) OtherCol.b);
				FBconf.SetValue("otherColg" + mode, (double) OtherCol.g);
				FBconf.SetValue ("useSmokeSkin", useSmokeSkin);
				for (int i = 0; i < typeArr.Length; i++)
				{
					FBconf.SetValue("filtArr" + mode + i, typeArr[i]);
				}
			}
	        FBconf.save();
	    }
	
	    private void OnApplicationQuit()
	    {
	        // Save configuration file
	        saveConfig();
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
				TimeSpan drainTime = TimeSpan.FromSeconds((int) (am_cur / (am_use - am_prod)));
				int years = 0;
				if (!wasDraining) {
					wasDraining = true;
					frmCount = 1;
				}
				
				if (am_cur == 0)
					GUILayout.Label("Bat cur: 0.0%", FBDrainStyle, GUILayout.ExpandWidth(true));
				else
					GUILayout.Label(string.Concat("Bat cur: ", (am_cur / am_max).ToString("0.0%")), FBDrainStyle, GUILayout.ExpandWidth(true));
				GUILayout.Label(string.Concat("Gen: ", am_prod.ToString(precString)), FBOKStyle, GUILayout.ExpandWidth(true));
				GUILayout.Label(string.Concat("Drain: ", am_use.ToString(precString)), FBDrainStyle, GUILayout.ExpandWidth(true));
				if (drainTime.Days > 365)
					years = (int)drainTime.Days / 365;
				if (drainTime.Days > 0)
					if (drainTime.Days > 365)
					{
						GUILayout.Label(string.Concat("0 in ", string.Format("{0:D2}y:{1:D2}d", years, (drainTime.Days - (years * 365)))), FBDrainStyle, GUILayout.ExpandWidth(true));
					}
					else
					{
						GUILayout.Label(string.Concat("0 in ", string.Format("{0:D2}d:{1:D2}h",drainTime.Days, drainTime.Hours)), FBDrainStyle, GUILayout.ExpandWidth(true));
					}
				else if (drainTime.Hours > 0)
					GUILayout.Label(string.Concat("0 in ", string.Format("{0:D2}h:{1:D2}m",drainTime.Hours, drainTime.Minutes)), FBDrainStyle, GUILayout.ExpandWidth(true));
				else if (drainTime.Minutes > 0)
					GUILayout.Label(string.Concat("0 in ", string.Format("{0:D2}m:{1:D2}s",drainTime.Minutes, drainTime.Seconds)), FBDrainStyle, GUILayout.ExpandWidth(true));
				else if (drainTime.Seconds > 0)
					GUILayout.Label(string.Concat("0 in ", string.Format("{0:D2}s",drainTime.Seconds)), FBDrainStyle, GUILayout.ExpandWidth(true));
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
				if (wasDraining) {
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
			if (GUILayout.Button ("Close"))
				uiActive = false;
//
	       	// Make window draggable
        	GUI.DragWindow();
	    }
		
		private void drawFilters(int windowID)
	    {
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
/*			if (TACLPresent)
				typeArr[14] = GUILayout.Toggle(typeArr[14], "TAC Life Sup", typeArr[14] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			if (KISPPresent)
				typeArr[15] = GUILayout.Toggle(typeArr[15], "Interstellar", typeArr[15] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)); */
			if (KASPresent)
				typeArr[16] = GUILayout.Toggle(typeArr[16], "KAS", typeArr[16] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
//			if (KarPresent)
//				typeArr[18] = GUILayout.Toggle(typeArr[18], "Karbonite", typeArr[18] ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

			if (mode == 1)
			{
				GUILayout.Label(String.Format("Engine thr {0:P0}", currThrottle));
				currThrottle = GUILayout.HorizontalSlider(currThrottle, 0.0F, 1.0F);
			}

			if (GUILayout.Button ("Close"))
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

			useSmokeSkin = GUILayout.Toggle (useSmokeSkin, "Use Smoke Skin");

			haltWarp = GUILayout.Toggle(haltWarp, "Warp halt toggle", haltWarp ? filterOn : filterOff, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			if (haltWarp)
			{
				GUILayout.Label("%age halt. 5-75", TextInp);
				string strThresh =  haltWarpThresh.ToString("F2");
				int tempThresh = haltWarpThresh;
				strThresh = GUILayout.TextField(strThresh, 2);
				if (int.TryParse(strThresh, out tempThresh))
				{
					if (tempThresh >= 5 && tempThresh <= 75)
						haltWarpThresh = tempThresh;
				}
				else if (strThresh == "") haltWarpThresh = 30;
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

			if (GUILayout.Button ("Close"))
				showSettings = false;
			// Make window draggable
        	GUI.DragWindow();
		}
		
		private void checkWarp()
		{
			if (!haltTriggered)
			{
				if ((TimeWarp.CurrentRate > 0) && haltWarp && (((am_cur/am_max) * 100) < haltWarpThresh)) {
					TimeWarp.SetRate(0, true);
					showWarn = true;
					warnRead = false;
					warnPopped = true;
					haltWarp = false;
				}
			}
			if (showWarn) {
				warnWin = GUILayout.Window(WARWINID, warnWin, stopAndWarn, "WARNING!", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
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
			if (GUILayout.Button("OK", GUILayout.MaxWidth(40))) {
                warnRead = true;
				warnPopped = false;
				haltTriggered = true;
			}
		}
		
		private void setTBIcon()
		{
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
				if (frmCount < 75) {
					if (am_perc < 0.1)
					{
						this.ToolbarButton.TexturePath = this.FB_TB_empty_P;
					}
					else
					{
						switch (segs)
						{
						case 1: this.ToolbarButton.TexturePath = this.FB_TB_dr1b_P;
							break;
						case 2: this.ToolbarButton.TexturePath = this.FB_TB_dr2b_P;
							break;
						case 3: this.ToolbarButton.TexturePath = this.FB_TB_dr3b_P;
							break;
						}
					}
				}
				else
				{
					if (frmCount == 75 && showCharge)
						this.ToolbarButton.TexturePath = this.FB_TB_drain_P;
					if (frmCount == 150)
					{
					  frmCount = 1;
					}
				}
			}
			else
			{
				frmCount++;
				if (frmCount < 75) {
					if (am_perc < 0.1)
					{
						this.ToolbarButton.TexturePath = this.FB_TB_empty_P;
					}
					else
					{
						switch (segs)
						{
						case 1: this.ToolbarButton.TexturePath = this.FB_TB_pos1b_P;
							break;
						case 2: this.ToolbarButton.TexturePath = this.FB_TB_pos2b_P;
							break;
						case 3: this.ToolbarButton.TexturePath = this.FB_TB_full_P;
							break;
						}
					}
				}
				else
				{
					if ((frmCount == 75) && (am_perc < 99.0) && showCharge)
						this.ToolbarButton.TexturePath = this.FB_TB_posgen_P;
					if (frmCount == 150)
					{
					  frmCount = 1;
					}
				}
			}
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
			if (GUILayout.Button("Pick")) {
				popupWin.x = drkWin.x;
				popupWin.y = drkWin.y + 120;
                pickBod = !pickBod;
			}
			GUILayout.EndHorizontal();
			GUILayout.Label("Orb alt km:", darkStyle);
			string strOrbit =  celOrbit.ToString("D");
			int tempOrbit = celOrbit;
			strOrbit = GUILayout.TextField(strOrbit, 6, darkStyle);
			if (int.TryParse(strOrbit, out tempOrbit))
			{
				if (tempOrbit > 1 && tempOrbit != celOrbit)
					celOrbit = tempOrbit;
			}
			else if (strOrbit == "") celOrbit = 1;
			
			TimeSpan darkTime = GameSettings.KERBIN_TIME ? ConvToKerb(TimeSpan.FromSeconds((int) getDarkTime())) : TimeSpan.FromSeconds((int) getDarkTime());
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

			TimeSpan darkTimeLanded = GameSettings.KERBIN_TIME ? ConvToKerb(TimeSpan.FromSeconds((int) getDarkTimeLanded())) : TimeSpan.FromSeconds((int) getDarkTimeLanded());
			if (darkTimeLanded.Days > 0)
			{
				darkStr = string.Format("{0:D2}d:{1:D2}h", darkTimeLanded.Days, darkTimeLanded.Hours);
			}
			else if	(darkTimeLanded.Hours > 0)
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

			if (GUILayout.Button ("Close"))
				showDarkTime = false;
			
			GUILayout.EndVertical();
			
			// Make window draggable
        	GUI.DragWindow();
		}

		private TimeSpan ConvToKerb(TimeSpan span)
		{
			const int dayMult = 4;
			const int hoursDay = 6;
			const int secMin = 60;
			int tmpDays = span.Days * dayMult;
			tmpDays += (int) span.Hours / hoursDay;
			int tmpHours = span.Hours % hoursDay;
			return TimeSpan.FromSeconds(((tmpDays * hoursDay * secMin * secMin) + (tmpHours * secMin * secMin) + (span.Minutes * secMin) + span.Seconds));
		}

		private double getDarkTime()
		{
			celBody b = allBodies.Find(x => x.CelName == pickedBod);
			double xyrA = (b.CelRad + celOrbit);
			double xyH = Math.Sqrt(xyrA*b.CelGM);
			double dTime = 2*xyrA*xyrA/xyH*(Math.Asin(b.CelRad/xyrA));
//			Debug.Log ("FB - " + b.CelName + " - " + dTime.ToString() + " sec of darktime");
			return dTime;
		}
		
		private double getDarkTimeLanded()
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
	public class Popup {
	    static int popupListHash = "PopupList".GetHashCode();
	 
	 
	 
	    public static bool List (Rect position, ref bool showList, ref int listEntry, GUIContent buttonContent, object[] list ,
	                             GUIStyle listStyle) {
	 
	 
	 
	        return List(position, ref showList, ref listEntry, buttonContent, list, "button", "box", listStyle);
		}
	 
	    public static bool List (Rect position, ref bool showList, ref int listEntry, GUIContent buttonContent,  object[] list,
	                             GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle) {
	 
	 
	        int controlID = GUIUtility.GetControlID(popupListHash, FocusType.Passive);
	        bool done = false;
	        switch (Event.current.GetTypeForControl(controlID)) {
	            case EventType.mouseDown:
	                if (position.Contains(Event.current.mousePosition)) {
	                    GUIUtility.hotControl = controlID;
	                    showList = true;
	                }
	                break;
	            case EventType.mouseUp:
	                if (showList) {
	                    done = true;
	                }
	                break;
	        }
	 
	        GUI.Label(position, buttonContent, buttonStyle);
	        if (showList) {
	 
				// Get our list of strings
				string[] text = new string[list.Length];
				// convert to string
				for (int i =0; i<list.Length; i++)
				{
					text[i] = list[i].ToString();
				}
	 
	            Rect listRect = new Rect(position.x, position.y + 25, position.width, list.Length * 25);
	            GUI.Box(listRect, "", boxStyle);
	            listEntry = GUI.SelectionGrid(listRect, listEntry, text, 1, listStyle);
	        }
	        if (done) {
	            showList = false;
	        }
	        return done;
	    }
	}

	// By Padimur for RPM integration
	public class FuseBoxMonitor : PartModule
	{
		private double Consumption { get { return Fusebox.am_use; } }
		private double Current { get { return Fusebox.am_cur; } }
		private double Max { get { return Fusebox.am_max; } }
		private double Production { get { return Fusebox.am_prod; } }
		private double Delta { get { return Fusebox.am_prod - Fusebox.am_use; } }
		
		public object ProcessVariable(string input)
		{
			switch (input)
			{
			case "FBAVAILABLE": return 1;
			case "ELECTRICCONSUMPTION": return Consumption;
			case "ELECTRICPRODUCTION": return Production;
			case "ELECTRICDELTA": return Delta;
				
			case "ELECTRICSTATUS":
				
				var cons = Consumption;
				var prod = Production;
				
				if (cons == prod) return "Low usage.";
				
				var charging = prod > cons;
				
				var aim = charging ? "Full" : "Empty";
				
				var timeSpan = charging ? TimeSpan.FromSeconds((double)((int)(Max - Current / (prod - cons)))) : TimeSpan.FromSeconds((double)((int)(Current / (cons - prod))));
				int num = 0;
				if (timeSpan.Days > 365)
				{
					num = timeSpan.Days / 365;
				}
				if (timeSpan.Days > 0)
				{
					if (timeSpan.Days > 365)
						return aim + " in " + string.Format("{0:D2}y:{1:D2}d", num, timeSpan.Days - num * 365);
					else
						return aim + " in " + string.Format("{0:D2}d:{1:D2}h", timeSpan.Days, timeSpan.Hours);
				}
				else
				{
					if (timeSpan.Hours > 0)
						return aim + " in " + string.Format("{0:D2}h:{1:D2}m", timeSpan.Hours, timeSpan.Minutes);
					else
					{
						if (timeSpan.Minutes > 0)
						{
							return aim + " in " + string.Format("{0:D2}m:{1:D2}s", timeSpan.Minutes, timeSpan.Seconds);
						}
						else
						{
							if (timeSpan.Seconds > 0)
								return aim + " in " + string.Format("{0:D2}s", timeSpan.Seconds);
							else
								return charging ? "Full!" : "Empty!";
						}
					}
				}
			default: return null;
			}
		}
	}
}