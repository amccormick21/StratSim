using StratSim.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace StratSim.View.MyFlowLayout
{
    public class ContentTabControl : MyPanel
    {
        TabControl MainControls;

        List<MyPanel> PanelsToDisplay = new List<MyPanel>();
        List<TabPage> TabPages = new List<TabPage>();

        ContextMenuStrip thisContextMenu;
        ToolStripDropDownButton closePanels;

        public ContentTabControl(MainForm FormToAdd)
            : base(400,300,"Main", FormToAdd, Properties.Resources.Main)
        {
            FlowLayoutEvents.MainPanelResizeEnd += ResizeMainControl;
                        
            MainControls = new TabControl();
            this.Controls.Add(MainControls);

            MainControls.ControlAdded += MainControls_ControlAdded;
            MainControls.ControlRemoved += MainControls_ControlRemoved;
            OpenedInNewForm += ContentTabControl_OpenedInNewForm;
            MyEvents.RemoveContentPanel += MyEvents_RemoveContentPanel;

            SetupContextMenu();

            SetPanelProperties(DockTypes.Top, AutosizeTypes.Free, FillStyles.None, this.Size);
        }

        void MyEvents_RemoveContentPanel(int PanelIndex)
        {
            closePanels.DropDownItems.RemoveAt(PanelIndex);
        }

        void ContentTabControl_OpenedInNewForm(MainForm NewForm)
        {
            foreach (MyPanel p in PanelsToDisplay)
            {
                p.Form = NewForm;
            }
            NewForm.IOController.SetContentTabControl(this);
        }

        void MainControls_ControlRemoved(object sender, ControlEventArgs e)
        {
            MainControls.SelectedIndex = MainControls.TabCount - 1;
        }

        void MainControls_ControlAdded(object sender, ControlEventArgs e)
        {
            MainControls.SelectedIndex = MainControls.TabCount - 1;
        }

        void SetupContextMenu()
        {
            closePanels = new ToolStripDropDownButton("Close");

            thisContextMenu = new ContextMenuStrip();
            thisContextMenu.Items.Add(closePanels);

            MyToolStripButton tempButton;
            int index = 0;
            foreach (MyPanel p in PanelsToDisplay)
            {
                tempButton = new MyToolStripButton(index++);
                tempButton.Text = p.Name;
                tempButton.ButtonClicked += tempButton_CustomCheckedChanged;
                closePanels.DropDownItems.Add(tempButton);
            }
            this.ContextMenuStrip = thisContextMenu;
            ContextMenuStrip.Width = 100;
        }

        void tempButton_CustomCheckedChanged(int buttonIndex, Type buttonType)
        {
            this.RemovePanelFromControl(PanelsToDisplay[buttonIndex]);
            thisContextMenu.Items.Remove(closePanels.DropDownItems[buttonIndex]);
            closePanels.DropDownItems.RemoveAt(buttonIndex);
            foreach (MyToolStripButton t in closePanels.DropDownItems)
            {
                if (t.ButtonIndex > buttonIndex)
                {
                    t.ButtonIndex--;
                    PanelsToDisplay[t.ButtonIndex].PanelIndex--;
                }
            }
        }

        void ResizeMainControl()
        {
            MainControls.Size = new Size(this.Width - MyPadding.Horizontal, this.Height - MyPadding.Vertical);
            MainControls.Location = new Point(MyPadding.Left, MyPadding.Top);

            foreach (TabPage p in TabPages)
            {
                p.Size = MainControls.ClientSize;
            }

            foreach (MyPanel p in PanelsToDisplay)
            {
                if (p.Parent == null) { p.Size = MainControls.ClientSize; }
                else { p.Size = p.Parent.ClientSize; }
                p.PositionComponents();
            }
        }

        void DisplayControls()
        {
            int visiblePanels = 0;
            TabPages.Clear();
            MainControls.TabPages.Clear();

            foreach (MyPanel p in PanelsToDisplay)
            {
                if (p.MyVisible == true) { ++visiblePanels; }
            }

            if (visiblePanels == 0)
            {
                this.Visible = false;
            }
            if (visiblePanels == 1)
            {
                MainControls.Visible = false;
                foreach (MyPanel p in PanelsToDisplay)
                {
                    if (p.MyVisible == true)
                    {
                        p.Location = new Point(MyPadding.Left, MyPadding.Top);
                        p.Size = new Size(this.Width - MyPadding.Horizontal, this.Height - MyPadding.Vertical);
                    }
                    this.Controls.Add((Control)p);
                }    
            }
            if (visiblePanels > 1)
            {
                MainControls.Visible = true;
                int panelsAdded = 0;
                foreach (MyPanel p in PanelsToDisplay)
                {
                    if (p.MyVisible == true)
                    {
                        TabPage tempTabPage = new TabPage(p.Name);
                        tempTabPage.Controls.Add(p);

                        p.Location = new Point(0, 0);
                        p.Size = new Size(MainControls.DisplayRectangle.Width, MainControls.DisplayRectangle.Height);

                        TabPages.Add(tempTabPage);

                        MainControls.TabPages.Add(TabPages[panelsAdded++]);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a panel to the tab control and re-draws the control.
        /// </summary>
        /// <param name="PanelToAdd">The panel to be added to the control.</param>
        public void AddPanelToTabControl(MyPanel PanelToAdd)
        {
            if (!PanelToAdd.InContentPanel)
            {
                PanelsToDisplay.Add(PanelToAdd);
                PanelToAdd.RemoveToolbarButtons();
                PanelToAdd.PanelIndex = PanelsToDisplay.Count - 1;
                PanelToAdd.InContentPanel = true;

                //Add tool strip button to close the panel.
                MyToolStripButton ButtonToAdd;
                ButtonToAdd = new MyToolStripButton(PanelsToDisplay.Count - 1);
                ButtonToAdd.Text = PanelToAdd.Name;
                ButtonToAdd.ButtonClicked += tempButton_CustomCheckedChanged;
                closePanels.DropDownItems.Add(ButtonToAdd);

                DisplayControls();
            }
        }

        /// <summary>
        /// Removes a panel from the content tab control, triggers the PanelClosed event and re-draws the control.
        /// </summary>
        /// <param name="PanelToRemove">The panel to be removed from the tab control</param>
        public void RemovePanelFromControl(MyPanel PanelToRemove)
        {
            if (PanelToRemove.InContentPanel)
            {
                PanelsToDisplay.Remove(PanelToRemove);
                PanelToRemove.InContentPanel = false;
                DisplayControls();
            }
        }
    }
}
