using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using KSP;
//using PartReplacement;
using Ratzap;

#if true
//
// This file is from the Virgin Kalactic mod
// https://github.com/Greys0/Virgin-Kalactic
// License is MIT
//
namespace TrackResource
{

    [KSPAddon(KSPAddon.Startup.Flight, true)]
    public class VesselStatsManager : MonoBehaviour
    {
        private static Dictionary<Vessel, ResourceStats> vesselDict = new Dictionary<Vessel, ResourceStats>();
        public static VesselStatsManager Instance;
        public static HashSet<string> moduleFilterList = new HashSet<string>();

        public VesselStatsManager()
        {
            Instance = this;
        }

        public void Start()
        {
            Log.Info("TrackResource.Start");
            GameEvents.onVesselGoOffRails.Add(OnVesselGoOffRails);
            GameEvents.onVesselWillDestroy.Add(Remove);
            GameEvents.onTimeWarpRateChanged.Add(Check);
            GameEvents.onVesselWasModified.Add(Reload);
            GameEvents.OnVesselRollout.Add(OnVesselRollout);
            

            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onVesselLoaded.Add(OnVesselLoad);
            GameEvents.onVesselCreate.Add(OnVesselCreate);
            GameEvents.onVesselDestroy.Add(onVesselDestroy);
            GameEvents.onVesselRecovered.Add(onVesselRecovered);
            DontDestroyOnLoad(this);
        }
        private void OnVesselRollout(ShipConstruct sc)
        {
            Log.Info("OnVesselRollout");
        }
        private void OnVesselGoOffRails(Vessel newvessel)
        {
            Log.Info("OnVesselGoOffRails");
            Add(newvessel);
        }
        private void OnVesselChange(Vessel newvessel)
        {
            Log.Info("OnVesselChange");
            Add(newvessel);
        }
        private void OnVesselLoad(Vessel newvessel)
        {
            Log.Info("OnVesselLoad");
            Add(newvessel);
        }

        private void OnVesselCreate(Vessel Vessel)
        {
            Log.Info("OnVesselCreate");
            Add(Vessel);
        }
        private void onVesselDestroy(Vessel vessel)
        {
            Log.Info("onVesselDestroy");
            Remove(vessel);
        }
        private void onVesselRecovered(ProtoVessel vessel, bool quick)
        {
            Log.Info("OnVesselRecovered");
            // Remove(vessel);
        }

        public void onDestroy()
        {
            GameEvents.onVesselGoOffRails.Remove(OnVesselGoOffRails);
            GameEvents.onVesselWillDestroy.Remove(Remove);
            GameEvents.onTimeWarpRateChanged.Remove(Check);
            GameEvents.onVesselWasModified.Remove(Reload);

            GameEvents.onVesselChange.Remove(OnVesselChange);
            GameEvents.onVesselLoaded.Remove(OnVesselLoad);
            GameEvents.onVesselCreate.Remove(OnVesselCreate);
            GameEvents.onVesselDestroy.Remove(onVesselDestroy);
            GameEvents.onVesselRecovered.Remove(onVesselRecovered);
        }

        void Reload(Vessel v)
        {
            Remove(v);
            Add(v);
        }

        public void SetFilterList(HashSet<string> filterList)
        {
            moduleFilterList = new HashSet<string>(filterList);
            foreach (var s in moduleFilterList)
                Log.Info("Filtering out: " + s);
            Reload(FlightGlobals.ActiveVessel);
        }

        public void ClearFilterList()
        {
            moduleFilterList.Clear();
            Reload(FlightGlobals.ActiveVessel);
        }

        void Add(Vessel v)
        {
            Log.Info("VesselStatsManager.Add");
            bool b;
            if (v == null)
                return;
            Log.Info("Add: " + v.name);
            if (moduleFilterList == null)
            {
                Log.Info("modulefilterList is null");
                return;
            }
            bool b1 = true, b2 = true;
            if (v.name.Length >= 9)
                b1 = (v.name.Substring(0, 9) != "kerbalEVA");
            if (v.name.Length >= 4)
                b2 = (v.name.Substring(0, 4) != "flag");
            //if ((v.name.Length >= 9 && v.name.Substring(0, 9) != "kerbalEVA") && (v.name.Length >= 4 && v.name.Substring(0, 4) != "flag") || 
            //    v.name.Length < 4)
            if (b1 && b2)
            {

                Log.Info("ModuleFilterList.count: " + moduleFilterList.Count().ToString());
                if (!vesselDict.ContainsKey(v))
                {
                    ResourceStats r = VesselStatsManager.Instance.gameObject.AddComponent<ResourceStats>();
                    if (r == null)
                        Log.Info("r == null");
                    vesselDict.Add(v, r);
                    foreach (PartTapIn part in v.Parts)
                    {
                        b = true;
                        foreach (var s in moduleFilterList)
                        {
                            if (part.Modules.Contains(s))
                            {
                                b = false;
                                break;
                            }
                        }
                        if (b)
                        {
                            Log.Info("Add 2");
                            part.OnRequestResource.Add(r.Sample);
                        }
                    }
                }
            }
        }

        public void Remove(Vessel v)
        {
            if (vesselDict.ContainsKey(v))
            {
                ResourceStats r = vesselDict[v];

                foreach (PartTapIn part in v.parts)
                {
                    part.OnRequestResource.Remove(r.Sample);
                }

                vesselDict.Remove(v);


            }
        }


        private void Check()
        {
            try
            {
                if (TimeWarp.CurrentRateIndex == 0)
                {
                    foreach (KeyValuePair<Vessel, ResourceStats> pair in vesselDict)
                    {
                        if (!pair.Key.loaded)
                        {
                            Remove(pair.Key);
                            Debug.Log("Vessel No Longer In Range Upon Leaving Timewarp" + pair.Key.name);
                        }
                    }
                }
            }
            catch { }
        }

        public ResourceStats GetOrAdd(Vessel v)
        {
            Log.Info("ResourceStats");
            if (v == null)
            {
                Log.Info("vessel is null");
                return null;
            }
            ResourceStats result;
            Log.Info("before TryGetValue");
            if (!vesselDict.TryGetValue(v, out result))
            {
                Log.Info("Adding vessel: " + v.name);
                Add(v);
                result = vesselDict[v];
            }
            Log.Info("After TryGetValue");
            return result;
        }
    }

    public class ResourceStats : MonoBehaviour
    {
        private Dictionary<string, double> consumption = new Dictionary<string, double>();
        private Dictionary<string, double> generation = new Dictionary<string, double>();
        private Dictionary<string, double> sumConsumption = new Dictionary<string, double>();
        private Dictionary<string, double> sumGeneration = new Dictionary<string, double>();

        public void Sample(string resourceName, double demand, ResourceFlowMode FlowMode, double accepted)
        {
            if (demand == 0)
            {
                return;
            }

            if (demand > 0)
            {
                if (!sumConsumption.ContainsKey(resourceName))
                {
                    sumConsumption.Add(resourceName, demand);
                    return;
                }
                sumConsumption[resourceName] += demand;
            }
            else
            {

                if (!sumGeneration.ContainsKey(resourceName))
                {
                    sumGeneration.Add(resourceName, demand);
                    return;
                }
                sumGeneration[resourceName] += demand;
            }
        }

        public double GetConsumption(string resourceName)
        {
            if (sumConsumption.ContainsKey(resourceName))
            {
                return sumConsumption[resourceName];
            }
            else
            {
                return 0;
            }
        }

        public double GetGeneration(string resourceName)
        {
            if (sumGeneration.ContainsKey(resourceName))
            {
                return sumGeneration[resourceName];
            }
            else
            {
                return 0;
            }
        }

        public void Reset()
        {
            Log.Info("TrackResource.Reset");
            consumption = sumConsumption;
            generation = sumGeneration;

            sumConsumption = new Dictionary<string, double>();
            sumGeneration = new Dictionary<string, double>();
        }
    }
}

#endif