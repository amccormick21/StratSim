using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MyFlowLayout
{
    /// <summary>
    /// Provides functionality for controlling the contents of the window flow panel.
    /// </summary>
    public class MyToolbar : ToolStripContainer, IDockableControl
    {
        ToolStrip menuBar, windowsBar;

        ToolStripDropDownButton ShowPanels;
        List<MyToolStripButton> PanelList = new List<MyToolStripButton>();

        FillStyles windowFillStyle;
        AutosizeTypes autoSize;
        DockTypes windowDockType;
        Size originalSize;
        Point dockPointLocation;

        Type type;

        WindowFlowPanel parent;
        MainForm form;

        bool removed;

        public MyToolbar(WindowFlowPanel parent, PanelControlEvents events)
        {
            SetupToolbar();
            windowFillStyle = FillStyles.FullWidth;
            autoSize = AutosizeTypes.Constant;
            windowDockType = DockTypes.TopLeft;

            this.parent = parent;
            removed = true;

            type = typeof(MyToolbar);

            originalSize = this.Size;

            events.ShowToolStrip += OnToolStripAdded;
            events.RemoveToolStrip += OnToolStripRemoved;
        }

        void LinkToForm(MainForm form)
        {
            form.IOController.FlowLayoutEvents.MainPanelLayoutChanged += GetShownHiddenPanels;
        }
        void UnLinkFromForm(MainForm form)
        {
            form.IOController.FlowLayoutEvents.MainPanelLayoutChanged -= GetShownHiddenPanels;
        }

        void OnToolStripAdded(ToolStripDropDownItem cm)
        {
            AddToolStripToToolbar(cm);
        }
        void OnToolStripRemoved(ToolStripDropDownItem cm)
        {
            RemoveToolStripFromToolbar(cm);
        }

        void SetupToolbar()
        {
            PrepareContainerForToolBar();
            AddMenuStripToContainer();
            AddButtonsToToolBar();
        }
        void PrepareContainerForToolBar()
        {
            this.Dock = DockStyle.Top;
            this.LeftToolStripPanelVisible = false;
            this.BottomToolStripPanelVisible = false;
            this.RightToolStripPanelVisible = false;
            this.Size = new Size(this.Width, 25);
        }
        void AddMenuStripToContainer()
        {
            menuBar = new ToolStrip();
            menuBar.BackColor = Color.White;
            windowsBar = new ToolStrip();
            windowsBar.BackColor = Color.White;
            this.TopToolStripPanel.Controls.Add(windowsBar);
            this.TopToolStripPanel.Controls.Add(menuBar);
            this.TopToolStripPanel.BackColor = Color.White;
        }

        public virtual void AddButtonsToToolBar()
        {
            //Show panels drop down
            ShowPanels = new ToolStripDropDownButton();
            menuBar.Items.Add(ShowPanels);
            ShowPanels.Text = "View";
            //Finish panels drop down
        }

        void AddToolStripToToolbar(ToolStripDropDownItem toolstripItemToAdd)
        {
            windowsBar.Items.Add(toolstripItemToAdd);
        }
        void RemoveToolStripFromToolbar(ToolStripDropDownItem toolstripItemToRemove)
        {
            windowsBar.Items.Remove(toolstripItemToRemove);
        }

        /// <summary>
        /// Adds the functionality to control a panel from the toolbar
        /// </summary>
        /// <param name="panelIndex">The index of the panel to add with respect to the form</param>
        public void AddPanel(int panelIndex)
        {
            MyToolStripButton PanelViewButton = new MyToolStripButton(panelIndex);
            PanelViewButton.ButtonClicked += ShowHidePanel;
            PanelViewButton.Text = parent.VisiblePanels[panelIndex].Name;
            PanelViewButton.Checked = parent.VisiblePanels[panelIndex].MyVisible;
            PanelViewButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            PanelViewButton.Image = parent.VisiblePanels[panelIndex].PanelIcon;
            PanelList.Add(PanelViewButton);
            ShowPanels.DropDownItems.Add(PanelViewButton);
        }
        /// <summary>
        /// Removes the show panel button for a panel from the toolbar
        /// </summary>
        /// <param name="panelIndex">The index of the panel to remove</param>
        public void RemovePanel(int panelIndex)
        {
            ShowPanels.DropDownItems.Remove(PanelList[panelIndex]);
            PanelList.RemoveAt(panelIndex);
            foreach (MyToolStripButton t in PanelList)
            {
                if (t.ButtonIndex > panelIndex)
                {
                    t.ButtonIndex--;
                }
            }

        }

        void ShowHidePanel(int buttonIndex, Type buttonType)
        {
            parent.VisiblePanels[buttonIndex].MyVisible = PanelList[buttonIndex].Checked;
        }
        void GetShownHiddenPanels()
        {
            int panelIndex = 0;
            foreach (MyToolStripButton t in ShowPanels.DropDownItems)
            {
                t.Checked = parent.VisiblePanels[panelIndex++].MyVisible;
            }
        }

        public void SetForm(MainForm Form)
        {
            if (this.Form != null)
            { UnLinkFromForm(this.Form); }

            this.Form = Form;
            LinkToForm(form);
        }

        public ToolStrip MenuBar
        { get { return menuBar; } }

        public MainForm Form
        {
            get { return form; }
            set { form = value; }
        }

        public FillStyles FillStyle
        {
            get { return windowFillStyle; }
            set { windowFillStyle = value; }
        }
        public AutosizeTypes AutoSizeType
        {
            get { return autoSize; }
            set { autoSize = value; }
        }
        public DockTypes DockType
        {
            get { return windowDockType; }
            set { windowDockType = value; }
        }
        public Type Type
        { get { return type; } }
        public Size OriginalSize
        {
            get { return originalSize; }
            set { originalSize = value; }
        }
        public Point DockPointLocation
        {
            get { return dockPointLocation; }
            set { dockPointLocation = value; }
        }
        public bool MyVisible
        {
            get { return Visible; }
            set { Visible = value; }
        }
        public bool Removed
        {
            get { return removed; }
            set { removed = value; }
        }
    }
}
