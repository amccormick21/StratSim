using StratSim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StratSim.View.UserControls
{
    class TyreTypeChangedEventArgs
    {
        internal TyreType NewTyreType { get; set; }
        internal int StintIndex { get; set; }
        internal int DriverIndex { get; set; }

        public TyreTypeChangedEventArgs(int stintIndex, int driverIndex, TyreType newTyreType)
        {
            NewTyreType = newTyreType;
            DriverIndex = driverIndex;
            StintIndex = stintIndex;
        }
    }

    class StintOperationEventArgs
    {
        internal int StintOperationIndex { get; set; }
        internal int StintIndex { get; set; }
        internal int DriverIndex { get; set; }

        public StintOperationEventArgs(int stintIndex, int driverIndex, int stintOperationIndex)
        {
            StintOperationIndex = stintOperationIndex;
            DriverIndex = driverIndex;
            StintIndex = stintIndex;
        }
    }

    class StintLengthChangedEventArgs
    {
        internal int NewStintLength { get; set; }
        internal int StintIndex { get; set; }
        internal int DriverIndex { get; set; }

        public StintLengthChangedEventArgs(int stintIndex, int driverIndex, int newStintLength)
        {
            NewStintLength = newStintLength;
            DriverIndex = driverIndex;
            StintIndex = stintIndex;
        }
    }
}
