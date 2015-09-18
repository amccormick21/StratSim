using System;
using System.Collections.Generic;

namespace StratSim.Model
{
    public class Stint : ICloneable
    {
        //properties
        int session;
        public int startLap;
        public int stintLength;
        public TyreType tyreType;
        public bool modified;
        public List<float> lapTimes;

        /// <summary>
        /// Creates a new, empty instance of the stint class.
        /// </summary>
        /// <param name="passSession">The session in which the stint takes place</param>
        /// <param name="passStartLap">The lap on which the stint starts</param>
        public Stint(int passSession, int passStartLap)
        {
            session = passSession;
            startLap = passStartLap;
            lapTimes = new List<float>();
            stintLength = 0;
            modified = false;
        }

        /// <summary>
        /// Creates a new instance of the stint class specifically for use in strateiges
        /// </summary>
        /// <param name="_driver">The driver to link to the stint</param>
        /// <param name="passStartLap">The lap on which the stint starts</param>
        /// <param name="passTyreType">The tyre type used for the stint</param>
        /// <param name="_stintLength">The number of laps in the stint</param>
        public Stint(int passStartLap, TyreType passTyreType, int _stintLength)
        {
            session = 5; //Must be the race
            startLap = passStartLap;
            lapTimes = new List<float>();
            tyreType = passTyreType;
            stintLength = _stintLength;
            modified = false;
        }

        public Stint()
        { }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public static Stint operator +(Stint stint, float lapTimeToAdd)
        {
            stint.AddLap(lapTimeToAdd);
            return stint;
        }

        public static List<Stint> operator +(List<Stint> sessionStints, Stint stintToAdd)
        {
            sessionStints.Add(stintToAdd);
            return sessionStints;
        }

        void AddLap(float lapTime)
        {
            lapTimes.Add(lapTime);
            stintLength++;
        }

        /// <summary>
        /// Writes data about the stint to the specified stream. Writing starts with the first
        /// data item and terminates on the same line.
        /// </summary>
        /// <param name="w">The streamwriter to write data to.</param>
        public void WriteStintData(ref System.IO.StreamWriter w)
        {
            w.Write(this.startLap);
            w.Write(',');
            w.Write(this.stintLength);
            w.Write(',');
            w.Write(this.tyreType);
        }

        public float TotalTime()
        {
            float totalTime = 0;
            foreach (float l in lapTimes)
            {
                totalTime += l;
            }

            return totalTime;
        }

        /// <summary>
        /// Gets the fastest lap in a stint without adjusting for fuel usage
        /// </summary>
        /// <returns></returns>
        public float FastestLap()
        {
            return FastestLap(0, 0, 0, 0);
        }

        /// <summary>
        /// Gets the adjusted fastest lap in a stint based only on fuel consumption
        /// </summary>
        /// <returns></returns>
        public float FastestLap(float fuelEffectPerLap)
        {
            return FastestLap(fuelEffectPerLap, 0, 0, Data.Settings.TrackImprovement);
        }

        public float FastestLap(float fuelEffectPerLap, float primeTyreWear, float optionTyreWear)
        {
            return FastestLap(fuelEffectPerLap, primeTyreWear, optionTyreWear, Data.Settings.TrackImprovement);
        }

        /// <summary>
        /// Gets the adjusted fastest lap in a stint, based on tyre wear and fuel consumption.
        /// </summary>
        /// <returns></returns>
        public float FastestLap(float fuelEffectPerLap, float primeTyreWear, float optionTyreWear, float trackImprovementPerLap)
        {
            float fastestLap = Data.Settings.DefaultPace;
            float fuelEffect = fuelEffectPerLap * (lapTimes.Count - 1);
            float tyreWearPerLap = (tyreType == TyreType.Prime ? primeTyreWear : optionTyreWear);
            float tyreEffect = 0;
            float trackEffect = trackImprovementPerLap * (lapTimes.Count - 1);
            float adjustedTime;
            foreach (float l in lapTimes)
            {
                adjustedTime = l - fuelEffect - tyreEffect - trackImprovementPerLap;
                fuelEffect -= fuelEffectPerLap;
                tyreEffect += tyreWearPerLap;
                if (adjustedTime < fastestLap)
                    fastestLap = adjustedTime;
            }
            return fastestLap;
        }

        /// <summary>
        /// Gets the index of the fastest lap in a stint without adjusting for fuel usage
        /// </summary>
        /// <returns></returns>
        public int FastestLapIndex()
        {
            return FastestLapIndex(0, 0, 0, 0);
        }

        /// <summary>
        /// Gets the index of the adjusted fastest lap in a stint based only on fuel consumption
        /// </summary>
        /// <returns></returns>
        public int FastestLapIndex(float fuelEffectPerLap)
        {
            return FastestLapIndex(fuelEffectPerLap, 0, 0, Data.Settings.TrackImprovement);
        }

        public int FastestLapIndex(float fuelEffectPerLap, float primeTyreWear, float optionTyreWear)
        {
            return FastestLapIndex(fuelEffectPerLap, primeTyreWear, optionTyreWear, Data.Settings.TrackImprovement);
        }

        public int FastestLapIndex(float fuelEffectPerLap, float primeTyreWear, float optionTyreWear, float trackImprovementPerLap)
        {
            float fastestLap = Data.Settings.DefaultPace;
            float fuelEffect = fuelEffectPerLap * (lapTimes.Count - 1);
            float tyreWearPerLap = (tyreType == TyreType.Prime ? primeTyreWear : optionTyreWear);
            float trackEffect = trackImprovementPerLap * (lapTimes.Count - 1);
            float tyreEffect = 0;
            float adjustedTime;
            int lapIndex = 0;
            int fastestLapIndex = 0;
            foreach (float l in lapTimes)
            {
                adjustedTime = l - fuelEffect - tyreEffect - trackImprovementPerLap;
                fuelEffect -= fuelEffectPerLap;
                tyreEffect += tyreWearPerLap;
                trackEffect -= trackImprovementPerLap;
                if (adjustedTime < fastestLap)
                {
                    fastestLap = adjustedTime;
                    fastestLapIndex = lapIndex;
                }
                lapIndex++;
            }
            return fastestLapIndex;
        }

        public float AverageLap()
        {
            float sum = 0;
            int totalled = 0;
            int count = 0;

            foreach (float l in lapTimes)
            {
                if (count++ != 0)
                {
                    sum += l;
                    ++totalled;
                }
            }

            return (sum / totalled);
        }

        struct indexedLapInStint
        {
            public float lapTime;
            public int lapIndex;
        };

        /// <returns>The average time reduction per lap due to any factor except track evolution for the stints</returns>
        public float AverageDegradation()
        {
            indexedLapInStint previousLap, currentLap, lap;
            float fastestLap;
            float totalDegradation = 0;
            float degradation = 0;
            int lapsAssessed = 0;
            int lapToAnalyse = 0;

            if (lapTimes.Count >= 2)
            {
                fastestLap = this.FastestLap();

                currentLap.lapTime = Data.Settings.DefaultPace;
                currentLap.lapIndex = 0;

                do
                {
                    lap.lapTime = lapTimes[lapToAnalyse];
                    lap.lapIndex = lapToAnalyse++;

                    if (lap.lapTime < fastestLap * 1.02)
                    {
                        previousLap = currentLap;
                        currentLap = lap;

                        ++lapsAssessed;

                        if (lapsAssessed >= 2)
                        {
                            totalDegradation += (currentLap.lapTime - previousLap.lapTime - Data.Settings.TrackImprovement);
                        }

                    }
                } while (lapToAnalyse < lapTimes.Count);

                degradation = (totalDegradation / lapsAssessed);

                if (Math.Abs(degradation) > fastestLap * 0.005) { degradation = 0; }
            }
            else
            {
                degradation = 0;
            }

            return degradation;
        }

        //methods for adding, removing, and re-ordering stints

        /// <summary>
        /// Splits a stint at a given lap in the race
        /// </summary>
        /// <param name="splitLap">The race lap to split the stint at</param>
        /// <returns>An array of the two generated stints, each with the same tyre type</returns>
        public Stint[] Split(int splitLap)
        {
            Stint stintToSplit = this;
            Stint[] splitStints = new Stint[2];
            int firstStintLength, secondStintLength;

            firstStintLength = splitLap - stintToSplit.startLap;
            secondStintLength = stintToSplit.startLap + stintToSplit.stintLength - splitLap;

            splitStints[0] = new Stint(stintToSplit.startLap, stintToSplit.tyreType, firstStintLength);
            splitStints[1] = new Stint(splitLap, stintToSplit.tyreType, secondStintLength);

            splitStints[0].modified = true;
            splitStints[1].modified = true;

            return splitStints;
        }
        /// <summary>
        /// Concatenates two stints into one longer stint, with the tyre type of the first stint
        /// </summary>
        /// <returns>The completed stint</returns>
        public static Stint Merge(Stint a, Stint b) //returns a stint of the correct length with no lap times included
        {
            Stint mergedStint = a;
            int startLap = a.startLap;
            int endLap = startLap + a.stintLength + b.stintLength;

            mergedStint.lapTimes.Clear();
            mergedStint.stintLength = endLap - startLap;

            mergedStint.modified = true;

            return mergedStint;
        }
        
        /// <summary>
        /// Swaps two stints orders and updates the start lap accordingly
        /// </summary>
        public static void Swap(ref Stint a, ref Stint b)
        {
            Stint tempStint;
            int stintAstartLap = a.startLap;

            tempStint = a;
            a = b;
            b = tempStint;

            a.startLap = stintAstartLap;
            b.startLap = stintAstartLap + a.stintLength;
        }
    }
}
