using StratSim.Model;
using StratSim.View.MyFlowLayout;
using StratSim.ViewModel;
using System;
using System.Drawing;
using System.Windows.Forms;
using MyFlowLayout;

namespace StratSim.View.Panels
{
    /// <summary>
    /// Panel that displays quick shortcuts to the program functions.
    /// Allows for data to be controlled through button clicks.
    /// Provides methods for hiding and showing the controls on the panel depending on system state.
    /// </summary>
    public class NewStartPanel : MyPanel
    {
        Label DemoPanel;
        Label SelectYear;
        ComboBox YearSelectBox;
        Label SelectRace;
        ComboBox RaceSelectBox;
        Button Archives;
        Button DataInput;
        Panel PaceParameters;
        Button PPFromRace;
        Button PPFromFile;
        Label PaceParametersTitle;
        Button ViewParameters;
        Panel Strategies;
        Button ViewStrategies;
        Button SFromData;
        Button SFromFile;
        Label StrategiesTitle;
        Panel Race;
        Label RaceTitle;
        Button ViewRace;

        MainForm linkedForm;

        bool paceParametersLoadedFromFile, strategiesLoadedFromFile;

        public NewStartPanel(MainForm Form)
            : base(450, 170, "Start", Form, Properties.Resources.Start)
        {
            InitializeComponent();
            LoadTracksComboBox();
            LinkedForm = Form;
            MyEvents.FinishedLoadingParameters += OnPaceParametersFinishedLoading;
            MyEvents.FinishedLoadingStrategies += OnStrategiesFinishedLoading;
        }

        void LoadTracksComboBox()
        {
            foreach (Track t in Data.Tracks)
            {
                RaceSelectBox.Items.Add(t.name);
            }
            RaceSelectBox.SelectedIndexChanged += RaceSelectBox_SelectedIndexChanged;

            YearSelectBox.Items.AddRange(new object[] { 2014, 2015 });
            YearSelectBox.SelectedIndex = 1;
            YearSelectBox.SelectedIndexChanged += YearSelectBox_SelectedIndexChanged;
        }

        void InitializeComponent()
        {
            SelectYear = new Label();
            YearSelectBox = new ComboBox();
            SelectRace = new Label();
            RaceSelectBox = new ComboBox();
            Archives = new Button();
            DataInput = new Button();
            PaceParameters = new Panel();
            ViewParameters = new Button();
            PPFromRace = new Button();
            PPFromFile = new Button();
            PaceParametersTitle = new Label();
            Strategies = new Panel();
            StrategiesTitle = new Label();
            SFromFile = new Button();
            SFromData = new Button();
            ViewStrategies = new Button();
            Race = new Panel();
            RaceTitle = new Label();
            ViewRace = new Button();
            DemoPanel = new Label();
            //
            // DemoPanel
            //
            DemoPanel.Location = new Point(15, 200);
            DemoPanel.Size = new Size(200, 40);
            DemoPanel.Text = "Welcome to StratSim. Click \"Instructions\" in the toolbar at the top to open a help window";
            //
            // Select Year
            //
            SelectYear.Location = new Point(10, 35);
            SelectYear.Size = new Size(100, 13);
            SelectYear.Text = "Select Year";
            //
            // Year Select Box
            //
            YearSelectBox.Location = new Point(110, 35);
            YearSelectBox.Size = new Size(100, 21);
            YearSelectBox.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            YearSelectBox.DropDownStyle = ComboBoxStyle.DropDownList;
            // 
            // SelectRace
            // 
            SelectRace.Location = new Point(10, 65);
            SelectRace.Size = new Size(100, 13);
            SelectRace.Text = "Select Race";
            // 
            // RaceSelectBox
            // 
            RaceSelectBox.Location = new Point(110, 65);
            RaceSelectBox.Size = new Size(100, 21);
            RaceSelectBox.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            RaceSelectBox.DropDownStyle = ComboBoxStyle.DropDownList;
            //
            // Archives
            //
            Archives.Location = new Point(330, 35);
            Archives.Size = new Size(100, 25);
            Archives.Text = "Archives";
            Archives.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            Archives.Click += Archives_Click;
            //
            // Data Input
            //
            DataInput.Location = new Point(220, 35);
            DataInput.Size = new Size(100, 25);
            DataInput.Text = "Input Data";
            DataInput.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            DataInput.Click += DataInput_Click;
            // 
            // PaceParameters
            // 
            PaceParameters.Controls.Add(ViewParameters);
            PaceParameters.Controls.Add(PPFromRace);
            PaceParameters.Controls.Add(PPFromFile);
            PaceParameters.Controls.Add(PaceParametersTitle);
            PaceParameters.Location = new Point(10, 95);
            PaceParameters.Size = new Size(440, 25);
            PaceParameters.Visible = false;
            // 
            // ViewParameters
            // 
            ViewParameters.Enabled = false;
            ViewParameters.Location = new Point(320, 0);
            ViewParameters.Size = new Size(100, 25);
            ViewParameters.Text = "View";
            ViewParameters.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            ViewParameters.Click += ViewParameters_Click;
            // 
            // PPFromRace
            // 
            PPFromRace.Location = new Point(210, 0);
            PPFromRace.Size = new Size(100, 25);
            PPFromRace.Text = "Race";
            PPFromRace.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            PPFromRace.Click += PPFromRace_Click;
            // 
            // PPFromFile
            // 
            PPFromFile.Location = new Point(100, 0);
            PPFromFile.Size = new Size(100, 25);
            PPFromFile.Text = "File";
            PPFromFile.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            PPFromFile.Click += PPFromFile_Click;
            // 
            // PaceParametersTitle
            // 
            PaceParametersTitle.Location = new Point(0, 0);
            PaceParametersTitle.Size = new Size(100, 13);
            PaceParametersTitle.Text = "Pace Parameters";
            //
            // Strategies
            // 
            Strategies.Controls.Add(ViewStrategies);
            Strategies.Controls.Add(SFromData);
            Strategies.Controls.Add(SFromFile);
            Strategies.Controls.Add(StrategiesTitle);
            Strategies.Location = new Point(10, 130);
            Strategies.Size = new Size(440, 25);
            Strategies.Visible = false;
            // 
            // StrategiesTitle
            // 
            StrategiesTitle.Location = new Point(0, 0);
            StrategiesTitle.Size = new Size(100, 13);
            StrategiesTitle.Text = "Strategies";
            // 
            // SFromFile
            // 
            SFromFile.Location = new Point(100, 0);
            SFromFile.Size = new Size(100, 25);
            SFromFile.Text = "File";
            SFromFile.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            SFromFile.Click += SFromFile_Click;
            // 
            // SFromData
            // 
            SFromData.Location = new Point(210, 0);
            SFromData.Size = new Size(100, 25);
            SFromData.Text = "Data";
            SFromData.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            SFromData.Click += SFromData_Click;
            // 
            // ViewStrategies
            // 
            ViewStrategies.Enabled = false;
            ViewStrategies.Location = new Point(320, 0);
            ViewStrategies.Size = new Size(100, 25);
            ViewStrategies.Text = "View";
            ViewStrategies.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            ViewStrategies.Click += ViewStrategies_Click;
            // 
            // Race
            // 
            Race.Controls.Add(ViewRace);
            Race.Controls.Add(RaceTitle);
            Race.Location = new Point(10, 165);
            Race.Size = new Size(440, 25);
            Race.Visible = false;
            // 
            // RaceTitle
            // 
            RaceTitle.Location = new Point(0, 0);
            RaceTitle.Size = new Size(100, 13);
            RaceTitle.Text = "Race";
            // 
            // ViewRace
            // 
            ViewRace.Enabled = true;
            ViewRace.Location = new Point(100, 0);
            ViewRace.Size = new Size(100, 25);
            ViewRace.Text = "View";
            ViewRace.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            ViewRace.Click += ViewRace_Click;

            Controls.Add(SelectYear);
            Controls.Add(YearSelectBox);
            Controls.Add(Race);
            Controls.Add(Strategies);
            Controls.Add(PaceParameters);
            Controls.Add(RaceSelectBox);
            Controls.Add(SelectRace);
            Controls.Add(Archives);
            Controls.Add(DataInput);
            Controls.Add(DemoPanel);
        }

        void OnPaceParametersFinishedLoading()
        {
            PaceParameters.Visible = true;
            Strategies.Visible = true;

            PPFromFile.Enabled = !paceParametersLoadedFromFile;
            PPFromRace.Enabled = paceParametersLoadedFromFile;

            ViewParameters.Enabled = true;
            ViewStrategies.Enabled = false;
            Race.Visible = false;
        }
        void OnStrategiesFinishedLoading()
        {
            PaceParameters.Visible = true;
            ViewParameters.Enabled = true;
            ViewStrategies.Enabled = true;

            SFromData.Enabled = strategiesLoadedFromFile;
            SFromFile.Enabled = !strategiesLoadedFromFile;

            Race.Visible = true;
        }

        void DataInput_Click(object sender, EventArgs e)
        {
            StratSimPanelControlEvents.OnShowDataInput(LinkedForm);
        }
        void ViewRace_Click(object sender, EventArgs e)
        {
            StratSimPanelControlEvents.OnShowRacePanel(LinkedForm);
        }
        void ViewStrategies_Click(object sender, EventArgs e)
        {
            StratSimPanelControlEvents.OnShowStrategies(LinkedForm);
        }
        void ViewParameters_Click(object sender, EventArgs e)
        {
            StratSimPanelControlEvents.OnShowPaceParameters(LinkedForm);
        }
        void RaceSelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Data.RaceIndex = RaceSelectBox.SelectedIndex;

            PaceParameters.Visible = true;
            ViewParameters.Enabled = false;
            ViewStrategies.Enabled = false;
            Strategies.Visible = false;
            Race.Visible = false;

            PPFromFile.Enabled = true;
            PPFromRace.Enabled = true;
            SFromData.Enabled = true;
            SFromFile.Enabled = true;

            StratSimPanelControlEvents.OnRemoveGraphPanels(LinkedForm);
        }
        void YearSelectBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.CurrentYear = Convert.ToInt32(YearSelectBox.SelectedItem);
        }
        void PPFromRace_Click(object sender, EventArgs e)
        {
            SFromData.Enabled = true;
            SFromFile.Enabled = true;
            paceParametersLoadedFromFile = false;
            StratSimPanelControlEvents.OnLoadPaceParametersFromRace();
        }
        void PPFromFile_Click(object sender, EventArgs e)
        {
            SFromData.Enabled = true;
            SFromFile.Enabled = true;
            paceParametersLoadedFromFile = true;
            StratSimPanelControlEvents.OnLoadPaceParametersFromFile();
        }
        void SFromFile_Click(object sender, EventArgs e)
        {
            strategiesLoadedFromFile = true;
            ViewStrategies.Enabled = false;
            SFromData.Enabled = true;
            StratSimPanelControlEvents.OnLoadStrategiesFromFile();
        }
        void SFromData_Click(object sender, EventArgs e)
        {
            strategiesLoadedFromFile = false;
            ViewStrategies.Enabled = false;
            StratSimPanelControlEvents.OnLoadStrategiesFromData();
        }
        void Archives_Click(object sender, EventArgs e)
        {
            StratSimPanelControlEvents.OnShowArchives(LinkedForm);
        }

        StratSimPanelControlEvents StratSimPanelControlEvents
        { get { return (StratSimPanelControlEvents)LinkedForm.IOController.PanelControlEvents; } }

        /// <summary>
        /// <para>Gets or sets the form to which the controls events are linked.</para>
        /// <para>This is not the same as parent form, which is the form on which this panel is displayed.</para>
        /// </summary>
        MainForm LinkedForm
        {
            get { return linkedForm; }
            set { linkedForm = value; }
        }
    }
}
