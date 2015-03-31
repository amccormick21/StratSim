using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace StratSim.Model
{
    public class DriverData
    {
        int driverNumber;
        int driverIndex;
        string team;
        string driverName;
        string abbreviation;
        Color lineColour;

        /// <summary>
        /// Initialises a new instance of a driver data class, containing information about the driver.
        /// </summary>
        /// <param name="DriverIndex">The index of the driver in the drivers array</param>
        /// <param name="DriverName">The name of the driver</param>
        /// <param name="DriverTeam">The team that the driver belongs to</param>
        /// <param name="DriverNumber">The driver's race number</param>
        /// <param name="LineColour">The colour to display this driver's traces on graphs and keys</param>
        /// <param name="Abbreviation">The three letter code representing the driver on timing sheets</param>
        public DriverData(int DriverIndex, string DriverName, string DriverTeam, int DriverNumber, Color LineColour, string Abbreviation)
        {
            this.DriverIndex = DriverIndex;
            this.DriverName = DriverName;
            this.DriverNumber = DriverNumber;
            this.Team = DriverTeam;
            this.LineColour = LineColour;
            this.Abbreviation = Abbreviation;
        }

        /// <summary>
        /// Initialises an empty instance of a driver data class
        /// </summary>
        public DriverData() { }

        public override string ToString()
        {
            var nameString = this.DriverNumber.ToString();
            nameString += " - ";
            nameString += this.Abbreviation;
            return nameString;
        }

        /// <summary>
        /// Converts a driver number to the corresponding driver index
        /// </summary>
        /// <param name="driverNumber">The driver number to be found</param>
        /// <returns>The index of the driver who currently holds the driver number.
        /// Returns -1 if the driver number does not exist.</returns>
        public static int ConvertToDriverIndex(int driverNumber)
        {
            int driverIndex = 0;
            bool exitLoop = false;
            do
            {
                if (driverIndex >= Data.NumberOfDrivers) //driver number not found
                {
                    exitLoop = true;
                    driverIndex = -1;
                }
                else
                {
                    exitLoop = (Data.Drivers[driverIndex].DriverNumber == driverNumber);
                }
                ++driverIndex;
            }
            while (!exitLoop);

            return driverIndex - 1;
        }

        public static int ConvertToDriverIndex(string DriverName)
        {
            int driverIndex = 0;
            bool exitLoop = false;
            do
            {
                if (driverIndex >= Data.NumberOfDrivers) //driver number not found
                {
                    exitLoop = true;
                    driverIndex = -1;
                }
                else
                {
                    exitLoop = (Data.Drivers[driverIndex].DriverName == DriverName);
                }
                ++driverIndex;
            }
            while (!exitLoop);

            return driverIndex - 1;

        }

        public void SetDriverIndex(int DriverIndex)
        {
            this.DriverIndex = DriverIndex;
        }

        public int DriverNumber
        {
            get { return driverNumber; }
            private set { driverNumber = value; }
        }

        public int DriverIndex
        {
            get { return driverIndex; }
            private set { driverIndex = value; }
        }

        public string Team
        {
            get { return team; }
            private set { team = value; }
        }

        public string DriverName
        {
            get { return driverName; }
            private set { driverName = value; }
        }

        public Color LineColour
        {
            get { return lineColour; }
            private set { lineColour = value; }
        }

        public string Abbreviation
        {
            get { return abbreviation; }
            private set { abbreviation = value; }
        }
    }
}
