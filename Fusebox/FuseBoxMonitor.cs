using System;
using KSP.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KSP.UI.Screens;


namespace Ratzap
{
    // By Padimur for RPM integration
    public class FuseBoxMonitor : PartModule
    {
        private double Consumption { get { return FuseBox_Flight.am_use; } }
        private double Current { get { return FuseBox_Flight.am_cur; } }
        private double Max { get { return FuseBox_Flight.am_max; } }
        private double Production { get { return FuseBox_Flight.am_prod; } }
        private double Delta { get { return FuseBox_Flight.am_prod - FuseBox_Flight.am_use; } }

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
