using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StratSim.Model
{
    public enum PaceParameterType
    {
        TopSpeed = 0,
        TyreDelta,
        PrimeDegradation,
        OptionDegradation,
        Pace,
        FuelEffect,
        FuelConsumption,
        FuelLoadP2
    }

    public class PaceParameterCollection
    {
        Dictionary<PaceParameterType, float> paceParameters;
        float pitStopLoss;

        public PaceParameterCollection(float[] PaceParameters, float PitStopLoss)
        {
            paceParameters = new Dictionary<PaceParameterType, float>();
            for (int parameter = 0; parameter < 8; ++parameter)
            {
                this.PaceParameters[(PaceParameterType)parameter] = PaceParameters[parameter];
            }
            this.PitStopLoss = PitStopLoss;
        }
        public PaceParameterCollection(Dictionary<PaceParameterType, float> PaceParameters, float PitStopLoss)
        {
            paceParameters = new Dictionary<PaceParameterType, float>();
            this.PaceParameters = PaceParameters;
            this.PitStopLoss = PitStopLoss;
        }
        public PaceParameterCollection(PracticeTimesCollection Collection)
        {
            paceParameters = new Dictionary<PaceParameterType, float>();
            SetPaceParameters(Collection, Data.RaceIndex);
        }

        public Dictionary<PaceParameterType, float> PaceParameters
        {
            get { return paceParameters; }
            set { paceParameters = value; }
        }

        public float PitStopLoss
        {
            get { return pitStopLoss; }
            set { pitStopLoss = value; }
        }

        public void SetPaceParameters(PracticeTimesCollection collection, int raceIndex)
        {
            PaceParameters[PaceParameterType.FuelLoadP2] = Data.Settings.DefaultP2Fuel;
            PaceParameters[PaceParameterType.TopSpeed] = collection.TopSpeed;
            PitStopLoss = Data.Tracks[raceIndex].pitStopLoss;
            PaceParameters[PaceParameterType.FuelConsumption] = Data.Tracks[raceIndex].fuelPerLap;
            PaceParameters[PaceParameterType.TyreDelta] = GetTyreDelta(collection.PracticeSessionStints);
            PaceParameters[PaceParameterType.FuelEffect] = GetFuelEffect(collection.PracticeSessionStints);
            float primeDegradation = GetPrimeDegradation(collection.PracticeSessionStints);
            float optionDegradation = GetOptionDegradation(collection.PracticeSessionStints, primeDegradation);
            SetTyreDegradation(primeDegradation, optionDegradation);
            PaceParameters[PaceParameterType.Pace] = GetLowFuelPace(collection.PracticeSessionStints);
        }

        private void SetTyreDegradation(float primeDegradation, float optionDegradation)
        {
            if (primeDegradation > optionDegradation)
            {
                //Set defaults
                PaceParameters[PaceParameterType.PrimeDegradation] = Data.Settings.DefaultPrimeDegradation;
                PaceParameters[PaceParameterType.OptionDegradation] = Data.Settings.DefaultOptionDegradation;
            }
            else
            {
                //Set to the values as normal
                PaceParameters[PaceParameterType.PrimeDegradation] = primeDegradation;
                PaceParameters[PaceParameterType.OptionDegradation] = optionDegradation;
            }
        }

        private float GetLowFuelPace(List<Stint>[] SessionStints)
        {
            float stintFastestLap;
            float fastestLap = Data.Settings.DefaultPace;

            //Finds the fastest lap across all sessions of the weekend
            for (int sessionDataIndex = 0; sessionDataIndex <= 3; sessionDataIndex++)
            {
                foreach (Stint s in SessionStints[sessionDataIndex])
                {
                    //Adjusts according to track evolution
                    stintFastestLap = s.FastestLap(PaceParameters[PaceParameterType.FuelConsumption] * PaceParameters[PaceParameterType.FuelEffect], PaceParameters[PaceParameterType.PrimeDegradation], PaceParameters[PaceParameterType.OptionDegradation]) - Data.Settings.TrackEvolution[sessionDataIndex];
                    if (stintFastestLap < fastestLap)
                    {
                        fastestLap = stintFastestLap;
                    }
                }
            }

            return fastestLap;
        }

        /*struct fastestLaps
        {
            public int fastestLapIndex;
            public int secondFastestLapIndex;
            public float fastestLap;
            public float secondFastestLap;
            public float effectOfFuel;
            public float degradation;
        };*/

        private float[,] GetPointArrayFromStints(List<Stint> stints)
        {
            int totalLaps = 0;
            foreach (Stint s in stints)
            {
                totalLaps += s.stintLength;
            }

            float[,] dataPoints = new float[totalLaps, 2];
            float[,] stintPoints;
            int stintLapIndex;
            int sessionLapIndex = 0;
            float stintFastestLap;
            float stintGradient, stintIntercept;
            foreach (Stint s in stints)
            {
                stintLapIndex = 0;
                stintFastestLap = s.FastestLap();

                //First, collect all of the points from the stint
                stintPoints = new float[s.stintLength, 2];
                foreach (float l in s.lapTimes)
                {
                    stintPoints[stintLapIndex, 0] = stintLapIndex;
                    stintPoints[stintLapIndex++, 1] = l;
                }
                //Plot a line through the stint to determine how it regresses.
                DataSources.Functions.LinearLeastSquaresFit(stintPoints, out stintGradient, out stintIntercept);

                //Normalise the data on the intercept and add to the main array
                stintLapIndex = 0;
                foreach (float l in s.lapTimes)
                {
                    dataPoints[sessionLapIndex, 0] = stintLapIndex++;
                    dataPoints[sessionLapIndex++, 1] = l - stintIntercept;
                }
            }
            return dataPoints;
        }

        private float GetAverageDegratationFromStints(List<Stint> stints)
        {
            float[,] dataPoints = GetPointArrayFromStints(stints);
            float gradient, intercept;
            float error = DataSources.Functions.LinearLeastSquaresFit(dataPoints, out gradient, out intercept);
            //Degradation per lap is the gradient of this line
            //Can validate here to have an error within certain bounds.
            return gradient;
        }

        private float GetOptionDegradation(List<Stint>[] SessionStints, float primeDegradation)
        {
            /* Finding the option degradation:
             * FP2, FP3, and qualifying will have stints with the option tyre
             * Pick the fastest stint with more than two valid laps to consider
             * Add all valid stints to an overall list, which is then analysed
             */

            List<Stint> validStints = new List<Stint>();
            float fastestLapInSession;
            int fastestStintIndexInSession;
            float stintFastestLap;
            int lapsCloseToFastest;

            for (int sessionIndex = 1; sessionIndex < 4; sessionIndex++)
            {
                fastestStintIndexInSession = -1;
                fastestLapInSession = 0;
                for (int stintIndex = 0; stintIndex < SessionStints[sessionIndex].Count; stintIndex++)
                {
                    //If the stint has more than two valid laps, we can consider it
                    stintFastestLap = SessionStints[sessionIndex][stintIndex].FastestLap();
                    lapsCloseToFastest = 0;
                    foreach (var l in SessionStints[sessionIndex][stintIndex].lapTimes)
                    {
                        if (l / stintFastestLap < 1.02)
                            lapsCloseToFastest++;
                    }
                    if (lapsCloseToFastest >= 2)
                    {
                        //Record the fastest lap and the index in a most wanted holder
                        if (stintFastestLap < fastestLapInSession || fastestStintIndexInSession == -1)
                        {
                            fastestLapInSession = stintFastestLap;
                            fastestStintIndexInSession = stintIndex;
                        }
                    }
                }
                //Add the best stint to the list
                if (fastestStintIndexInSession != -1)
                {
                    validStints.Add(SessionStints[sessionIndex][fastestStintIndexInSession]);
                }
            }

            //Now analyse the list
            float degradation = GetAverageDegratationFromStints(validStints) + (PaceParameters[PaceParameterType.FuelConsumption] * PaceParameters[PaceParameterType.FuelEffect]);
            if (degradation < primeDegradation)
            {
                degradation = Data.Settings.DefaultOptionDegradation;
            }
            return degradation;
        }

        private float GetPrimeDegradation(List<Stint>[] SessionStints)
        {
            /* Finding the prime degradation:
             * FP1, 2, 3 all have stints done on the prime tyre
             * Pick the longest stint and use this to analyse the tyre degradation
             */

            List<Stint> validStints = new List<Stint>();
            int longestStintInSession;
            int longeststintIndexInSession = 0;
            float fastestLapFound = 0;

            for (int sessionIndex = 0; sessionIndex < 3; sessionIndex++)
            {
                longeststintIndexInSession = -1;
                longestStintInSession = 0;
                for (int stintIndex = 0; stintIndex < SessionStints[sessionIndex].Count; stintIndex++)
                {
                    //Check the length
                    if (SessionStints[sessionIndex][stintIndex].stintLength > longestStintInSession || longeststintIndexInSession == -1)
                    {
                        longestStintInSession = SessionStints[sessionIndex][stintIndex].stintLength;
                        longeststintIndexInSession = stintIndex;
                    }
                }
                //Add the best stint to the list
                if (longeststintIndexInSession != -1)
                {
                    validStints.Add(SessionStints[sessionIndex][longeststintIndexInSession]);
                    if (sessionIndex == 0 || SessionStints[sessionIndex][longeststintIndexInSession].FastestLap() < fastestLapFound)
                    {
                        fastestLapFound = SessionStints[sessionIndex][longeststintIndexInSession].FastestLap();
                    }
                }
            }

            //Now analyse the list
            float degradation = GetAverageDegratationFromStints(validStints) + (PaceParameters[PaceParameterType.FuelConsumption] * PaceParameters[PaceParameterType.FuelEffect]);

            //Checks within reasonable bounds.
            if (degradation < 0 || degradation > fastestLapFound * 0.005)
            {
                degradation = Data.Settings.DefaultPrimeDegradation;
            }
            return degradation;
        }

        /*
        private float GetOptionDegradation(List<Stint>[] SessionStints)
        {
            /*finding the option tyre degradation:
             * for FP2 and FP3 in stint containing the fastest lap
             * check difference between fastest and second fastest lap
             * raw delta = later lap - earlier lap
             * correct for fuel effects
             * average between sessions
             *

            /*validation:
             * ignore if lap is greater than 1% of fastest lap
             * set results to default if:
             *      result is negative
             *      result is less than prime tyre degradation
             *

            fastestLaps[] sessionFastestLaps = new fastestLaps[2];
            int[] stintContainingFastestLap = new int[2];
            float stintFastestLap = Data.Settings.DefaultPace;
            int stintIndex;
            int lapNo;
            float optionDegradation = 0;
            int sessionsRead = 0;
            bool canReadFP2 = true;
            bool canReadFP3 = true;

            if (SessionStints[1].Count == 0) { canReadFP2 = false; }
            if (SessionStints[2].Count == 0) { canReadFP3 = false; }

            for (int i = 0; i <= 1; i++)
            {
                sessionFastestLaps[i].degradation = 0;
                sessionFastestLaps[i].effectOfFuel = 0;
                sessionFastestLaps[i].fastestLap = Data.Settings.DefaultPace;
                sessionFastestLaps[i].secondFastestLap = Data.Settings.DefaultPace;
                sessionFastestLaps[i].fastestLapIndex = 0;
                sessionFastestLaps[i].secondFastestLapIndex = 0;
            }


            stintContainingFastestLap[0] = 0; //FP2
            stintContainingFastestLap[1] = 1; //FP3

            if (canReadFP2)
            {
                try //get stint containing fastest lap from FP2
                {
                    stintIndex = 0;
                    foreach (Stint s in SessionStints[1])
                    {
                        stintFastestLap = s.FastestLap(PaceParameters[PaceParameterType.FuelEffect] * PaceParameters[PaceParameterType.FuelConsumption]);
                        if (stintFastestLap < sessionFastestLaps[0].fastestLap)
                        {
                            sessionFastestLaps[0].fastestLap = stintFastestLap;
                            stintContainingFastestLap[0] = stintIndex;
                        }
                        stintIndex++;
                    }
                }
                catch
                {
                    canReadFP2 = false;
                }
            }

            if (canReadFP2)
            {
                try //get degradation from FP2
                {
                    lapNo = 0;
                    foreach (float lap in SessionStints[1][stintContainingFastestLap[0]].lapTimes)
                    {
                        if ((lap < sessionFastestLaps[0].secondFastestLap) && (lap > sessionFastestLaps[0].fastestLap))
                        {
                            sessionFastestLaps[0].secondFastestLap = lap;
                            sessionFastestLaps[0].secondFastestLapIndex = lapNo;
                        }
                        if (lap == sessionFastestLaps[0].fastestLap)
                        {
                            sessionFastestLaps[0].fastestLapIndex = lapNo;
                        }
                        lapNo++;
                    }

                    //Adjust according to the effect of fuel
                    sessionFastestLaps[0].effectOfFuel = (PaceParameters[PaceParameterType.FuelEffect] * PaceParameters[PaceParameterType.FuelConsumption]) * Math.Abs((sessionFastestLaps[0].secondFastestLapIndex - sessionFastestLaps[0].fastestLapIndex));
                    sessionFastestLaps[0].degradation = (sessionFastestLaps[0].fastestLap - (sessionFastestLaps[0].secondFastestLap - sessionFastestLaps[0].effectOfFuel));
                }
                catch
                {
                    sessionFastestLaps[0].degradation = Data.Settings.DefaultOptionDegradation;
                }
            }
            else //set to default
            {
                sessionFastestLaps[0].degradation = Data.Settings.DefaultOptionDegradation;
            }

            if (canReadFP3)
            {
                try //get stint containing fastest lap from FP3
                {
                    stintIndex = 0;
                    foreach (Stint s in SessionStints[2])
                    {
                        stintFastestLap = s.FastestLap();
                        if (stintFastestLap < sessionFastestLaps[1].fastestLap)
                        {
                            sessionFastestLaps[1].fastestLap = stintFastestLap;
                            stintContainingFastestLap[1] = stintIndex;
                        }
                        stintIndex++;
                    }
                }
                catch
                {
                    canReadFP3 = false;
                }
            }

            if (canReadFP3)
            {
                try //get degradation from FP3
                {
                    lapNo = 0;
                    foreach (float lap in SessionStints[2][stintContainingFastestLap[1]].lapTimes)
                    {
                        if ((lap < sessionFastestLaps[1].secondFastestLap) && (lap > sessionFastestLaps[1].fastestLap))
                        {
                            sessionFastestLaps[1].secondFastestLap = lap;
                            sessionFastestLaps[1].secondFastestLapIndex = lapNo;
                        }
                        if (lap == sessionFastestLaps[1].fastestLap)
                        {
                            sessionFastestLaps[1].fastestLapIndex = lapNo;
                        }
                        lapNo++;
                    }

                    //adjust for fuel
                    sessionFastestLaps[1].effectOfFuel = (PaceParameters[PaceParameterType.FuelEffect] * PaceParameters[PaceParameterType.FuelConsumption]) * (sessionFastestLaps[1].secondFastestLapIndex - sessionFastestLaps[1].fastestLapIndex);
                    sessionFastestLaps[1].degradation = ((sessionFastestLaps[1].secondFastestLap - sessionFastestLaps[1].effectOfFuel) - sessionFastestLaps[1].fastestLap);
                }
                catch
                {
                    sessionFastestLaps[1].degradation = Data.Settings.DefaultOptionDegradation;
                }
            }
            else
            {
                sessionFastestLaps[1].degradation = Data.Settings.DefaultOptionDegradation;
            }

            //For each fastestLaps in the sessionFastestLaps array:
            for (int i = 0; i <= 1; i++)
            {
                if (sessionFastestLaps[i].degradation == 0) { sessionFastestLaps[i].degradation = Data.Settings.DefaultOptionDegradation; }
                if ((sessionFastestLaps[i].degradation < sessionFastestLaps[i].fastestLap * 0.01) && (sessionFastestLaps[i].degradation > PaceParameters[PaceParameterType.PrimeDegradation])) { optionDegradation += sessionFastestLaps[i].degradation; sessionsRead++; }
            }

            //Divides by the number of valid sessions found
            if (sessionsRead == 0) { optionDegradation = Data.Settings.DefaultOptionDegradation; }
            else { optionDegradation /= sessionsRead; }

            //Final validation to be greater than the prime degradation
            if (optionDegradation <= PaceParameters[PaceParameterType.PrimeDegradation]) { optionDegradation = Data.Settings.DefaultOptionDegradation; }

            return optionDegradation;
        }

        private float GetPrimeDegradation(List<Stint>[] SessionStints)
        {
            /*finding prime tyre degradation
             * Find the longest stint in FP1
             * find the average degradation throughout this stint
             * correct for fuel effects.
             * return default if negative
             *

            int stintLength = 0;
            int stintIndex = 0;
            int longestStintLength = 0;
            int longestStintIndex = 0;
            bool canReadFP1 = true;
            bool validate = true;

            float rawPrimeDegradation = 0;
            float primeDegradation = 0;

            //Checks that sessions are present in FP1
            if (SessionStints[0].Count == 0) { canReadFP1 = false; }

            if (canReadFP1)
            {
                //Finds the longest stint in FP1
                foreach (Stint s in SessionStints[0])
                {
                    stintLength = s.lapTimes.Count;
                    if (stintLength > longestStintLength)
                    {
                        longestStintLength = stintLength;
                        longestStintIndex = stintIndex;
                    }

                    stintIndex++;
                }
            }
            else
            {
                primeDegradation = Data.Settings.DefaultPrimeDegradation;
                validate = false;
            }

            //Avoids a null reference exception
            if (longestStintIndex < SessionStints[0].Count && longestStintIndex >= 0)
            {
                //Sets the raw value for prime tyre degradation by calculating degradation from the longest stint.
                rawPrimeDegradation = SessionStints[0][longestStintIndex].AverageDegradation();
            }
            else
            {
                primeDegradation = Data.Settings.DefaultPrimeDegradation;
                rawPrimeDegradation = 0;
                validate = false;
            }

            //Adjusts degradation for the calculated fuel effect.
            rawPrimeDegradation -= (PaceParameters[PaceParameterType.FuelEffect] * PaceParameters[PaceParameterType.FuelConsumption]);

            if (validate) //If the value has been changed
            {
                //Checks within reasonable bounds.
                if (rawPrimeDegradation > 0 && rawPrimeDegradation < SessionStints[0][longestStintIndex].FastestLap() * 0.003)
                {
                    primeDegradation = rawPrimeDegradation;
                }
                else
                {
                    primeDegradation = Data.Settings.DefaultPrimeDegradation;
                }
            }

            return primeDegradation;
        }*/

        private float GetFuelEffect(List<Stint>[] SessionStints)
        {
            float[] sessionFastestLap = new float[2];
            float stintFastestLap = Data.Settings.DefaultPace;
            float fuelEffect;

            sessionFastestLap[0] = Data.Settings.DefaultPace; //FP2
            sessionFastestLap[1] = Data.Settings.DefaultPace; //FP3 and qualifying

            try
            {
                //Gets the fastest lap from FP2
                foreach (Stint s in SessionStints[1])
                {
                    stintFastestLap = s.FastestLap();
                    if (stintFastestLap < sessionFastestLap[0])
                        sessionFastestLap[0] = stintFastestLap;
                }

                //Gets the fastest lap from FP3 or qualifying (low fuel runs)
                stintFastestLap = Data.Settings.DefaultPace;
                foreach (List<Stint> l in new List<Stint>[] { SessionStints[2], SessionStints[3] })
                {
                    foreach (Stint s in l)
                    {
                        stintFastestLap = s.FastestLap();
                        if (stintFastestLap < sessionFastestLap[1])
                            sessionFastestLap[1] = stintFastestLap;
                    }
                }

                //Finds the difference between these two times, taking into account the track evolution
                fuelEffect = ((sessionFastestLap[0] - Data.Settings.TrackEvolution[1]) - (sessionFastestLap[1] - Data.Settings.TrackEvolution[2])) / PaceParameters[PaceParameterType.FuelLoadP2];
            }
            catch
            {
                //return default
                return Data.Settings.DefaultFuelEffect;
            }

            //Checks within sensible bounds.
            if ((fuelEffect > (sessionFastestLap[1] * 0.005)) || (fuelEffect <= 0))
            {
                fuelEffect = Data.Settings.DefaultFuelEffect;
            }

            return fuelEffect;
        }

        private float GetTyreDelta(List<Stint>[] SessionStints)
        {
            //Calculate difference between fastest lap excluding fastest stint, and fastest lap with fastest stint
            //This can be done in FP2 and FP3

            float fastestLapFound = 0;
            float totalDelta = 0;
            int deltasFound = 0;
            float fastestStintLap = 0;
            float secondFastestStintLap = 0;
            int fastestStintIndex;
            bool fastLapFound = false;
            bool secondLapFound = false;
            for (int sessionIndex = 1; sessionIndex < 3; sessionIndex++)
            {
                fastestStintIndex = -1;
                fastestStintLap = 0;
                //Find the fastest stint
                for (int stintIndex = 0; stintIndex < SessionStints[sessionIndex].Count; stintIndex++)
                {
                    if (SessionStints[sessionIndex][stintIndex].FastestLap() < fastestStintLap || stintIndex == 0)
                    {
                        fastestStintLap = SessionStints[sessionIndex][stintIndex].FastestLap();
                        fastestStintIndex = stintIndex;
                        fastLapFound = true;
                        if (stintIndex == 0 || fastestStintLap < fastestLapFound)
                        {
                            fastestLapFound = fastestStintLap;
                        }
                    }
                }
                //Now find the second fastest stint
                for (int stintIndex = 0; stintIndex < SessionStints[sessionIndex].Count; stintIndex++)
                {
                    if (stintIndex != fastestStintIndex)
                    {
                        if (SessionStints[sessionIndex][stintIndex].FastestLap() < secondFastestStintLap || stintIndex == 0)
                        {
                            secondFastestStintLap = SessionStints[sessionIndex][stintIndex].FastestLap();
                            secondLapFound = true;
                        }
                    }
                }

                if (fastLapFound && secondLapFound)
                {
                    totalDelta += fastestStintLap - secondFastestStintLap;
                    deltasFound++;
                }
            }

            float tyreDelta;
            if (deltasFound > 0)
            {
                tyreDelta = totalDelta / deltasFound;
                //check within sensible limits.
                if ((tyreDelta < -(fastestStintLap * 0.02)) || (tyreDelta >= 0))
                {
                    tyreDelta = Data.Settings.DefaultCompoundDelta;
                }
            }
            else
            {
                tyreDelta = Data.Settings.DefaultCompoundDelta;
            }
            return tyreDelta;
        }

        /*
        private float GetTyreDelta(List<Stint>[] SessionStints)
        {
            float[] sessionFastestLap = new float[2];
            float stintFastestLap = Data.Settings.DefaultPace;
            float tyreDelta;

            sessionFastestLap[0] = Data.Settings.DefaultPace;
            sessionFastestLap[1] = Data.Settings.DefaultPace;

            try
            {
                //Gets the fastest lap in FP1
                foreach (Stint s in SessionStints[0])
                {
                    stintFastestLap = s.FastestLap();
                    if (stintFastestLap < sessionFastestLap[0])
                        sessionFastestLap[0] = stintFastestLap;
                }

                //Gets the fastest lap in FP2
                foreach (Stint s in SessionStints[1])
                {
                    stintFastestLap = s.FastestLap();
                    if (stintFastestLap < sessionFastestLap[1])
                        sessionFastestLap[1] = stintFastestLap;
                }

                //Calculates the difference between these two times, taking into account the track evolution.
                tyreDelta = (sessionFastestLap[1] - Data.Settings.TrackEvolution[1]) - (sessionFastestLap[0] - Data.Settings.TrackEvolution[0]);
            }
            catch
            {
                //return default tyre delta
                tyreDelta = Data.Settings.DefaultCompoundDelta;
            }

            //check within sensible limits.
            if ((tyreDelta < -(sessionFastestLap[1] * 0.02)) || (tyreDelta >= 0))
            {
                tyreDelta = Data.Settings.DefaultCompoundDelta;
            }

            return tyreDelta;
        }*/
    }
}
