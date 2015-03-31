using StratSim.Model;
using MyFlowLayout;
using StratSim.View.MyFlowLayout;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace StratSim.View.Panels
{
    /// <summary>
    /// Represents a panel which displays information about the current state of the system.
    /// Includes data about the current driver and track
    /// </summary>
    public class InfoPanel : MyPanel
    {
        Panel driverInfo;
        Panel trackInfo;

        TextBox driverName, driverNumber, driverTeam;
        TextBox trackName, trackLaps, trackTimeLoss;

        bool showDriverInfo;
        bool showTrackInfo;

        public InfoPanel(MainForm FormToAdd)
            : base(150, 300, "Info", FormToAdd, Properties.Resources.Info)
        {
            showDriverInfo = false;
            showTrackInfo = false;

            InitialiseControls();
            AddControls();

            SetPanelProperties(DockTypes.TopLeft, AutosizeTypes.AutoHeight, FillStyles.None, this.Size);
        }

        void InitialiseControls()
        {
            driverInfo = new Panel();
            trackInfo = new Panel();

            driverName = new TextBox();
            driverNumber = new TextBox();
            driverTeam = new TextBox();
            trackName = new TextBox();
            trackLaps = new TextBox();
            trackTimeLoss = new TextBox();
        }

        /// <summary>
        /// Populates and adds the controls to the panel
        /// </summary>
        void AddControls()
        {
            int defaultLeftPosition = this.MyPadding.Left;
            int defaultWidth = this.Width - this.MyPadding.Left - this.MyPadding.Right;
            int PositionY = this.MyPadding.Top;
            //markPositionY gives the top position of the current panel
            int markPositionY = PositionY;

            if (showDriverInfo)
            {
                driverInfo.Top = PositionY;
                markPositionY = PositionY;
                PositionY += 4;

                driverInfo.Width = defaultWidth;
                driverInfo.Left = defaultLeftPosition;

                driverName.BackColor = Data.CurrentDriver.LineColour;
                if (Data.CurrentDriver.LineColour.GetBrightness() < 0.5) { driverName.ForeColor = Color.White; }
                else { driverName.ForeColor = Color.Black; }
                driverName.Text = Data.CurrentDriver.DriverName;
                driverName.Location = new Point(defaultLeftPosition, PositionY - markPositionY);
                PositionY += driverName.Height + 4;
                driverInfo.Controls.Add(driverName);

                driverNumber.Text = Convert.ToString(Data.CurrentDriver.DriverNumber);
                driverNumber.Location = new Point(defaultLeftPosition, PositionY - markPositionY);
                PositionY += driverNumber.Height + 4;
                driverInfo.Controls.Add(driverNumber);

                driverTeam.Text = Data.CurrentDriver.Team;
                driverTeam.Location = new Point(defaultLeftPosition, PositionY - markPositionY);
                PositionY += driverTeam.Height + 4;
                driverInfo.Controls.Add(driverTeam);

                driverInfo.Height = PositionY - markPositionY;
                this.Controls.Add(driverInfo);
            }

            markPositionY = PositionY;

            if (showTrackInfo)
            {
                trackInfo.Top = PositionY;
                markPositionY = PositionY;
                PositionY += 4;

                trackInfo.Width = defaultWidth;
                trackInfo.Left = defaultLeftPosition;

                trackName.Text = Data.CurrentTrack.name;
                trackName.Location = new Point(defaultLeftPosition, PositionY - markPositionY);
                PositionY += trackName.Height + 4;
                trackInfo.Controls.Add(trackName);

                trackLaps.Text = Convert.ToString(Data.CurrentTrack.laps) + " laps";
                trackLaps.Location = new Point(defaultLeftPosition, PositionY - markPositionY);
                PositionY += trackLaps.Height + 4;
                trackInfo.Controls.Add(trackLaps);

                trackTimeLoss.Text = Convert.ToString(Data.CurrentTrack.pitStopLoss) + "s pit loss";
                trackTimeLoss.Location = new Point(defaultLeftPosition, PositionY - markPositionY);
                PositionY += trackTimeLoss.Height + 4;
                trackInfo.Controls.Add(trackTimeLoss);

                trackInfo.Height = PositionY - markPositionY;
                this.Controls.Add(trackInfo);
            }
        }

        public bool ShowDriverInfo
        {
            get { return showDriverInfo; }
            set
            {
                showDriverInfo = value;
                AddControls();
            }
        }
        public bool ShowTrackInfo
        {
            get { return showTrackInfo; }
            set
            {
                showTrackInfo = value;
                AddControls();
            }
        }
    }
}
