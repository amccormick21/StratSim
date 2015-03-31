using StratSim.Model;
using StratSim.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace StratSim.View.MyFlowLayout
{
    /// <summary>
    /// Provides functionality for controlling the contents of the window flow panel.
    /// </summary>
    public class MyToolbar : ToolStripContainer, IDockableControl
    {
        ToolStrip menuBar, windowsBar;
        ToolStripButton VersionInfo;

        ToolStripDropDownButton ShowPanels;
        List<MyToolStripButton> PanelList = new List<MyToolStripButton>();

        ToolStripDropDownButton QuickStart;
        ToolStripDropDownButton Parameters, Strategies;
        ToolStripButton Race, Archives;

        ToolStripDropDownButton OpenWindows;
        ToolStripButton Info, Settings, Main, ParameterViewer, StrategyViewer, ArchiveViewer, Graph, Axes, DriverViewer;

        FillStyles windowFillStyle;
        AutosizeTypes autoSize;
        DockTypes windowDockType;
        Size originalSize;
        Point dockPointLocation;

        Type type;

        WindowFlowPanel _parent;
        MainForm form;

        bool removed;

        public MyToolbar(WindowFlowPanel parent)
        {
            SetupToolbar();
            windowFillStyle = FillStyles.FullWidth;
            autoSize = AutosizeTypes.Constant;
            windowDockType = DockTypes.TopLeft;

            _parent = parent;
            form = _parent._parent;
            removed = true;

            type = typeof(MyToolbar);

            originalSize = this.Size;

            form.PanelAdded += OnPanelAdded;
            FlowLayoutEvents.MainPanelLayoutChanged += GetShownHiddenPanels;
            MyEvents.ShowToolStrip += OnToolStripAdded;
            MyEvents.RemoveToolStrip += OnToolStripRemoved;
            MyEvents.FinishedLoadingParameters += OnPaceParametersFinishedLoading;
            MyEvents.FinishedLoadingStrategies += OnStrategiesFinishedLoading;
        }

        void OnToolStripAdded(ToolStripDropDownItem cm)
        {
            AddToolStripToToolbar(cm);
        }
        void OnToolStripRemoved(ToolStripDropDownItem cm)
        {
            RemoveToolStripFromToolbar(cm);
        }

        void OnPanelAdded(int panelIndex)
        {
            AddPanel(panelIndex);
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

        void AddButtonsToToolBar()
        {
            //Show panels drop down
            ShowPanels = new ToolStripDropDownButton();
            menuBar.Items.Add(ShowPanels);
            ShowPanels.Text = "View";
            //Finish panels drop down

            //QuickStart
            QuickStart = new ToolStripDropDownButton();
            menuBar.Items.Add(QuickStart);
            QuickStart.Text = "Startup";

            Parameters = new ToolStripDropDownButton("Parameters", Properties.Resources.Pace_Parameters);
            QuickStart.DropDownItems.Add(Parameters);
            ToolStripDropDownButton FromRaceData = new ToolStripDropDownButton("From Race Data");
            Parameters.DropDownItems.Add(FromRaceData);
            FromRaceData.AutoSize = true;
            for (int trackIndex = 0; trackIndex < Data.NumberOfTracks; trackIndex++)
            {
                RaceDropDown button = new RaceDropDown(trackIndex);
                FromRaceData.DropDownItems.Add(button);
                button.ButtonClicked += RaceIndexSelected;
            }
            ToolStripButton FromFile = new ToolStripButton("From File");
            Parameters.DropDownItems.Add(FromFile);
            FromFile.Click += FromFile_Click;

            Strategies = new ToolStripDropDownButton("Strategies", Properties.Resources.Strategies);
            Strategies.Visible = false;
            QuickStart.DropDownItems.Add(Strategies);
            ToolStripButton StrategiesFromFile = new ToolStripButton("From File");
            Strategies.DropDownItems.Add(StrategiesFromFile);
            StrategiesFromFile.Click += StrategiesFromFile_Click;
            ToolStripButton StrategiesFromData = new ToolStripButton("From Pace Data");
            Strategies.DropDownItems.Add(StrategiesFromData);
            StrategiesFromData.Click += StrategiesFromData_Click;

            Race = new ToolStripButton("Race", Properties.Resources.Race);
            Race.Visible = false;
            Race.Click += Race_Click;
            QuickStart.DropDownItems.Add(Race);

            Archives = new ToolStripButton("Archives", Properties.Resources.Archives);
            Archives.Click += ArchivesReady;
            QuickStart.DropDownItems.Add(Archives);

            ToolStripButton DataInput = new ToolStripButton("Input Data", Properties.Resources.Input);
            DataInput.Click += DataInput_Click;
            QuickStart.DropDownItems.Add(DataInput);
            //Finish Quickstart

            //Open Windows
            ToolStripButton tempButton;

            OpenWindows = new ToolStripDropDownButton();
            OpenWindows.Text = "Open Windows";
            menuBar.Items.Add(OpenWindows);

            //Adds the buttons and event handlers for all panels that could be opened by the user
            tempButton = new ToolStripButton("Info", Properties.Resources.Info);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowInfoPanel(form);
            Info = tempButton;
            OpenWindows.DropDownItems.Add(Info);
            tempButton = new ToolStripButton("Settings", Properties.Resources.Settings);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowSettingsPanel(form);
            Settings = tempButton;
            OpenWindows.DropDownItems.Add(Settings);
            tempButton = new ToolStripButton("Main", Properties.Resources.Main);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowContentTabControl(form);
            Main = tempButton;
            OpenWindows.DropDownItems.Add(Main);
            tempButton = new ToolStripButton("Data Input", Properties.Resources.Input);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowDataInput(form);
            DataInput = tempButton;
            OpenWindows.DropDownItems.Add(DataInput);
            tempButton = new ToolStripButton("Parameter Viewer", Properties.Resources.Pace_Parameters);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowPaceParameters(form);
            ParameterViewer = tempButton;
            OpenWindows.DropDownItems.Add(ParameterViewer);
            tempButton = new ToolStripButton("Strategy Viewer", Properties.Resources.Strategies);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowStrategies(form);
            StrategyViewer = tempButton;
            OpenWindows.DropDownItems.Add(StrategyViewer);
            tempButton = new ToolStripButton("Archives", Properties.Resources.Archives);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowArchives(form);
            ArchiveViewer = tempButton;
            OpenWindows.DropDownItems.Add(ArchiveViewer);
            tempButton = new ToolStripButton("Graph", Properties.Resources.Graph);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowGraph(form);
            Graph = tempButton;
            OpenWindows.DropDownItems.Add(Graph);
            tempButton = new ToolStripButton("Axes", Properties.Resources.Axes);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowAxes(form);
            Axes = tempButton;
            OpenWindows.DropDownItems.Add(Axes);
            tempButton = new ToolStripButton("Drivers", Properties.Resources.Driver_Select);
            tempButton.AutoSize = true;
            tempButton.Click += (s, e) => PanelControlEvents.OnShowDriverSelectPanel(form);
            DriverViewer = tempButton;
            OpenWindows.DropDownItems.Add(DriverViewer);
            //Finish Windows

            //Version info
            VersionInfo = new ToolStripButton();
            VersionInfo.Text = "Version";
            VersionInfo.Click += VersionInfo_Click;
            menuBar.Items.Add(VersionInfo);
            //Finish version info
        }

        void OnPaceParametersFinishedLoading()
        {
            Strategies.Visible = true;
        }
        void OnStrategiesFinishedLoading()
        {
            Race.Visible = true;
        }

        void DataInput_Click(object sender, EventArgs e)
        {
            PanelControlEvents.OnShowDataInput(form);
        }
        void VersionInfo_Click(object sender, EventArgs e)
        {
            PanelControlEvents.OnShowVersionInfo();
        }

        void ArchivesReady(object sender, EventArgs e)
        {
            PanelControlEvents.OnShowArchives(form);
        }
        void FromFile_Click(object sender, EventArgs e)
        {
            PanelControlEvents.OnLoadPaceParametersFromFile();
            PanelControlEvents.OnShowPaceParameters(form);
        }
        void RaceIndexSelected(int buttonIndex, Type buttonType)
        {
            Data.RaceIndex = buttonIndex;
            PanelControlEvents.OnLoadPaceParametersFromRace();
            PanelControlEvents.OnShowPaceParameters(form);
        }
        void StrategiesFromFile_Click(object sender, EventArgs e)
        {
            PanelControlEvents.OnLoadStrategiesFromFile();
            PanelControlEvents.OnShowStrategies(form);
        }
        void StrategiesFromData_Click(object sender, EventArgs e)
        {
            PanelControlEvents.OnLoadStrategiesFromData();
            PanelControlEvents.OnShowStrategies(form);
        }

        void Race_Click(object sender, EventArgs e)
        {
            PanelControlEvents.OnStartRaceFromStrategies();
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
            PanelViewButton.Text = _parent.VisiblePanels[panelIndex].Name;
            PanelViewButton.Checked = _parent.VisiblePanels[panelIndex].MyVisible;
            PanelViewButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            PanelViewButton.Image = _parent.VisiblePanels[panelIndex].PanelIcon;
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
            _parent.VisiblePanels[buttonIndex].MyVisible = PanelList[buttonIndex].Checked;
        }
        void GetShownHiddenPanels()
        {
            int panelIndex = 0;
            foreach (MyToolStripButton t in ShowPanels.DropDownItems)
            {
                t.Checked = _parent.VisiblePanels[panelIndex++].MyVisible;
            }
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
