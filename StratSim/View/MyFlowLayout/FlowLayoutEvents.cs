using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace StratSim.View.MyFlowLayout
{
    /// <summary>
    /// Class controlling events when the window requires layout
    /// </summary>
    class FlowLayoutEvents
    {
        public FlowLayoutEvents()
        { }

        public static void OnMainPanelLayoutChanged()
        {
            if (MainPanelResizeStart != null)
                MainPanelResizeStart();
            MainPanelLayoutChanged();
        }
        public static void OnMainPanelResize()
        {
            if (MainPanelResizeEnd != null)
                MainPanelResizeEnd();
        }

        public delegate void PanelLayoutEventHandler();
        public static event PanelLayoutEventHandler MainPanelLayoutChanged;
        public static event PanelLayoutEventHandler MainPanelResizeStart;
        public static event PanelLayoutEventHandler MainPanelResizeEnd;

        public static void OnPanelDrag(Point newPoint)
        {
            PanelDrag(newPoint);
        }

        public delegate void PanelDragEventhandler(Point newPoint);
        public static event PanelDragEventhandler PanelDrag;

    }
}
