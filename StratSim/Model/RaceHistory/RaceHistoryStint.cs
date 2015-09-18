using DataSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StratSim.Model.RaceHistory
{
    public class RaceHistoryStint
    {
        public TyreType TyreType { get; set; }
        public List<float> LapTimes { get; private set; }
        public int StartLapIndex { get; set; }
        public int StintLength
        {
            get { return LapTimes.Count; }
        }

        public RaceHistoryStint(TyreType tyreType, int firstLapIndex)
        {
            TyreType = tyreType;
            StartLapIndex = firstLapIndex;
            LapTimes = new List<float>();
        }

        public RaceHistoryStint(TyreType tyreType, int firstLapIndex, List<float> lapTimes)
        {
            TyreType = tyreType;
            StartLapIndex = firstLapIndex;
            LapTimes = new List<float>(lapTimes);
        }

        public void AddLap(float lapTime)
        {
            LapTimes.Add(lapTime);
        }

        public override string ToString()
        {
            return StintLength.ToString() + (StintLength == 1 ? " Lap" : " Laps") + ", " + TyreType.ToString();
        }

        internal float TotalTime()
        {
            float totalTime = 0;
            foreach (var lap in LapTimes)
            {
                totalTime += lap;
            }
            return totalTime;
        }

        private List<float> GetLapsBasedOnOtherStint(RaceHistoryStint other, int stintIndex, int numberOfStints, int length, float[] lapLosses)
        {
            return GetLapsBasedOnOtherStint(other, stintIndex, numberOfStints, length, lapLosses, new List<RaceHistoryStint> { this });
        }

        internal static List<float> GetLapsBasedOnOtherStint(RaceHistoryStint other, int stintIndex, int numberOfStints, int length, float[] lapLosses, List<RaceHistoryStint> stints)
        {
            //Uses the same model as the other stint
            float[] parameters;
            bool trendlineAvailable = GetStintTrendline(stints, other.TyreType, out parameters);
            if (!trendlineAvailable)
                trendlineAvailable = GetStintTrendline(stints, out parameters);

            List<float> lapTimes = new List<float>();
            float lapTime;
            for (int lapIndex = 0; lapIndex < length; lapIndex++)
            {
                if (trendlineAvailable)
                {
                    lapTime = parameters[0] * lapIndex + parameters[1];
                    if (lapIndex == 0 && stintIndex > 0)
                        lapTime += lapLosses[0];
                    else if (lapIndex == length - 1 && stintIndex < numberOfStints - 1)
                        lapTime += lapLosses[1];
                    lapTimes.Add(lapTime);
                }
                else
                {
                    if (lapIndex < other.LapTimes.Count)
                        lapTimes.Add(other.LapTimes[lapIndex]);
                    else
                        lapTimes.Add(other.AverageLapTime());
                }
            }
            return lapTimes;
        }

        /// <summary>
        /// Extends a stint by a given number of laps, and applies the given time offset
        /// to every lap in the stint.
        /// </summary>
        /// <returns></returns>
        private List<float> Extrapolate(int lengthChange, int stintIndex, int numberOfStints, float timeOffset, float inLapLoss)
        {
            return Extrapolate(lengthChange, stintIndex, numberOfStints, timeOffset, inLapLoss, new List<RaceHistoryStint> { this });
        }

        internal List<float> Extrapolate(int lengthChange, int stintIndex, int numberOfStints, float timeOffset, float inLapLoss, List<RaceHistoryStint> stints)
        {
            List<float> newLapTimes = new List<float>();
            int oldStintLength = StintLength;
            int newLength = oldStintLength + lengthChange;
            float[] parameters;
            bool trendlineAvailable = GetStintTrendline(stints, TyreType, out parameters);
            if (!trendlineAvailable)
                trendlineAvailable = GetStintTrendline(stints, out parameters);

            float newLapTime;
            for (int lapIndex = 0; lapIndex < newLength; lapIndex++)
            {
                if (lapIndex < oldStintLength - 1) //Don't add the lap that includes the in-lap
                {
                    newLapTime = LapTimes[lapIndex] + timeOffset;
                }
                else
                {
                    if (trendlineAvailable)
                    {
                        newLapTime = parameters[0] * lapIndex + parameters[1] + timeOffset;
                        if (lapIndex == newLength - 1 && stintIndex < numberOfStints - 1)
                            newLapTime += inLapLoss;
                    }
                    else
                    {
                        newLapTime = LapTimes.Find(l => l <= AverageLapTime());
                    }
                }
                newLapTimes.Add(newLapTime);
            }
            return newLapTimes;
        }

        /// <summary>
        /// Shortens a stint by a given number of laps, and applies the given time offset
        /// to every lap in the stint. Only ever passed a negative length change
        /// </summary>
        /// <returns></returns>
        internal List<float> Truncate(int lengthChange, int stintIndex, int numberOfStints, float timeOffset, float inLapLoss)
        {
            List<float> newLapTimes = new List<float>();
            int oldStintLength = StintLength;
            int newLength = oldStintLength + lengthChange;
            float newLapTime;
            for (int lapIndex = 0; lapIndex < newLength; lapIndex++)
            {
                newLapTime = LapTimes[lapIndex] + timeOffset;
                //If this is the final lap, add the pit stop loss
                if (lapIndex == newLength - 1 && stintIndex < numberOfStints - 1)
                    newLapTime += inLapLoss;
                newLapTimes.Add(newLapTime);
            }
            return newLapTimes;
        }

        private float Interpolate(int lapIndexInRace)
        {
            return Interpolate(lapIndexInRace, new List<RaceHistoryStint> { this });
        }

        internal float Interpolate(int lapIndexInRace, List<RaceHistoryStint> stints)
        {
            float lapTime;
            int lapIndexInStint = lapIndexInRace - StartLapIndex;
            float[] trendline;
            if (GetStintTrendline(stints, TyreType, out trendline))
            {
                lapTime = trendline[1] + lapIndexInStint * trendline[0];
            }
            else if (GetStintTrendline(stints, out trendline))
            {
                lapTime = trendline[1] + lapIndexInStint * trendline[0];
            }
            else
            {
                lapTime = LapTimes.Find(l => l <= AverageLapTime());
            }
            return lapTime;
        }

        private bool TrendlineAvailable()
        {
            int goodLaps = 0;
            float averageLapTime = AverageLapTime();
            for (int lapIndex = 0; lapIndex < StintLength; lapIndex++)
            {
                if (CanUseLap(averageLapTime, lapIndex))
                    goodLaps++;
            }
            return goodLaps > 1;
        }

        private bool CanUseLap(float averageLapTime, int lapIndex)
        {
            //Add lap if it is below average, automatically excluding slow in-laps, etc.
            return LapTimes[lapIndex] < 1.035 * averageLapTime;
        }

        private bool GetStintTrendline(out float[] stintTrendline)
        {
            List<float> lapTimes = new List<float>();
            List<int> lapNumbers = new List<int>();
            float gradient = 0;
            float intercept = 0;
            float averageLapTime = AverageLapTime();
            bool trendlineAvailable = TrendlineAvailable();
            if (trendlineAvailable)
            {
                for (int lapIndexInStint = 0; lapIndexInStint < StintLength; lapIndexInStint++)
                {
                    if (CanUseLap(averageLapTime, lapIndexInStint))
                    {
                        lapNumbers.Add(lapIndexInStint);
                        lapTimes.Add(LapTimes[lapIndexInStint]);
                    }
                }
                float[,] points = new float[lapNumbers.Count, 2];
                for (int i = 0; i < lapNumbers.Count; i++)
                {
                    points[i, 0] = lapNumbers[i];
                    points[i, 1] = lapTimes[i];
                }
                Functions.LinearLeastSquaresFit(points, out gradient, out intercept);
            }
            stintTrendline = new float[] { gradient, intercept };
            return trendlineAvailable;
        }

        internal static bool GetStintTrendline(List<RaceHistoryStint> stints, out float[] averageStintTrendline)
        {
            //Find the stints to use
            var stintsToUse = stints.FindAll(s => s.TrendlineAvailable());
            var stintTrendline = new float[2];
            averageStintTrendline = new float[] { 0, 0 };
            if (stintsToUse.Count > 0)
            {
                foreach (var stint in stintsToUse)
                {
                    stint.GetStintTrendline(out stintTrendline);
                    averageStintTrendline[0] += stintTrendline[0];
                    averageStintTrendline[1] += stintTrendline[1];
                }
                averageStintTrendline[0] /= stintsToUse.Count;
                averageStintTrendline[1] /= stintsToUse.Count;
                return true;
            }
            else
                return false;
        }

        internal static bool GetStintTrendline(List<RaceHistoryStint> stints, TyreType tyreType, out float[] stintTrendline)
        {
            // Narrow down the stints by tyre
            var stintsToUse = stints.FindAll(s => s.TyreType == tyreType);
            return GetStintTrendline(stintsToUse, out stintTrendline);
        }

        internal static float[] GetPitLosses(List<RaceHistoryStint> stints)
        {
            float[] stintTrendline;
            bool trendlineAvailable = GetStintTrendline(stints, out stintTrendline);
            float[] pitLosses = new float[2];
            float[] averagePitLosses = new float[] { 0, 0 };
            if (trendlineAvailable)
            {
                foreach (var stint in stints)
                {
                    pitLosses = stint.GetPitLosses(stintTrendline);
                    averagePitLosses[0] += pitLosses[0];
                    averagePitLosses[1] += pitLosses[1];
                }
                averagePitLosses[0] /= stints.Count;
                averagePitLosses[1] /= stints.Count;
            }
            return averagePitLosses;
        }

        private float[] GetPitLosses()
        {
            float[] stintTrendline;
            GetStintTrendline(out stintTrendline);
            return GetPitLosses(stintTrendline);
        }

        internal float[] GetPitLosses(float[] stintTrendline)
        {
            float outLapLoss = LapTimes[0] - stintTrendline[1];
            float inLapLoss = LapTimes[LapTimes.Count - 1] - (LapTimes.Count - 1) * stintTrendline[0] - stintTrendline[1];
            return new float[] { outLapLoss, inLapLoss };
        }

        /// <summary>
        /// Gets the average lap time of a stint, excluding the first and last laps
        /// </summary>
        private float AverageLapTime()
        {
            float totalTime = 0;
            int count = 0;
            for (int lapIndexInStint = 1; lapIndexInStint < StintLength - 1; lapIndexInStint++)
            {
                totalTime += LapTimes[lapIndexInStint];
                count++;
            }
            return totalTime / count;
        }
    }
}
