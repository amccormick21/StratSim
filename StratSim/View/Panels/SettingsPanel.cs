using DataSources;
using StratSim.Model;
using StratSim.View.MyFlowLayout;
using StratSim.ViewModel;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using MyFlowLayout;

namespace StratSim.View.Panels
{
    /// <summary>
    /// Represents a panel which displays all of the currently-held settings data.
    /// Provides functionality for updating this settings data
    /// </summary>
    public class SettingsPanel : MyPanel
    {

        Button OK, Cancel;
        TabControl SettingsTabControl;
        TabPage PDFSettings;
        TextBox txtPDFReader;
        Label lblPDFReader;
        TabPage FilePaths;
        TextBox txtTimingDataFolder;
        Label lblTimingDataPath;
        TabPage DriversandTracks;
        TextBox txtDataFolder;
        Label lblDataFolder;
        TabPage PaceDefaults;
        TextBox txtTopSpeed;
        Label lblTopSpeed;
        TextBox txtPrimeDegradation;
        Label lblPrimeTyreDeg;
        TextBox txtOptionDegradation;
        Label lblOptionDeg;
        TextBox txtTyreDelta;
        Label lblDefaultDelta;
        TabPage Overtaking;
        TextBox txtBackmarkerLoss;
        Label lblBackmarkerLoss;
        TextBox txtTimeLoss;
        Label lblTimeLoss;
        TextBox txtSpeedDelta;
        Label lblSpeedDelta;
        TextBox txtPaceDelta;
        Label lblPaceDelta;
        Label lblTimeLosses;
        Label lblRequirements;
        TextBox txtFuelEffect;
        Label lblFuelEffect;
        TextBox txtPace;
        Label lblDefaultPace;
        TextBox txtImprovementPerLap;
        TextBox txtEvolution4;
        TextBox txtEvolution2;
        TextBox txtEvolution3;
        TextBox txtEvolution1;
        TextBox txtEvolution0;
        Label lblTrackImprovement;
        Label lblTrackEvolution;
        TextBox txtFuelLoad;
        Label lblFuelLoad;
        Label lblPitStopLoss;
        TextBox txtTimeGap;
        Label lblFollowTimeGap;

        public SettingsPanel(MainForm FormToAdd)
            : base(600, 250, "Settings", FormToAdd, Properties.Resources.Settings)
        {
            InitialiseComponent();
            PopulateTextboxes();

            SetPanelProperties(DockTypes.BottomLeft, AutosizeTypes.Constant, FillStyles.None, this.Size);
        }

        protected override void PositionComponents()
        {
 	        base.PositionComponents();
            OK.Location = new Point(Width - 200, Height - 40);
            Cancel.Location = new Point(Width - 100, Height - 40);
        }

        /// <summary>
        /// Saves the current settings data and writes it to files if it is valid
        /// </summary>
        void ConfirmChanges()
        {
            bool incorrectValue = false;
            SaveSettingsData(ref incorrectValue);
            if (!incorrectValue)
            {
                Data.Settings.WriteSettingsData();
            }
            MyEvents.OnSettingsModified();
        }

        /// <summary>
        /// Reverts the settings data to previous values
        /// </summary>
        void RevertChanges()
        {
            Data.Settings.LoadData();
            PopulateTextboxes();
        }

        /// <summary>
        /// Populates the text boxes on the panel with settings data
        /// </summary>
        void PopulateTextboxes()
        {
            txtPaceDelta.Text = Convert.ToString(Data.Settings.RequiredPaceDelta);
            txtSpeedDelta.Text = Convert.ToString(Data.Settings.RequiredSpeedDelta);
            txtTimeLoss.Text = Convert.ToString(Data.Settings.TimeLoss);
            txtBackmarkerLoss.Text = Convert.ToString(Data.Settings.BackmarkerLoss);
            txtTimeGap.Text = Convert.ToString(Data.Settings.TimeGap);
            txtTyreDelta.Text = Convert.ToString(Data.Settings.DefaultCompoundDelta);
            txtOptionDegradation.Text = Convert.ToString(Data.Settings.DefaultOptionDegradation);
            txtPrimeDegradation.Text = Convert.ToString(Data.Settings.DefaultPrimeDegradation);
            txtTopSpeed.Text = Convert.ToString(Data.Settings.DefaultTopSpeed);
            txtFuelEffect.Text = Convert.ToString(Data.Settings.DefaultFuelEffect);
            txtPace.Text = Convert.ToString(Data.Settings.DefaultPace);
            txtFuelLoad.Text = Convert.ToString(Data.Settings.DefaultP2Fuel);
            txtDataFolder.Text = Data.Settings.DataFilePath;
            txtEvolution0.Text = Convert.ToString(Data.Settings.TrackEvolution[0]);
            txtEvolution1.Text = Convert.ToString(Data.Settings.TrackEvolution[1]);
            txtEvolution2.Text = Convert.ToString(Data.Settings.TrackEvolution[2]);
            txtEvolution3.Text = Convert.ToString(Data.Settings.TrackEvolution[3]);
            txtEvolution4.Text = Convert.ToString(Data.Settings.TrackEvolution[4]);
            txtImprovementPerLap.Text = Convert.ToString(Data.Settings.TrackImprovement);
            txtPDFReader.Text = Data.Settings.PDFReaderName;
            txtTimingDataFolder.Text = Data.Settings.TimingDataBaseFolder;
        }

        /// <summary>
        /// Sets the values of the text boxes to the current settings data.
        /// </summary>
        void SaveSettingsData(ref bool incorrectValue)
        {
            var dp = CultureInfo.InvariantCulture.NumberFormat;
            incorrectValue = false;

            Data.Settings.RequiredPaceDelta = Functions.ValidateNotEqualTo(float.Parse(txtPaceDelta.Text, dp), 0, "Required Pace Delta", ref incorrectValue, true);
            Data.Settings.RequiredSpeedDelta = Functions.ValidateNotEqualTo(float.Parse(txtSpeedDelta.Text, dp), 0, "Required Speed Delta", ref incorrectValue, true);
            Data.Settings.TimeLoss = Functions.ValidateGreaterThan(float.Parse(txtTimeLoss.Text, dp), 0, "Time Loss", ref incorrectValue, true);
            Data.Settings.BackmarkerLoss = Functions.ValidateGreaterThan(float.Parse(txtBackmarkerLoss.Text, dp), 0, "BackMarker Time Loss", ref incorrectValue, true);
            Data.Settings.TimeGap = Functions.ValidateBetweenExclusive(float.Parse(txtTimeGap.Text, dp), 0, 1, "Time Gap In Traffic", ref incorrectValue, true);
            Data.Settings.DefaultCompoundDelta = Functions.ValidateLessThan(float.Parse(txtTyreDelta.Text, dp), 0, "Default Compound Delta", ref incorrectValue, true);
            Data.Settings.DefaultOptionDegradation = Functions.ValidateGreaterThan(float.Parse(txtOptionDegradation.Text, dp), 0, "Default Option Degradation", ref incorrectValue, true);
            Data.Settings.DefaultPrimeDegradation = Functions.ValidateGreaterThan(float.Parse(txtPrimeDegradation.Text, dp), 0, "Default Prime Degradation", ref incorrectValue, true);
            Data.Settings.DefaultTopSpeed = Functions.ValidateGreaterThan(float.Parse(txtTopSpeed.Text, dp), 0, "Default Top Speed", ref incorrectValue, true);
            Data.Settings.DefaultFuelEffect = Functions.ValidateBetweenExclusive(float.Parse(txtFuelEffect.Text, dp), 0, 0.1F, "Default Fuel Effect", ref incorrectValue, true);
            Data.Settings.DefaultPace = Functions.ValidateGreaterThan(float.Parse(txtPace.Text, dp), 0, "Default Pace", ref incorrectValue, true);
            Data.Settings.DefaultP2Fuel = Functions.ValidateGreaterThan(float.Parse(txtFuelLoad.Text, dp), 0, "P2 Fuel Load", ref incorrectValue, true);
            Data.Settings.DataFilePath = txtDataFolder.Text;
            Data.Settings.TrackEvolution[0] = float.Parse(txtEvolution0.Text, dp);
            Data.Settings.TrackEvolution[1] = float.Parse(txtEvolution1.Text, dp);
            Data.Settings.TrackEvolution[2] = float.Parse(txtEvolution2.Text, dp);
            Data.Settings.TrackEvolution[3] = float.Parse(txtEvolution3.Text, dp);
            Data.Settings.TrackEvolution[4] = float.Parse(txtEvolution4.Text, dp);
            Data.Settings.TrackImprovement = float.Parse(txtImprovementPerLap.Text, dp);
            Data.Settings.PDFReaderName = txtPDFReader.Text;
            Data.Settings.TimingDataBaseFolder = @txtTimingDataFolder.Text;
        }

        void InitialiseComponent()
        {
            OK = new Button();
            Cancel = new Button();
            SettingsTabControl = new TabControl();
            Overtaking = new TabPage();
            txtBackmarkerLoss = new TextBox();
            lblBackmarkerLoss = new Label();
            txtTimeLoss = new TextBox();
            lblTimeLoss = new Label();
            txtSpeedDelta = new TextBox();
            lblSpeedDelta = new Label();
            txtPaceDelta = new TextBox();
            lblPaceDelta = new Label();
            lblTimeLosses = new Label();
            lblRequirements = new Label();
            PaceDefaults = new TabPage();
            lblPitStopLoss = new Label();
            txtFuelLoad = new TextBox();
            lblFuelLoad = new Label();
            txtPace = new TextBox();
            lblDefaultPace = new Label();
            txtFuelEffect = new TextBox();
            lblFuelEffect = new Label();
            txtTopSpeed = new TextBox();
            lblTopSpeed = new Label();
            txtPrimeDegradation = new TextBox();
            lblPrimeTyreDeg = new Label();
            txtOptionDegradation = new TextBox();
            lblOptionDeg = new Label();
            txtTyreDelta = new TextBox();
            lblDefaultDelta = new Label();
            DriversandTracks = new TabPage();
            txtImprovementPerLap = new TextBox();
            txtEvolution4 = new TextBox();
            txtEvolution2 = new TextBox();
            txtEvolution3 = new TextBox();
            txtEvolution1 = new TextBox();
            txtEvolution0 = new TextBox();
            lblTrackImprovement = new Label();
            lblTrackEvolution = new Label();
            txtDataFolder = new TextBox();
            lblDataFolder = new Label();
            PDFSettings = new TabPage();
            txtPDFReader = new TextBox();
            lblPDFReader = new Label();
            FilePaths = new TabPage();
            txtTimingDataFolder = new TextBox();
            lblTimingDataPath = new Label();
            lblFollowTimeGap = new Label();
            txtTimeGap = new TextBox();

            SettingsTabControl.SuspendLayout();
            Overtaking.SuspendLayout();
            PaceDefaults.SuspendLayout();
            DriversandTracks.SuspendLayout();
            PDFSettings.SuspendLayout();
            FilePaths.SuspendLayout();
            SuspendLayout();

            //OK
            OK.Size = new Size(90, 25);
            OK.Text = "OK";
            OK.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            OK.Click += OK_Click;
            Controls.Add(OK);

            //Cancel
            Cancel.Size = new Size(90, 25);
            Cancel.Text = "Cancel";
            Cancel.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            Cancel.Click += Cancel_Click;
            Controls.Add(Cancel);

            // 
            // SettingsTabControl
            // 
            SettingsTabControl.Controls.Add(Overtaking);
            SettingsTabControl.Controls.Add(PaceDefaults);
            SettingsTabControl.Controls.Add(DriversandTracks);
            SettingsTabControl.Controls.Add(PDFSettings);
            SettingsTabControl.Controls.Add(FilePaths);
            SettingsTabControl.Location = new Point(this.Left + this.MyPadding.Left, this.Top + this.MyPadding.Top);
            SettingsTabControl.Name = "SettingsTabControl";
            SettingsTabControl.SelectedIndex = 0;
            SettingsTabControl.Size = new Size(this.Width - this.MyPadding.Left - this.MyPadding.Right, this.Height - this.MyPadding.Top - this.MyPadding.Bottom - 50);
            SettingsTabControl.TabIndex = 3;
            // 
            // Overtaking
            // 
            Overtaking.Controls.Add(txtTimeGap);
            Overtaking.Controls.Add(lblFollowTimeGap);
            Overtaking.Controls.Add(txtBackmarkerLoss);
            Overtaking.Controls.Add(lblBackmarkerLoss);
            Overtaking.Controls.Add(txtTimeLoss);
            Overtaking.Controls.Add(lblTimeLoss);
            Overtaking.Controls.Add(txtSpeedDelta);
            Overtaking.Controls.Add(lblSpeedDelta);
            Overtaking.Controls.Add(txtPaceDelta);
            Overtaking.Controls.Add(lblPaceDelta);
            Overtaking.Controls.Add(lblTimeLosses);
            Overtaking.Controls.Add(lblRequirements);
            Overtaking.Location = new System.Drawing.Point(4, 22);
            Overtaking.Name = "Overtaking";
            Overtaking.Padding = new Padding(3);
            Overtaking.Size = SettingsTabControl.Size;
            Overtaking.TabIndex = 4;
            Overtaking.Text = "Overtaking";
            Overtaking.UseVisualStyleBackColor = true;
            // 
            // txtBackmarkerLoss
            // 
            txtBackmarkerLoss.Location = new System.Drawing.Point(319, 49);
            txtBackmarkerLoss.Name = "txtBackmarkerLoss";
            txtBackmarkerLoss.Size = new System.Drawing.Size(59, 20);
            txtBackmarkerLoss.TabIndex = 9;
            // 
            // lblBackmarkerLoss
            // 
            lblBackmarkerLoss.AutoSize = true;
            lblBackmarkerLoss.Location = new System.Drawing.Point(200, 55);
            lblBackmarkerLoss.Name = "lblBackmarkerLoss";
            lblBackmarkerLoss.Size = new System.Drawing.Size(116, 13);
            lblBackmarkerLoss.TabIndex = 8;
            lblBackmarkerLoss.Text = "Time loss (backmarker)";
            // 
            // txtTimeLoss
            // 
            txtTimeLoss.Location = new System.Drawing.Point(319, 27);
            txtTimeLoss.Name = "txtTimeLoss";
            txtTimeLoss.Size = new System.Drawing.Size(59, 20);
            txtTimeLoss.TabIndex = 7;
            // 
            // lblTimeLoss
            // 
            lblTimeLoss.AutoSize = true;
            lblTimeLoss.Location = new System.Drawing.Point(200, 33);
            lblTimeLoss.Name = "lblTimeLoss";
            lblTimeLoss.Size = new System.Drawing.Size(95, 13);
            lblTimeLoss.TabIndex = 6;
            lblTimeLoss.Text = "Time Loss (normal)";
            // 
            // txtSpeedDelta
            // 
            txtSpeedDelta.Location = new System.Drawing.Point(125, 52);
            txtSpeedDelta.Name = "txtSpeedDelta";
            txtSpeedDelta.Size = new System.Drawing.Size(59, 20);
            txtSpeedDelta.TabIndex = 5;
            // 
            // lblSpeedDelta
            // 
            lblSpeedDelta.AutoSize = true;
            lblSpeedDelta.Location = new System.Drawing.Point(13, 55);
            lblSpeedDelta.Name = "lblSpeedDelta";
            lblSpeedDelta.Size = new System.Drawing.Size(115, 13);
            lblSpeedDelta.TabIndex = 4;
            lblSpeedDelta.Text = "Required Speed Delta:";
            // 
            // txtPaceDelta
            // 
            txtPaceDelta.Location = new System.Drawing.Point(125, 30);
            txtPaceDelta.Name = "txtPaceDelta";
            txtPaceDelta.Size = new System.Drawing.Size(59, 20);
            txtPaceDelta.TabIndex = 3;
            // 
            // lblPaceDelta
            // 
            lblPaceDelta.AutoSize = true;
            lblPaceDelta.Location = new System.Drawing.Point(13, 33);
            lblPaceDelta.Name = "lblPaceDelta";
            lblPaceDelta.Size = new System.Drawing.Size(109, 13);
            lblPaceDelta.TabIndex = 2;
            lblPaceDelta.Text = "Required Pace Delta:";
            // 
            // lblTimeLosses
            // 
            lblTimeLosses.AutoSize = true;
            lblTimeLosses.Location = new System.Drawing.Point(197, 9);
            lblTimeLosses.Name = "lblTimeLosses";
            lblTimeLosses.Size = new System.Drawing.Size(66, 13);
            lblTimeLosses.TabIndex = 1;
            lblTimeLosses.Text = "Time Losses";
            // 
            // lblRequirements
            // 
            lblRequirements.AutoSize = true;
            lblRequirements.Location = new System.Drawing.Point(9, 9);
            lblRequirements.Name = "lblRequirements";
            lblRequirements.Size = new System.Drawing.Size(72, 13);
            lblRequirements.TabIndex = 0;
            lblRequirements.Text = "Requirements";
            // 
            // PaceDefaults
            // 
            PaceDefaults.Controls.Add(txtFuelLoad);
            PaceDefaults.Controls.Add(lblFuelLoad);
            PaceDefaults.Controls.Add(txtPace);
            PaceDefaults.Controls.Add(lblDefaultPace);
            PaceDefaults.Controls.Add(txtFuelEffect);
            PaceDefaults.Controls.Add(lblFuelEffect);
            PaceDefaults.Controls.Add(txtTopSpeed);
            PaceDefaults.Controls.Add(lblTopSpeed);
            PaceDefaults.Controls.Add(txtPrimeDegradation);
            PaceDefaults.Controls.Add(lblPrimeTyreDeg);
            PaceDefaults.Controls.Add(txtOptionDegradation);
            PaceDefaults.Controls.Add(lblOptionDeg);
            PaceDefaults.Controls.Add(txtTyreDelta);
            PaceDefaults.Controls.Add(lblDefaultDelta);
            PaceDefaults.Location = new System.Drawing.Point(4, 22);
            PaceDefaults.Name = "PaceDefaults";
            PaceDefaults.Padding = new Padding(3);
            PaceDefaults.Size = SettingsTabControl.Size;
            PaceDefaults.TabIndex = 3;
            PaceDefaults.Text = "Pace Defaults";
            PaceDefaults.UseVisualStyleBackColor = true;
            // 
            // txtFuelLoad
            // 
            txtFuelLoad.Location = new System.Drawing.Point(143, 121);
            txtFuelLoad.Name = "txtFuelLoad";
            txtFuelLoad.Size = new System.Drawing.Size(78, 20);
            txtFuelLoad.TabIndex = 13;
            // 
            // lblFuelLoad
            // 
            lblFuelLoad.AutoSize = true;
            lblFuelLoad.Location = new System.Drawing.Point(13, 123);
            lblFuelLoad.Name = "lblFuelLoad";
            lblFuelLoad.Size = new System.Drawing.Size(70, 13);
            lblFuelLoad.TabIndex = 12;
            lblFuelLoad.Text = "P2 Fuel Load";
            // 
            // txtPace
            // 
            txtPace.Location = new System.Drawing.Point(343, 35);
            txtPace.Name = "txtPace";
            txtPace.Size = new System.Drawing.Size(69, 20);
            txtPace.TabIndex = 11;
            // 
            // lblDefaultPace
            // 
            lblDefaultPace.AutoSize = true;
            lblDefaultPace.Location = new System.Drawing.Point(253, 38);
            lblDefaultPace.Name = "lblDefaultPace";
            lblDefaultPace.Size = new System.Drawing.Size(69, 13);
            lblDefaultPace.TabIndex = 10;
            lblDefaultPace.Text = "Default Pace";
            // 
            // txtFuelEffect
            // 
            txtFuelEffect.Location = new System.Drawing.Point(142, 93);
            txtFuelEffect.Name = "txtFuelEffect";
            txtFuelEffect.Size = new System.Drawing.Size(80, 20);
            txtFuelEffect.TabIndex = 9;
            // 
            // lblFuelEffect
            // 
            lblFuelEffect.AutoSize = true;
            lblFuelEffect.Location = new System.Drawing.Point(13, 93);
            lblFuelEffect.Name = "lblFuelEffect";
            lblFuelEffect.Size = new System.Drawing.Size(58, 13);
            lblFuelEffect.TabIndex = 8;
            lblFuelEffect.Text = "Fuel Effect";
            // 
            // txtTopSpeed
            // 
            txtTopSpeed.Location = new System.Drawing.Point(343, 6);
            txtTopSpeed.Name = "txtTopSpeed";
            txtTopSpeed.Size = new System.Drawing.Size(69, 20);
            txtTopSpeed.TabIndex = 7;
            // 
            // lblTopSpeed
            // 
            lblTopSpeed.AutoSize = true;
            lblTopSpeed.Location = new System.Drawing.Point(253, 9);
            lblTopSpeed.Name = "lblTopSpeed";
            lblTopSpeed.Size = new System.Drawing.Size(60, 13);
            lblTopSpeed.TabIndex = 6;
            lblTopSpeed.Text = "Top Speed";
            // 
            // txtPrimeDegradation
            // 
            txtPrimeDegradation.Location = new System.Drawing.Point(142, 63);
            txtPrimeDegradation.Name = "txtPrimeDegradation";
            txtPrimeDegradation.Size = new System.Drawing.Size(80, 20);
            txtPrimeDegradation.TabIndex = 5;
            // 
            // lblPrimeTyreDeg
            // 
            lblPrimeTyreDeg.AutoSize = true;
            lblPrimeTyreDeg.Location = new System.Drawing.Point(13, 66);
            lblPrimeTyreDeg.Name = "lblPrimeTyreDeg";
            lblPrimeTyreDeg.Size = new System.Drawing.Size(118, 13);
            lblPrimeTyreDeg.TabIndex = 4;
            lblPrimeTyreDeg.Text = "Prime Tyre Degradation";
            // 
            // txtOptionDegradation
            // 
            txtOptionDegradation.Location = new System.Drawing.Point(142, 35);
            txtOptionDegradation.Name = "txtOptionDegradation";
            txtOptionDegradation.Size = new System.Drawing.Size(80, 20);
            txtOptionDegradation.TabIndex = 3;
            // 
            // lblOptionDeg
            // 
            lblOptionDeg.AutoSize = true;
            lblOptionDeg.Location = new System.Drawing.Point(13, 38);
            lblOptionDeg.Name = "lblOptionDeg";
            lblOptionDeg.Size = new System.Drawing.Size(123, 13);
            lblOptionDeg.TabIndex = 2;
            lblOptionDeg.Text = "Option Tyre Degradation";
            // 
            // txtTyreDelta
            // 
            txtTyreDelta.Location = new System.Drawing.Point(141, 6);
            txtTyreDelta.Name = "txtTyreDelta";
            txtTyreDelta.Size = new System.Drawing.Size(81, 20);
            txtTyreDelta.TabIndex = 1;
            // 
            // lblDefaultDelta
            // 
            lblDefaultDelta.AutoSize = true;
            lblDefaultDelta.Location = new System.Drawing.Point(13, 9);
            lblDefaultDelta.Name = "lblDefaultDelta";
            lblDefaultDelta.Size = new System.Drawing.Size(113, 13);
            lblDefaultDelta.TabIndex = 0;
            lblDefaultDelta.Text = "Tyre Compound Delta:";
            // 
            // DriversandTracks
            // 
            DriversandTracks.Controls.Add(txtImprovementPerLap);
            DriversandTracks.Controls.Add(txtEvolution4);
            DriversandTracks.Controls.Add(txtEvolution2);
            DriversandTracks.Controls.Add(txtEvolution3);
            DriversandTracks.Controls.Add(txtEvolution1);
            DriversandTracks.Controls.Add(txtEvolution0);
            DriversandTracks.Controls.Add(lblTrackImprovement);
            DriversandTracks.Controls.Add(lblTrackEvolution);
            DriversandTracks.Controls.Add(txtDataFolder);
            DriversandTracks.Controls.Add(lblDataFolder);
            DriversandTracks.Location = new System.Drawing.Point(4, 22);
            DriversandTracks.Name = "DriversandTracks";
            DriversandTracks.Padding = new Padding(3);
            DriversandTracks.Size = SettingsTabControl.Size;
            DriversandTracks.TabIndex = 2;
            DriversandTracks.Text = "Drivers and Tracks";
            DriversandTracks.UseVisualStyleBackColor = true;
            // 
            // txtImprovementPerLap
            // 
            txtImprovementPerLap.Location = new System.Drawing.Point(159, 66);
            txtImprovementPerLap.Name = "txtImprovementPerLap";
            txtImprovementPerLap.Size = new System.Drawing.Size(147, 20);
            txtImprovementPerLap.TabIndex = 13;
            // 
            // txtEvolution4
            // 
            txtEvolution4.Location = new System.Drawing.Point(339, 36);
            txtEvolution4.Name = "txtEvolution4";
            txtEvolution4.Size = new System.Drawing.Size(39, 20);
            txtEvolution4.TabIndex = 12;
            // 
            // txtEvolution2
            // 
            txtEvolution2.Location = new System.Drawing.Point(249, 36);
            txtEvolution2.Name = "txtEvolution2";
            txtEvolution2.Size = new System.Drawing.Size(39, 20);
            txtEvolution2.TabIndex = 11;
            // 
            // txtEvolution3
            // 
            txtEvolution3.Location = new System.Drawing.Point(294, 36);
            txtEvolution3.Name = "txtEvolution3";
            txtEvolution3.ReadOnly = true;
            txtEvolution3.Size = new System.Drawing.Size(39, 20);
            txtEvolution3.TabIndex = 10;
            txtEvolution3.Text = "0";
            // 
            // txtEvolution1
            // 
            txtEvolution1.Location = new System.Drawing.Point(204, 36);
            txtEvolution1.Name = "txtEvolution1";
            txtEvolution1.Size = new System.Drawing.Size(39, 20);
            txtEvolution1.TabIndex = 9;
            // 
            // txtEvolution0
            // 
            txtEvolution0.Location = new System.Drawing.Point(159, 36);
            txtEvolution0.Name = "txtEvolution0";
            txtEvolution0.Size = new System.Drawing.Size(39, 20);
            txtEvolution0.TabIndex = 8;
            // 
            // lblTrackImprovement
            // 
            lblTrackImprovement.AutoSize = true;
            lblTrackImprovement.Location = new System.Drawing.Point(13, 69);
            lblTrackImprovement.Name = "lblTrackImprovement";
            lblTrackImprovement.Size = new System.Drawing.Size(99, 13);
            lblTrackImprovement.TabIndex = 7;
            lblTrackImprovement.Text = "Track Improvement";
            // 
            // lblTrackEvolution
            // 
            lblTrackEvolution.AutoSize = true;
            lblTrackEvolution.Location = new System.Drawing.Point(13, 39);
            lblTrackEvolution.Name = "lblTrackEvolution";
            lblTrackEvolution.Size = new System.Drawing.Size(81, 13);
            lblTrackEvolution.TabIndex = 6;
            lblTrackEvolution.Text = "Track evolution";
            // 
            // txtDataFolder
            // 
            txtDataFolder.Location = new System.Drawing.Point(159, 6);
            txtDataFolder.Name = "txtDataFolder";
            txtDataFolder.Size = new System.Drawing.Size(147, 20);
            txtDataFolder.TabIndex = 1;
            // 
            // lblDataFolder
            // 
            lblDataFolder.AutoSize = true;
            lblDataFolder.Location = new System.Drawing.Point(13, 9);
            lblDataFolder.Name = "lblDataFolder";
            lblDataFolder.Size = new System.Drawing.Size(130, 13);
            lblDataFolder.TabIndex = 0;
            lblDataFolder.Text = "File Path Containing Data:";
            // 
            // PDFSettings
            // 
            PDFSettings.Controls.Add(txtPDFReader);
            PDFSettings.Controls.Add(lblPDFReader);
            PDFSettings.Location = new System.Drawing.Point(4, 22);
            PDFSettings.Name = "PDFSettings";
            PDFSettings.Padding = new Padding(3);
            PDFSettings.Size = SettingsTabControl.Size;
            PDFSettings.TabIndex = 0;
            PDFSettings.Text = "PDF Reader";
            PDFSettings.UseVisualStyleBackColor = true;
            // 
            // txtPDFReader
            // 
            txtPDFReader.Location = new System.Drawing.Point(122, 7);
            txtPDFReader.Name = "txtPDFReader";
            txtPDFReader.Size = new System.Drawing.Size(150, 20);
            txtPDFReader.TabIndex = 1;
            // 
            // lblPDFReader
            // 
            lblPDFReader.AutoSize = true;
            lblPDFReader.Location = new System.Drawing.Point(13, 9);
            lblPDFReader.Name = "lblPDFReader";
            lblPDFReader.Size = new System.Drawing.Size(100, 13);
            lblPDFReader.TabIndex = 0;
            lblPDFReader.Text = "PDF Reader Name:";
            // 
            // FilePaths
            // 
            FilePaths.Controls.Add(txtTimingDataFolder);
            FilePaths.Controls.Add(lblTimingDataPath);
            FilePaths.Location = new System.Drawing.Point(4, 22);
            FilePaths.Name = "FilePaths";
            FilePaths.Padding = new Padding(3);
            FilePaths.Size = SettingsTabControl.Size;
            FilePaths.TabIndex = 1;
            FilePaths.Text = "File Paths";
            FilePaths.UseVisualStyleBackColor = true;
            // 
            // txtTimingDataFolder
            // 
            txtTimingDataFolder.Location = new System.Drawing.Point(157, 6);
            txtTimingDataFolder.Name = "txtTimingDataFolder";
            txtTimingDataFolder.Size = new System.Drawing.Size(160, 20);
            txtTimingDataFolder.TabIndex = 1;
            // 
            // lblTimingDataPath
            // 
            lblTimingDataPath.AutoSize = true;
            lblTimingDataPath.Location = new System.Drawing.Point(13, 9);
            lblTimingDataPath.Name = "lblTimingDataPath";
            lblTimingDataPath.Size = new System.Drawing.Size(126, 13);
            lblTimingDataPath.TabIndex = 0;
            lblTimingDataPath.Text = "Timing Data Base Folder:";
            // 
            // lblFollowTimeGap
            // 
            lblFollowTimeGap.AutoSize = true;
            lblFollowTimeGap.Location = new System.Drawing.Point(200, 75);
            lblFollowTimeGap.Name = "lblFollowTimeGap";
            lblFollowTimeGap.Size = new System.Drawing.Size(97, 13);
            lblFollowTimeGap.TabIndex = 10;
            lblFollowTimeGap.Text = "Time Gap in Traffic";
            // 
            // txtTimeGap
            // 
            txtTimeGap.Location = new System.Drawing.Point(319, 72);
            txtTimeGap.Name = "txtTimeGap";
            txtTimeGap.Size = new System.Drawing.Size(59, 20);
            txtTimeGap.TabIndex = 11;
            // 
            // Settings
            // 
            Controls.Add(SettingsTabControl);
            Name = "Settings";
            Text = "Settings";
            SettingsTabControl.ResumeLayout(false);
            Overtaking.ResumeLayout(false);
            Overtaking.PerformLayout();
            PaceDefaults.ResumeLayout(false);
            PaceDefaults.PerformLayout();
            DriversandTracks.ResumeLayout(false);
            DriversandTracks.PerformLayout();
            PDFSettings.ResumeLayout(false);
            PDFSettings.PerformLayout();
            FilePaths.ResumeLayout(false);
            FilePaths.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        void OK_Click(object sender, EventArgs e)
        {
            ConfirmChanges();
        }

        void Cancel_Click(object sender, EventArgs e)
        {
            RevertChanges();
        }
    }
}
