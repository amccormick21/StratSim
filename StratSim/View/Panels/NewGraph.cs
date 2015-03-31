using MyFlowLayout;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graphing;
using StratSim.ViewModel;
using StratSim.Model;
using StratSim.View.UserControls;

namespace StratSim.View.Panels
{
    public class NewGraph : MyPanel
    {
        protected DriverSelectPanel DriverPanel;
        internal void SetDriverPanel(DriverSelectPanel driverSelectPanel)
        {
            DriverPanel = driverSelectPanel;
        }

        protected CyclingGraph graphPanel;
        internal CyclingGraph GraphPanel
        {
            get { return graphPanel; }
            set { graphPanel = value; }
        }

        public NewGraph(string graphTitle, MainForm formToLink, Image icon)
            : base(500, 400, graphTitle, formToLink, icon)
        {
            LoadGraph();

            PanelOpened += NewGraph_PanelOpened;
            PanelClosed += NewGraph_PanelClosed;

            SetPanelProperties(DockTypes.Top, AutosizeTypes.Free, FillStyles.None, this.Size);
        }

        void NewGraph_PanelClosed(MainForm LeavingForm)
        {
            //Unsubscribe from events
            MyEvents.AxesChangedByUser -= MyEvents_AxesModifiedByUser;
            MyEvents.LapNumberChanged -= MyEvents_LapNumberChanged;
            GraphPanel.GraphClicked -= GraphPanel_GraphClicked;
        }

        void NewGraph_PanelOpened(MainForm OpenedOnForm)
        {
            //Subscribe to events
            MyEvents.AxesChangedByUser += MyEvents_AxesModifiedByUser;
            MyEvents.LapNumberChanged += MyEvents_LapNumberChanged;
            GraphPanel.GraphClicked += GraphPanel_GraphClicked;
            GraphPanel.HorizontalAxisModified += AxisModified;
            GraphPanel.VerticalAxisModified += AxisModified;

            //Setup axes based on panel size
            GraphPanel.SetupAxes(StratSim.Model.Data.GetRaceLaps(), 0, 100, 0.5);
        }

        private void AxisModified(object sender, axisParameters e)
        {
            MyEvents.OnAxesComputerGenerated(GraphPanel.HorizontalAxis, GraphPanel.VerticalAxis, GraphPanel.NormalisationType);
            Invalidate();
        }

        private void MyEvents_LapNumberChanged(int newLapNumber)
        {
            GraphPanel.ResizeGraph(newLapNumber);
        }

        private void MyEvents_AxesModifiedByUser(axisParameters horizontalAxis, axisParameters verticalAxis, NormalisationType normalisation)
        {
            GraphPanel.SetNormalisationType(normalisation);
            GraphPanel.SetHorizontalAxis(horizontalAxis);
            GraphPanel.SetVerticalAxis(verticalAxis);
            Invalidate();
        }

        protected override void OnMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseClick(e);
            GraphPanel.Graph_MouseClick(this, e);
        }

        private void LoadGraph()
        {
            graphPanel = new CyclingGraph();
            GraphPanel.SetNormalisationType(NormalisationType.OnAverageValue);
            GraphPanel.CycleGraph = true;
            Invalidate();
            this.Resize += NewGraph_Resize;
        }

        void NewGraph_Resize(object sender, EventArgs e)
        {
            graphPanel.Top = this.ClientRectangle.Top;
            graphPanel.Left = this.ClientRectangle.Left;
            graphPanel.Width = this.ClientRectangle.Width;
            graphPanel.Height = this.ClientRectangle.Height;
        }

        public event EventHandler<DataPoint> GraphRightClick;
        void GraphPanel_GraphClicked(object sender, DataPoint clickLocation)
        {
            if (clickLocation.index != -1)
            {
                if (GraphRightClick != null)
                    GraphRightClick(sender, clickLocation);
            }
        }

        /// <summary>
        /// Draws the graph to the screen
        /// </summary>
        /// <param name="tracesToShow">A list of GraphLine traces to show on the graph</param>
        /// <param name="showAllOnGraph">Represents whether all traces should be shown, or only the traces specified in the driver select panel</param>
        /// <param name="changeNormalised">Represents whether the graph should be re-normalised on the fastest trace</param>
        public void DrawGraph(List<GraphLine> tracesToShow, bool showAllOnGraph, bool changeNormalised)
        {
            GraphPanel.SetTraces(tracesToShow);
            int bestDriver;
            if (changeNormalised)
            {
                GraphPanel.NormaliseOnBestTrace();
                bestDriver = GraphPanel.NormalisationIndex;
                Data.DriverIndex = bestDriver;

                DriverPanel.UpdateRadioButtons(bestDriver);
                DriverPanel.UpdateCheckBoxes(ShowTracesState.All);
            }
            else
            {
                bestDriver = Data.DriverIndex;
            }
            Invalidate();
        }

        /// <summary>
        /// Shows traces on the graph if the driver index is selected for show
        /// </summary>
        public void ShowTraces()
        {
            GraphPanel.ShowTraces(DriverPanel.SelectedDriverIndices);
            Invalidate();
        }

        /// <summary>
        /// Changes the trace on which the graph is normalised,
        /// and redraws the graph
        /// </summary>
        /// <param name="SelectedDriver">The new driver to normalise the graph on</param>
        public void ChangeNormalised(int SelectedDriver)
        {
            Data.DriverIndex = SelectedDriver;
            GraphPanel.SetNormalisationIndex(SelectedDriver);
            DriverPanel.UpdateCheckBoxes(ShowTracesState.Selected);
            DriverPanel.UpdateRadioButtons(SelectedDriver);
            Invalidate();
        }

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            base.OnPaint(e);
            GraphPanel.Redraw(e);
        }
    }
}
