using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace StratSim.View.MyFlowLayout
{
    public interface IDockableControl
    {
        FillStyles FillStyle { get; set; }
        AutosizeTypes AutoSizeType { get; set; }
        DockTypes DockType { get; set; }
        Size OriginalSize { get; set; }
        Point DockPointLocation { get; set; }
        Type Type { get;}
        bool MyVisible { get; set; }
        bool Removed { get; set; }
    }
}
