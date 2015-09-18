using System.Collections.Generic;
using System.IO;
using DataSources.DataConnections;
using System;

namespace StratSim.Model.Files
{
    /// <summary>
    /// Base class for types of timing data processed by the system
    /// </summary>
    public class TimingData
    {
        protected struct DriverTimingDataElement
        {
            public int driverIndex;
            public float value;
        };

        private string[] dataAsArray;

        public TimingData()
        { }

        public TimingData(string passedData)
        {
            FileData = ConvertToArray(passedData);
        }

        public virtual void UpdateDatabaseWithData(Session session, int raceIndex)
        {

        }

        /// <summary>
        /// Gets the full file name with extension for the given session, track, and data type
        /// </summary>
        /// <param name="session">The session to find the file for</param>
        /// <param name="raceIndex">The index of the race to find the file for</param>
        /// <returns>The complete file name with extension for locating the PDF file containing the required data.</returns>
        public static string GetFileName(Session session, int raceIndex)
        {
            string fileName = "";

            fileName += Data.Tracks[raceIndex].name;
            fileName += " ";
            fileName += session.GetSessionName();
            fileName += " ";
            fileName += session.GetTextFileName();
            fileName += ".txt";

            return fileName;
        }

        /// <summary>
        /// Converts a character value to an integer
        /// </summary>
        /// <param name="c">The character to be changed</param>
        /// <returns>An integer value that is the same as the textual representation held by the character</returns>
        public static int CovertCharToInt(char c)
        {
            return ((int)c - 48);
        }

        /// <summary>
        /// Gets a driver number from a line of text containing the driver number.
        /// </summary>
        /// <param name="line">The string containing the number</param>
        /// <param name="startPosition">The start position of the number in the line</param>
        /// <returns>The driver number as an integer from the line</returns>
        public static int GetNumber(string line, int startPosition)
        {
            int number = 0;

            try
            {
                if (line[startPosition + 1] != ' ')
                {
                    number += CovertCharToInt(line[startPosition]) * 10;
                    line = line.Remove(startPosition, 1); //takes out first character
                }

                number += (CovertCharToInt(line[startPosition]));

                return number;
            }
            catch
            {
                return -1;
            }

        }

        /// <summary>
        /// Converts data from a string including line spacing characters to an array of each line.
        /// </summary>
        /// <param name="data">The string of data to be converted to a string</param>
        /// <returns>An array of strings representing each line</returns>
        public static string[] ConvertToArray(string data)
        {
            List<string> temp = new List<string>();
            string[] arrayToReturn;
            string stringToAdd = "";
            int charsProcessed = 0;

            do
            {
                if (data[charsProcessed] == '\r')
                {
                    if (data[charsProcessed + 1] == '\n') //If a line break has been found
                    {
                        temp.Add(stringToAdd);
                        stringToAdd = "";
                        charsProcessed += 1;
                    }
                }
                else { stringToAdd += data[charsProcessed]; }
            } while (++charsProcessed <= data.Length - 1);

            //Adds the last string to the list
            if (stringToAdd != null && stringToAdd.Length > 0)
                temp.Add(stringToAdd);

            //sets up an array of the strings
            arrayToReturn = new string[temp.Count + 1];
            int stringIndex = 0;

            //populates the array of strings
            foreach (string s in temp)
            {
                arrayToReturn[stringIndex++] = s;
            }

            return arrayToReturn;
        }

        /// <summary>
        /// Checks if the line contains a driver's name and number, and if it is a valid combination
        /// </summary>
        /// <param name="testLine">The line to check for names</param>
        /// <returns>True if the line contains a driver name and matching number</returns>
        public static bool NameCheck(string testLine, out int driverNumber)
        {
            int driverIndex;
            int startOfName, nameLength;
            string driverName = "";

            try
            {
                //gets the number of the driver. Stored as string so converts from ascii
                driverNumber = GetNumber(testLine, 0);
                driverIndex = Driver.ConvertToDriverIndex(driverNumber);

                if (driverIndex < 0 || driverIndex >= Data.NumberOfDrivers) { return false; }
            }
            catch
            {
                driverNumber = 0;
                return false;
            }

            startOfName = (driverNumber >= 10 ? 6 : 5);
            nameLength = Data.Drivers[driverIndex].DriverName.Length;

            if (testLine.Length >= (startOfName + nameLength))
            {
                driverName = testLine.Substring(startOfName, nameLength);
            }

            try { if (driverName != Data.Drivers[driverIndex].DriverName) { return false; } }
            catch { return false; }

            return true;
        }

        public static bool NameCheck(string testLine)
        {
            int driverNumber;

            return NameCheck(testLine, out driverNumber);
        }

        public static bool IsDriverName(string name)
        {
            int driverIndex = 0;

            return IsDriverName(name, out driverIndex);
        }

        /// <summary>
        /// Checks if the specified value is a valid driver name
        /// </summary>
        /// <param name="name">The name to test</param>
        /// <returns>True if the name is valid</returns>
        public static bool IsDriverName(string name, out int driverIndex)
        {
            bool isName = false;
            int driverIndexIterator = -1;
            driverIndex = -1;

            foreach (Driver d in Data.Drivers)
            {
                ++driverIndexIterator;
                if (name == d.DriverName)
                {
                    isName = true;
                    driverIndex = driverIndexIterator;
                }
            }

            return isName;
        }

        /// <summary>
        /// Gets a single string line from a string array of data
        /// </summary>
        /// <param name="lineNumber">The array index to retrieve</param>
        /// <param name="fileData">The array to retrieve data from</param>
        /// <returns>Empty string if the array index is out of range</returns>
        public static string GetNextLineToInput(int lineNumber, string[] fileData)
        {
            string lineToReturn;

            try
            {
                lineToReturn = fileData[lineNumber];
                return lineToReturn;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>Gets a lap time from a specified string</summary>
        /// <param name="time">A string with the time in mm:ss:000 format</param>
        /// <returns>The number of seconds represented by the time</returns>
        public float GetLapTime(string time)
        {
            float lapTime = 0;
            int decimalPlaces = 3;

            if (time[1] != ':')
            {
                lapTime += CovertCharToInt(time[0]) * 600; //counts tens of minutes if they are present
                time = time.Remove(0, 1); //removes tens of minutes
                decimalPlaces = 2;
            }

            //add minutes, 10 x seconds, seconds
            lapTime += CovertCharToInt(time[0]) * 60;
            lapTime += CovertCharToInt(time[2]) * 10;
            lapTime += CovertCharToInt(time[3]);

            //add thousandths
            for (int characterIndex = 1; characterIndex <= decimalPlaces; characterIndex++)
            {
                try
                { lapTime += CovertCharToInt(time[4 + characterIndex]) * (float)Math.Pow(10, -characterIndex); }
                catch
                { }
            }

            return lapTime;
        }

        /// <returns>The file path of the directory containing the required file</returns>
        public static string GetTimingDataDirectory(int raceIndex, int currentYear)
        {
            string directoryName = Data.Tracks[raceIndex].name;

            string baseDirectory = Data.Settings.DataFilePath;
            string dataPath = baseDirectory + "/RaceData/" + currentYear.ToString() + "/";
            dataPath += directoryName;

            if (!Directory.Exists(dataPath)) { Directory.CreateDirectory(dataPath); }

            dataPath += "/";

            return dataPath;
        }

        public string[] FileData
        {
            get { return dataAsArray; }
            private set { dataAsArray = value; }
        }
    }
}
