using DataSources;
using StratSim.Model;
using StratSim.View.MyFlowLayout;
using StratSim.View.UserControls;
using StratSim.ViewModel;
using System;
using System.Drawing;
using System.Windows.Forms;
using MyFlowLayout;
using Graphing;

namespace StratSim.View.Panels
{
    /// <summary>
    /// Displays data about graph axes on a panel,
    /// and provides methods for modifying the axes
    /// </summary>
    public class AxesWindow : MyPanel
    {
        TableLayoutPanel tableOfParameters;
        Label horizontalAxisTitle, verticalAxisTitle;
        Label Offset, ScaleFactor, StartPoint, LabelSpacing;
        TextBox[,] axisParameterDataBoxes;
        Button OK, Cancel;
        ComboBox NormalisationTypeSelected;

        axisParameters localHorizontalAxis, localVerticalAxis;
        NormalisationType localNormalisationType;

        public AxesWindow(MainForm FormToAdd)
            : base(250, 250, "Axes", FormToAdd, Properties.Resources.Axes)
        {
            InitialiseControls();
            AddControls();

            MyEvents.AxesChangedByComputer += MyEvents_AxesModified;

            SetPanelProperties(DockTypes.Bottom, AutosizeTypes.Constant, FillStyles.None, this.Size);
        }

        void MyEvents_AxesModified(axisParameters XAxis, axisParameters YAxis, NormalisationType normalisation)
        {
            localHorizontalAxis = XAxis;
            localVerticalAxis = YAxis;
            PopulateAxesBoxes(XAxis, YAxis, normalisation);
        }

        protected override void PositionComponents()
        {
            base.PositionComponents();
            tableOfParameters.Width = this.ClientSize.Width - 20;
            tableOfParameters.Height = this.ClientSize.Height - 40;
            OK.Location = new Point(this.Width - 160, this.Height - 40);
            Cancel.Location = new Point(this.Width - 80, this.Height - 40);
        }

        /// <summary>
        /// Sets up the controls to be displayed on the panel
        /// </summary>
        void InitialiseControls()
        {
            Size ControlDefault = new Size(70, 25);
            MyToolTip toolTip;

            tableOfParameters = new TableLayoutPanel();
            tableOfParameters.ColumnCount = 3;
            tableOfParameters.RowCount = 5;
            tableOfParameters.Location = new Point(0, 25);

            OK = new Button();
            OK.Size = new Size(60, 25);
            OK.Text = "OK";
            OK.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            OK.Click += OK_Click;
            Cancel = new Button();
            Cancel.Size = new Size(60, 25);
            Cancel.Text = "Cancel";
            Cancel.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            Cancel.Click += Cancel_Click;

            horizontalAxisTitle = new Label();
            horizontalAxisTitle.Text = "X axis";
            horizontalAxisTitle.Size = ControlDefault;

            verticalAxisTitle = new Label();
            verticalAxisTitle.Text = "Y axis";
            verticalAxisTitle.Size = ControlDefault;

            Offset = new Label();
            Offset.Text = "Offset";
            Offset.Size = ControlDefault;
            toolTip = new MyToolTip(Offset, "Start location of the axis in units");

            ScaleFactor = new Label();
            ScaleFactor.Text = "Scale";
            ScaleFactor.Size = ControlDefault;
            toolTip = new MyToolTip(ScaleFactor, "Pixels per unit scale of the axis");

            StartPoint = new Label();
            StartPoint.Text = "Start Location";
            StartPoint.Size = ControlDefault;
            toolTip = new MyToolTip(StartPoint, "Start location of the axis in pixels, with \r\nrespect to the top left corner of the panel");

            LabelSpacing = new Label();
            LabelSpacing.Text = "Label Spacing";
            LabelSpacing.Size = ControlDefault;
            toolTip = new MyToolTip(LabelSpacing, "Unit spacing of labels on the axis");

            NormalisationTypeSelected = new ComboBox();
            foreach (var n in (NormalisationType[])Enum.GetValues(typeof(NormalisationType)))
            {
                NormalisationTypeSelected.Items.Add(Convert.ToString(n));
            }
            NormalisationTypeSelected.Location = new Point(50, 180);
            NormalisationTypeSelected.DropDownStyle = ComboBoxStyle.DropDownList;
            NormalisationTypeSelected.FlatStyle = global::MyFlowLayout.Properties.Settings.Default.FlatStyle;
            toolTip = new MyToolTip(NormalisationTypeSelected, "Normalisation type of the graph, either on an average or on every lap");

            axisParameterDataBoxes = new TextBox[2, 4];
            for (int column = 0; column <= 1; column++)
            {
                for (int j = 0; j <= 3; j++)
                {
                    axisParameterDataBoxes[column, j] = new TextBox();
                    axisParameterDataBoxes[column, j].Size = ControlDefault;
                }
            }
        }

        /// <summary>
        /// Displays the controls on the panel
        /// </summary>
        void AddControls()
        {
            tableOfParameters.Controls.Add(horizontalAxisTitle, 1, 0);
            tableOfParameters.Controls.Add(verticalAxisTitle, 2, 0);
            tableOfParameters.Controls.Add(Offset, 0, 1);
            tableOfParameters.Controls.Add(ScaleFactor, 0, 2);
            tableOfParameters.Controls.Add(StartPoint, 0, 3);
            tableOfParameters.Controls.Add(LabelSpacing, 0, 4);

            for (int columnIndex = 0; columnIndex <= 1; columnIndex++)
            {
                for (int rowIndex = 0; rowIndex <= 3; rowIndex++)
                {
                    tableOfParameters.Controls.Add(axisParameterDataBoxes[columnIndex, rowIndex], columnIndex + 1, rowIndex + 1);
                }
            }

            this.Controls.Add(OK);
            this.Controls.Add(Cancel);

            this.Controls.Add(NormalisationTypeSelected);

            this.Controls.Add(tableOfParameters);
        }


        void Cancel_Click(object sender, EventArgs e)
        {
            //Populates the text boxes from the file, resetting the values
            PopulateAxesBoxes(localHorizontalAxis, localVerticalAxis, localNormalisationType);
        }

        void OK_Click(object sender, EventArgs e)
        {
            //Validate the input
            bool incorrectValue = false;
            PopulateAxisValues(ref incorrectValue);

            if (!incorrectValue) //If all values are within reasonable bounds
            {
                //Modify the axes
                MyEvents.OnAxesChangedByUser(localHorizontalAxis, localVerticalAxis, localNormalisationType);
            }
        }

        /// <summary>
        /// Populates the text boxes on the panel with data from the given axes
        /// </summary>
        void PopulateAxesBoxes(axisParameters horizontalAxis, axisParameters verticalAxis, NormalisationType normalisationType)
        {
            axisParameterDataBoxes[0, 0].Text = Convert.ToString(horizontalAxis.baseOffset);
            axisParameterDataBoxes[0, 1].Text = Convert.ToString(horizontalAxis.scaleFactor);
            axisParameterDataBoxes[0, 2].Text = Convert.ToString(horizontalAxis.startLocation);
            axisParameterDataBoxes[0, 3].Text = Convert.ToString(horizontalAxis.axisLabelSpacing);

            axisParameterDataBoxes[1, 0].Text = Convert.ToString(verticalAxis.baseOffset);
            axisParameterDataBoxes[1, 1].Text = Convert.ToString(verticalAxis.scaleFactor);
            axisParameterDataBoxes[1, 2].Text = Convert.ToString(verticalAxis.startLocation);
            axisParameterDataBoxes[1, 3].Text = Convert.ToString(verticalAxis.axisLabelSpacing);

            NormalisationTypeSelected.SelectedIndex = (int)normalisationType;
        }

        /// <summary>
        /// Validates the data in the text boxes,
        /// and populates the local axis data with the data from the text boxes
        /// </summary>
        /// <param name="incorrectValue">Returns true if any one of the data items is invalid</param>
        void PopulateAxisValues(ref bool incorrectValue)
        {
            localHorizontalAxis.baseOffset = (int)Functions.ValidateBetweenInclusive(Convert.ToInt32(axisParameterDataBoxes[0, 0].Text), 0, Data.GetRaceLaps(), "Horizontal Axis Base Offset", ref incorrectValue, true);
            localHorizontalAxis.scaleFactor = Functions.ValidateGreaterThanEqualTo(Convert.ToSingle(axisParameterDataBoxes[0, 1].Text), 0, "Horizontal Axis Scale", ref incorrectValue, true);
            localHorizontalAxis.startLocation = (int)Functions.ValidateGreaterThan(Convert.ToInt32(axisParameterDataBoxes[0, 2].Text), 0, "Horizontal Axis Start", ref incorrectValue, true);
            localHorizontalAxis.axisLabelSpacing = (int)Functions.ValidateGreaterThan(Convert.ToInt32(axisParameterDataBoxes[0, 3].Text), 1, "Horizontal Axis Label Spacing", ref incorrectValue, true);

            localVerticalAxis.baseOffset = (int)Functions.ValidateBetweenInclusive(Convert.ToInt32(axisParameterDataBoxes[1, 0].Text), -75, 75, "Vertical Axis Base Offset", ref incorrectValue, true);
            localVerticalAxis.scaleFactor = Functions.ValidateGreaterThanEqualTo(Convert.ToSingle(axisParameterDataBoxes[1, 1].Text), 0, "Vertical Axis Scale", ref incorrectValue, true);
            localVerticalAxis.startLocation = (int)Functions.ValidateGreaterThan(Convert.ToInt32(axisParameterDataBoxes[1, 2].Text), 0, "Vertical Axis Start", ref incorrectValue, true);
            localVerticalAxis.axisLabelSpacing = (int)Functions.ValidateGreaterThan(Convert.ToInt32(axisParameterDataBoxes[1, 3].Text), 1, "Vertical Axis Label Spacing", ref incorrectValue, true);

            localNormalisationType = (NormalisationType)NormalisationTypeSelected.SelectedIndex;
        }
    }
}
