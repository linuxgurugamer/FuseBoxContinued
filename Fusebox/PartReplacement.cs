using System;
using UnityEngine;
using KSP;
using System.Linq;
using System.Collections.Generic;

using KSP.UI.Screens;

//
// This file is from the Virgin Kalactic mod
// https://github.com/Greys0/Virgin-Kalactic
// License is MIT
//
namespace Ratzap
{

    public class PartTapIn : Part
    {
#if true
        //
        // The following is here because it is a private function in the Part class
        // and therefore is not being activated or called
        //
        VesselRenameDialog renameDialog;

        [KSPEvent(guiActiveUncommand = false, guiActive = false, guiActiveEditor = false, guiName = "#autoLOC_8003140")]
        void MySetVesselNaming()
        {
            if (renameDialog != null)
                return;

            InputLockManager.SetControlLock("vesselRenameDialog");
            renameDialog = VesselRenameDialog.SpawnNameFromPart(this, myonVesselNamingAccept, myonVesselNamingDismiss, myonVesselNamingRemove, true, VesselType.Debris);
        }
        void myonVesselNamingAccept(string newVesselName, VesselType newVesselType, int newPriority)
        {
            if (!Vessel.IsValidVesselName(newVesselName))
                return;

            if (vesselNaming == null)
                vesselNaming = new VesselNaming();
            vesselNaming.vesselName = newVesselName;
            vesselNaming.vesselType = newVesselType;
            vesselNaming.namingPriority = newPriority;
            GameEvents.onPartVesselNamingChanged.Fire(this);
            myonVesselNamingDismiss();
        }
        void myonVesselNamingDismiss()
        {
            InputLockManager.RemoveControlLock("vesselRenameDialog");
        }
        void myonVesselNamingRemove()
        {
            vesselNaming = null;
        }
#if true
        //
        // For some reason, Start() causes Kerbalism to spam the log with errors
        // So using LateUpdate is a compromise
        // This bug will be fixed in the next release of KSP, so restrict this only to 1.4.3
        //
        public new void LateUpdate()
        {
            if (Versioning.version_major == 1 && Versioning.version_minor == 4 && Versioning.Revision == 3)
            {
                Log.Info("PartReplacement.LateUpdate");
                if (HighLogic.LoadedSceneIsEditor)
                {
                    // In case this gets fixed in the future, don't show my version
                    if (!Events.Contains("SetVesselNaming") && this.Modules.Contains("ModuleCommand"))
                        Events["MySetVesselNaming"].guiActiveEditor = true;
                }
                else
                {
                    if (GameSettings.SHOW_VESSEL_NAMING_IN_FLIGHT && !Events.Contains("SetVesselNaming") && this.Modules.Contains("ModuleCommand"))
                        Events["MySetVesselNaming"].guiActiveUncommand = true;
                }
            }
        }

        // End of code for vessel naming

#endif
#endif
        public PartTapIn()
        {
            OnRequestResource = new PartEventTypes.Ev4Arg<string, double, ResourceFlowMode, double>();
            //OnResourceRequested = new PartEventTypes.SingleCallBack3Arg<double, string, double, ResourceFlowMode> (base.RequestResource);
        }

        // Fires after RequestResource() transaction is completed
        // string resourceName, double resourceDemand, ResourceFlowMode, double resourceConsumed
        public PartEventTypes.Ev4Arg<string, double, ResourceFlowMode, double> OnRequestResource;

        // Fires when RequestResource() is called, single delegate must satisfy the transaction
        // string return consumedAmount, string resourceName, double resourceDemand, ResourceFlowMode
        //public PartEventTypes.SingleCallBack3Arg<double, string, double, ResourceFlowMode> OnResourceRequested;

        // Request Resource Funnel
        [Obsolete]
        public override float RequestResource(int resourceID, float demand)
        {
            return (float)RequestResource(resourceID, (double)demand);
            // Recast's Float, sends Int
        }

        [Obsolete]
        public override float RequestResource(string resourceName, float demand)
        {
            return (float)RequestResource(resourceName, (double)demand);
            // Recast's Float, Sends String
        }

        public override double RequestResource(int resourceID, double demand)
        {

            return RequestResource(PartResourceLibrary.Instance.GetDefinition(resourceID).name, demand);
            // Finds resource name, sends Double
        }

        public override double RequestResource(string resourceName, double demand)
        {
            return RequestResource(resourceName, demand, PartResourceLibrary.Instance.GetDefinition(resourceName).resourceFlowMode);
            // Finds default flow mode, sends string and double
        }

        public override double RequestResource(int resourceID, double demand, ResourceFlowMode flowMode)
        {
            return RequestResource(PartResourceLibrary.Instance.GetDefinition(resourceID).name, demand, flowMode);
            // Finds Resource Name, send's demand and flowMode
        }

        public override double RequestResource(string resourceName, double demand, ResourceFlowMode flowMode)
        {
            // Pass transaction data to designated handler class
            double accepted = 0;
            //  if (OnResourceRequested != null)
            {
                //accepted = this.OnResourceRequested.Invoke(resourceName, demand, flowMode);
                accepted = base.RequestResource(resourceName, demand, flowMode);
            }

            // Send results of transaction to any classes that have asked to be told about it
            if (OnRequestResource != null)
            {
                this.OnRequestResource.Invoke(resourceName, demand, flowMode, accepted);
            }

            // Complete transaction by returning the amount of resource that was actually consumed
            return accepted;
        }

    }

    public class PartEventTypes
    {
        // Event passes 4 arguments to delegate, takes no returns
        public class Ev4Arg<A, B, C, D>
        {
            public delegate void OnEvent(A arg1, B arg2, C arg3, D arg4);

            private OnEvent membership;

            public void Add(OnEvent evt)
            {
                membership += evt;

            }

            public void Remove(OnEvent evt)
            {
                membership -= evt;
            }

            public void Invoke(A arg1, B arg2, C arg3, D arg4)
            {
                if (membership != null)
                {
                    membership(arg1, arg2, arg3, arg4);
                }
            }
        }
#if false
        // Event passes 3 arguments to single delegate, use for operations where one method must complete a necessary action
        public class SingleCallBack3Arg<A,B,C,D>
		{
			public SingleCallBack3Arg (OnEvent Default)
			{
				this.Set(Default);
			}
			
			public delegate A OnEvent (B arg1, C arg2, D arg3);
			
			private OnEvent callback;
			
			public void Set (OnEvent evt)
			{
				callback = evt;
			}
			
			public A Invoke (B arg1, C arg2, D arg3)
			{
				return callback(arg1, arg2, arg3);
			}
			
		}
#endif
    }

}