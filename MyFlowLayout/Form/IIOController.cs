using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFlowLayout
{
    interface IIOController
    {
        MainFormIOController GetNew();
        void AddPanel(IDockableControl ControlToAdd);
        void RemovePanel(IDockableControl ControlToRemove);
        void AddContentPanel(MyPanel ControlToAdd);
        void RemoveContentPanel(MyPanel ControlToRemove);
        MainForm AssociatedForm { get; set; }
        MyToolbar Toolbar { get; set; }
        WindowFlowPanel MainPanel { get; set; }
        ContentTabControl ContentTabControl { get; set; }
        PanelControlEvents PanelControlEvents { get; set; }
        FlowLayoutEvents FlowLayoutEvents { get; set; }
        string FormTitle { get; set; }
    }
}
