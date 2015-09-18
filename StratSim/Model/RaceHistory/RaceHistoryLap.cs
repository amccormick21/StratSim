using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StratSim.Model.RaceHistory
{
    public enum RaceLapAction
    {
        Clear,
        Overtake,
        Stuck,
        Retire
    }

    public class RaceHistoryLap
    {
        public RaceHistoryLap(string representation)
        {
            DriverIndex = Convert.ToInt32(representation.Split(' ')[0]);
            LapIndexInRace = Convert.ToInt32(representation.Split(new char[] { ' ', ':' })[2]);
            LapTime = float.Parse(representation.Split(new char[] { ':', ',' })[1]);
            LapStatus = Convert.ToInt32(representation.Split(new char[] { ' ', '(' })[3]);
            string lapActionString = representation.Split(new char[] { '(', ')' })[1];
            switch (lapActionString)
            {
                case "Clear": LapAction = RaceLapAction.Clear; break;
                case "Overtake": LapAction = RaceLapAction.Overtake; break;
                case "Stuck": LapAction = RaceLapAction.Stuck; break;
                case "Retire": LapAction = RaceLapAction.Retire; break;
                default: LapAction = RaceLapAction.Clear; break;
            }
            string lapDeficitString = representation.Split(new char[] { '(', '-', ')' }, StringSplitOptions.RemoveEmptyEntries)[3];
            LapDeficit = Convert.ToInt32(lapDeficitString);
        }
        public RaceHistoryLap(int driverIndex, int lapIndex, float lapTime, int lapStatus, RaceLapAction lapAction, int lapDeficit)
        {
            DriverIndex = driverIndex;
            LapIndexInRace = lapIndex;
            LapTime = lapTime;
            LapStatus = lapStatus;
            LapAction = lapAction;
            LapDeficit = lapDeficit;
        }
        public int DriverIndex { get; private set; }
        /// <summary>
        /// Zero based lap-index in the race (i.e. final lap of 60 lap race is index 59)
        /// </summary>
        public int LapIndexInRace { get; private set; }
        public float LapTime { get; private set; }
        /// <summary>
        /// 0 is out-lap, 1 is normal, 2 is in-lap
        /// </summary>
        public int LapStatus { get; private set; }
        public RaceLapAction LapAction { get; private set; }
        /// <summary>
        /// Number of laps behind the lead lap. This is always positive.
        /// </summary>
        public int LapDeficit { get; private set; }

        public override string ToString()
        {
            string representation = "";
            representation += DriverIndex.ToString() + " - ";
            representation += LapIndexInRace.ToString() + ":" + LapTime.ToString();
            representation += ", " + LapStatus.ToString() + "(" + LapAction.ToString() + ")";
            representation += "(-" + LapDeficit.ToString() + ")";
            return representation;
        }

        /// <summary>
        /// Comparer for two race history laps. Assumes the times are cumulative.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Compare(RaceHistoryLap a, RaceHistoryLap b)
        {
            if (a.LapDeficit != b.LapDeficit)
            {
                return a.LapDeficit > b.LapDeficit;
            }
            else
            {
                //If b is zero, don't swap
                if (b.LapTime.Equals(0F))
                    return false;
                else if (a.LapTime.Equals(0F))
                    return true;
                else
                    return (a.LapTime > b.LapTime);
            }
        }

        internal void SetLapStatus(int lapStatus)
        {
            LapStatus = lapStatus;
        }
        internal void SetLapAction(RaceLapAction lapAction)
        {
            LapAction = lapAction;
        }
        internal void SetLapDeficit(int lapDeficit)
        {
            LapDeficit = lapDeficit;
        }

        internal void SetLapTime(float lapTime)
        {
            LapTime = lapTime;
        }
    }
}
