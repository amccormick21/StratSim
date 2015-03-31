using StratSim.ViewModel;

namespace StratSim.View.UserControls
{
    interface IDriverIndexControl
    {
        int DriverIndex { get; set; }
        event DriverIndexEvent ControlActivated;
    }
}
