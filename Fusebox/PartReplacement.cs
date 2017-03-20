using System;
using UnityEngine;
using KSP;
using System.Linq;
using System.Collections.Generic;

//
// This file is from the Virgin Kalactic mod
// https://github.com/Greys0/Virgin-Kalactic
// License is MIT
//
namespace PartReplacement
{
	
	public class PartTapIn : Part
	{
		
		public PartTapIn ()
		{
			OnRequestResource = new PartEventTypes.Ev4Arg<string, double, ResourceFlowMode, double>();
			OnResourceRequested = new PartEventTypes.SingleCallBack3Arg<double, string, double, ResourceFlowMode> (base.RequestResource);
		}
		
		// Fires after RequestResource() transaction is completed
		// string resourceName, double resourceDemand, ResourceFlowMode, double resourceConsumed
		public PartEventTypes.Ev4Arg<string, double, ResourceFlowMode, double> OnRequestResource;
		
		// Fires when RequestResource() is called, single delegate must satisfy the transaction
		// string return consumedAmount, string resourceName, double resourceDemand, ResourceFlowMode
		public PartEventTypes.SingleCallBack3Arg<double, string, double, ResourceFlowMode> OnResourceRequested;

		// Request Resource Funnel
		public override float RequestResource (int resourceID, float demand)
		{
			return (float)RequestResource (resourceID, (double)demand);
			// Recast's Float, sends Int
		}
		 
		public override float RequestResource (string resourceName, float demand)
		{
			return (float)RequestResource (resourceName, (double)demand);
			// Recast's Float, Sends String
		}
		
		public override double RequestResource (int resourceID, double demand)
		{
			return RequestResource (PartResourceLibrary.Instance.GetDefinition (resourceID).name, demand);
			// Finds resource name, sends Double
		}
		
		public override double RequestResource (string resourceName, double demand)
		{
			return RequestResource (resourceName, demand, PartResourceLibrary.Instance.GetDefinition (resourceName).resourceFlowMode);
			// Finds default flow mode, sends string and double
		}
		
		public override double RequestResource (int resourceID, double demand, ResourceFlowMode flowMode)
		{
			return RequestResource (PartResourceLibrary.Instance.GetDefinition (resourceID).name, demand, flowMode);
			// Finds Resource Name, send's demand and flowMode
		}
		
		public override double RequestResource (string resourceName, double demand, ResourceFlowMode flowMode)
		{
            // Pass transaction data to designated handler class
            double accepted = 0;
            if (OnResourceRequested != null)
                accepted = this.OnResourceRequested.Invoke (resourceName, demand, flowMode);
			
			// Send results of transaction to any classes that have asked to be told about it
            if (OnRequestResource != null)
    			this.OnRequestResource.Invoke(resourceName, demand, flowMode, accepted);
			
			// Complete transaction by returning the amount of resource that was actually consumed
			return accepted;
		}
		
	}
	
	public class PartEventTypes
	{
		// Event passes 4 arguments to delegate, takes no returns
		public class Ev4Arg<A,B,C,D>
		{
			public delegate void OnEvent (A arg1, B arg2, C arg3, D arg4);
			
			private OnEvent membership;
			
			public void Add (OnEvent evt)
			{
				membership += evt;
				
			}
			
			public void Remove (OnEvent evt)
			{
				membership -= evt;
			}
			
			public void Invoke (A arg1, B arg2, C arg3, D arg4)
			{
				if (membership != null)
				{
					membership(arg1, arg2, arg3, arg4);
				}
			}
		}
		
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
	}
	
}