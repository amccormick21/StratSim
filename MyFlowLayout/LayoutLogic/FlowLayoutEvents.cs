using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MyFlowLayout
{
    /// <summary>
    /// Class controlling events when the window requires layout
    /// </summary>
    public class FlowLayoutEvents
    {
        public FlowLayoutEvents()
        { }

        public void OnMainPanelLayoutChanged()
        {
            if (MainPanelResizeStart != null)
                MainPanelResizeStart();
            if (MainPanelLayoutChanged != null)
                MainPanelLayoutChanged();
        }
        public void OnMainPanelResize()
        {
            if (MainPanelResizeEnd != null)
                MainPanelResizeEnd();
        }

        public delegate void PanelLayoutEventHandler();
        public event PanelLayoutEventHandler MainPanelLayoutChanged;
        public event PanelLayoutEventHandler MainPanelResizeStart;
        public event PanelLayoutEventHandler MainPanelResizeEnd;

        public void OnPanelDrag(Point newPoint)
        {
            PanelDrag(newPoint);
        }

        public delegate void PanelDragEventhandler(Point newPoint);
        public event PanelDragEventhandler PanelDrag;

    }
}
