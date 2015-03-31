using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyFlowLayout;
using System.Drawing;
using System.Windows.Forms;
using StratSim.ViewModel;
using Graphing;

namespace StratSim.View.Panels
{
    /// <summary>
    /// Represents a panel with two axes and the ability to draw lines onto the panel.
    /// </summary>
    public class Graph : MyPanel
    {
        protected axisParameters horizontalAxis;
        protected axisParameters verticalAxis;
        protected NormalisationType graphNormalisationType;

        protected Rectangle graphArea;

        List<Label> horizontalAxisLabels = new List<Label>();
        List<Label> verticalAxisLabels = new List<Label>();

        public Graph(string graphTitle, NormalisationType normalisationType, MainForm formToLink, Image icon)
            : base (500, 400, graphTitle, formToLink, icon)
        {
            normalisationType = graphNormalisationType;

        }

        protected override void UnlinkFromForm(MainForm Form)
        {
            base.UnlinkFromForm(Form);
            Form.IOController.FlowLayoutEvents.MainPanelResizeStart -= MyEvents_MainPanelResizeStart;
            Form.IOController.FlowLayoutEvents.MainPanelResizeEnd -= MyEvents_MainPanelResizeEnd;
        }

        protected override void LinkToForm(MainForm Form)
        {
            base.LinkToForm(Form);
            Form.IOController.FlowLayoutEvents.MainPanelResizeStart += MyEvents_MainPanelResizeStart;
            Form.IOController.FlowLayoutEvents.MainPanelResizeEnd += MyEvents_MainPanelResizeEnd;
        }

        void MyEvents_MainPanelResizeStart()
        {
            graphArea = SetGraphArea();
        }

        void MyEvents_MainPanelResizeEnd()
        {
            Rectangle newGraphArea = SetGraphArea();
            ScaleAxes(graphArea, newGraphArea);

            graphArea = newGraphArea;
        }

        protected void MyEvents_AxesModified(axisParameters XAxis, axisParameters YAxis, NormalisationType normalisation, bool UserModified)
        {
            if (UserModified)
            {
                SetupAxes(XAxis, YAxis, normalisation);
            }
        }

        public void SetupAxes()
        {
            //TODO: setup axes based on the traces?
        }

        /// <summary>
        /// Sets the axes to the specified values
        /// </summary>
        void SetupAxes(axisParameters horizontalAxis, axisParameters verticalAxis, NormalisationType normalisation)
        {
            this.horizontalAxis = horizontalAxis;
            this.verticalAxis = verticalAxis;
            graphNormalisationType = normalisation;
            Invalidate();
        }

        /// <summary>
        /// Scales the axes after the availabe graph area is changed
        /// </summary>
        /// <param name="oldSize">The old graph area</param>
        /// <param name="newSize">The new graph area</param>
        void ScaleAxes(Rectangle oldSize, Rectangle newSize)
        {
            float scaleFactorX = (float)newSize.Width / (float)oldSize.Width;
            float scaleFactorY = (float)newSize.Height / (float)oldSize.Height;

            horizontalAxis.axisLabelSpacing = (int)Math.Round(scaleFactorX * horizontalAxis.axisLabelSpacing);
            if (horizontalAxis.axisLabelSpacing == 0) { horizontalAxis.axisLabelSpacing = 1; }
            verticalAxis.axisLabelSpacing = (int)Math.Round(scaleFactorY * verticalAxis.axisLabelSpacing);
            if (verticalAxis.axisLabelSpacing == 0) { verticalAxis.axisLabelSpacing = 1; }

            horizontalAxis.scaleFactor *= scaleFactorX;
            verticalAxis.scaleFactor *= scaleFactorY;

            verticalAxis.startLocation = (int)(scaleFactorY * verticalAxis.startLocation);

            MyEvents.OnAxesComputerGenerated(horizontalAxis, verticalAxis, graphNormalisationType);
        }
        /// <summary>
        /// Shows a detailed section of the axes around the specified lap
        /// </summary>
        /// <param name="displayUpToLap">The lap to show in detail</param>
        void ScaleAxes(int displayUpToLap)
        {
            int displayFromLap = (displayUpToLap - 15 < 0 ? 0 : displayUpToLap - 15);
            horizontalAxis.axisLabelSpacing = 1;
            horizontalAxis.baseOffset = displayFromLap;
            horizontalAxis.scaleFactor = (float)(graphArea.Width) / (float)(displayUpToLap - displayFromLap);
            MyEvents.OnAxesComputerGenerated(horizontalAxis, verticalAxis, graphNormalisationType);
        }

        void StrategyGraph_PanelSelected()
        {
            graphArea = SetGraphArea();
        }

        void StrategyGraph_OpenedInNewForm(MainForm Form)
        {
            Rectangle newGraphArea = SetGraphArea();
            ScaleAxes(graphArea, newGraphArea);

            graphArea = newGraphArea;
        }

        /// <summary>
        /// Draws the graph axes on the panel
        /// </summary>
        /// <param name="g">The OnPaint method graphics used to draw the axes</param>
        /// <param name="upToLap">The lap up to which the graph axes are to be drawn</param>
        void DrawAxes(Graphics g, int upToLap)
        {
            Label tempLabel;
            int lapNumber = horizontalAxis.baseOffset;
            int labelIndex = 0;

            int laps = upToLap;

            var axisPen = new Pen(Color.Black);
            var minorAxisPen = new Pen(Color.LightGray);

            //horizontal axis
            //draw the labels
            for (int labelLocation = horizontalAxis.startLocation; (labelLocation <= graphArea.Right) && (lapNumber <= laps); labelLocation += Math.Abs((int)Math.Round((horizontalAxis.scaleFactor * horizontalAxis.axisLabelSpacing))))
            {
                tempLabel = new Label();
                tempLabel.Location = new Point(labelLocation - 10, graphArea.Bottom);
                tempLabel.Text = Convert.ToString(lapNumber);
                lapNumber += horizontalAxis.axisLabelSpacing;
                tempLabel.Width = 20;
                horizontalAxisLabels.Add(tempLabel);
                this.Controls.Add(horizontalAxisLabels[labelIndex++]);

                //draw the vertical dividing lines
                g.DrawLine(minorAxisPen, labelLocation, graphArea.Top + 10, labelLocation, graphArea.Bottom - 10);
            }

            //draw the major axis
            g.DrawLine(axisPen, graphArea.Right, verticalAxis.startLocation + (int)(verticalAxis.baseOffset * verticalAxis.scaleFactor), horizontalAxis.startLocation, verticalAxis.startLocation + (int)(verticalAxis.baseOffset * verticalAxis.scaleFactor));

            //vertical axis
            //draw the labels
            labelIndex = 0;
            //above the axis
            for (int labelLocation = verticalAxis.startLocation + (int)(verticalAxis.baseOffset * verticalAxis.scaleFactor); labelLocation <= graphArea.Bottom - 10; labelLocation += 1 + (int)(verticalAxis.scaleFactor * verticalAxis.axisLabelSpacing))
            {
                tempLabel = new Label();
                tempLabel.Location = new Point(horizontalAxis.startLocation - 31, labelLocation - 6);
                tempLabel.Text = Convert.ToString((int)(((-labelLocation + verticalAxis.startLocation) / verticalAxis.scaleFactor) + verticalAxis.baseOffset));
                tempLabel.Width = 30;
                verticalAxisLabels.Add(tempLabel);
                this.Controls.Add(verticalAxisLabels[labelIndex++]);
            }

            //below the axis
            for (int labelLocation = verticalAxis.startLocation + (int)(verticalAxis.baseOffset * verticalAxis.scaleFactor); labelLocation >= graphArea.Top + 10; labelLocation -= 1 + (int)(verticalAxis.scaleFactor * verticalAxis.axisLabelSpacing))
            {
                tempLabel = new Label();
                tempLabel.Location = new Point(horizontalAxis.startLocation - 31, labelLocation - 6);
                tempLabel.Text = Convert.ToString((int)(((-labelLocation + verticalAxis.startLocation) / verticalAxis.scaleFactor) + verticalAxis.baseOffset));
                tempLabel.Width = 30;
                verticalAxisLabels.Add(tempLabel);
                this.Controls.Add(verticalAxisLabels[labelIndex++]);
            }

            //draw the major axis
            g.DrawLine(axisPen, horizontalAxis.startLocation, graphArea.Bottom - 10, horizontalAxis.startLocation, graphArea.Top + 10);
        }


        /// <summary>
        /// Gets the area available for traces on the graph
        /// </summary>
        /// <returns>A rectangle representing the available area on the panel for the graph traces</returns>
        protected Rectangle SetGraphArea()
        {
            Rectangle Area = new Rectangle();
            Area.Location = new Point(horizontalAxis.startLocation, 35);
            Area.Size = new Size(this.ClientRectangle.Width - 20 - horizontalAxis.startLocation, this.ClientRectangle.Height - 35 - 20);

            return Area;
        }


        //TODO: work out which parts of graph can be brought to here neat,
        // which bits need to be made virtual and overriden,
        // and which bits have to stay.
        //TODO: refactoring!
        //TODO: Strategy line needs to inherit from new class 'line'. Graph needs a type parameter,
        //e.g. Graph<Line>; StrategyGraph<StrategyLine>.

        //TODO: new Line class.
    }
}
