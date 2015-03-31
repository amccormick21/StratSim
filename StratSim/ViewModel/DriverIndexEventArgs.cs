using System;

namespace StratSim.ViewModel
{
    public delegate void DriverIndexEvent(DriverIndexEventArgs e);

    public class DriverIndexEventArgs : EventArgs
    {
        public int DriverIndex { get; private set; }

        public DriverIndexEventArgs(int DriverIndex)
        {
            this.DriverIndex = DriverIndex;
        }
    }
}
