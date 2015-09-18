using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Graphing
{
    public class Graph : Panel
    {
        protected static Padding graphPadding = new Padding(20, 10, 20, 10);
        protected Point latestClickLocation;

        AxisParameters horizontalAxis, verticalAxis;
        protected bool AxesUserModified { get; set; }

        Rectangle graphArea;

        public event EventHandler<AxisParameters> HorizontalAxisModified;
        public AxisParameters HorizontalAxis
        {
            get { return horizontalAxis; }
            set
            {
                horizontalAxis = value;
                if (HorizontalAxisModified != null)
                    HorizontalAxisModified(this, value);
            }
        }

        public event EventHandler<AxisParameters> VerticalAxisModified;
        public AxisParameters VerticalAxis
        {
            get { return verticalAxis; }
            set
            {
                verticalAxis = value;
                if (VerticalAxisModified != null)
                    VerticalAxisModified(this, value);
            }
        }

        protected List<GraphLine> traces;
        protected List<GraphLine> originalTraces;

        public Graph()
            : base()
        {
            this.DoubleBuffered = true;
            this.Width = 100;
            this.Height = 100;
            traces = new List<GraphLine>();
            originalTraces = new List<GraphLine>();
            SetupContextMenu();
            VerticalAxisModified += Graph_VerticalAxisModified;
            HorizontalAxisModified += Graph_HorizontalAxisModified;
            Resize += Graph_Resize;
            MouseClick += Graph_MouseClick;
            AxesUserModified = false;
            SetGraphArea();
            SetupAxes();
        }

        public event EventHandler<DataPoint> GraphClicked;
        public void Graph_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                latestClickLocation = e.Location;
                this.ContextMenuStrip.Show(e.Location);
            }
        }

        void Graph_Resize(object sender, EventArgs e)
        {
            SetGraphArea();
        }

        void Graph_HorizontalAxisModified(object sender, AxisParameters e)
        {
            Invalidate();
        }

        void Graph_VerticalAxisModified(object sender, AxisParameters e)
        {
            Invalidate();
        }

        protected virtual void SetupContextMenu()
        {
            var menuStrip = new ContextMenuStrip();
            var button = new ToolStripButton("Add Action");
            button.Click += Action_Click;
            menuStrip.Items.Add(button);
            this.ContextMenuStrip = menuStrip;
        }

        private void Action_Click(object sender, EventArgs e)
        {
            if (latestClickLocation != null)
            {
                DataPoint clickData = GetPointData(latestClickLocation);
                if (GraphClicked != null)
                    GraphClicked(this, clickData);
            }
        }

        public void SetTraces(ICollection<GraphLine> traces)
        {
            originalTraces = new List<GraphLine>(traces);
            SetupAxes();
            Invalidate();
        }
        public void AddTrace(GraphLine trace)
        {
            this.originalTraces.Add(trace);
            SetupAxes();
            Invalidate();
        }

        /// <summary>
        /// Shows traces on the graph if the driver index is selected for show
        /// </summary>
        public void ShowTraces(List<int> tracesToShow)
        {
            foreach (GraphLine trace in originalTraces)
            {
                trace.Show = tracesToShow.Exists(i => i == trace.Index);
            }
        }

        /// <summary>
        /// Returns the trace that represents the trace with the lowest final value; the lowest cumulative time overall
        /// </summary>
        protected GraphLine GetBestTrace()
        {
            GraphLine bestTrace = null;
            if (traces.Count > 0)
            {
                double lowestTotal = 0;
                int tracesChecked = 0;
                foreach (GraphLine line in traces)
                {
                    if (line.Show)
                    {
                        if (tracesChecked++ == 0)
                        {
                            bestTrace = line;
                            lowestTotal = line.DataPoints.Last().Y;
                        }
                        else
                        {
                            if (line.DataPoints.Last().Y < lowestTotal)
                            {
                                bestTrace = line;
                            }
                        }
                    }
                }
            }
            return bestTrace;
        }

        public void SetHorizontalAxis(AxisParameters axis)
        {
            this.horizontalAxis = axis;
            AxesUserModified = true;
        }
        public void SetVerticalAxis(AxisParameters axis)
        {
            this.verticalAxis = axis;
            AxesUserModified = true;
        }

        private void SetGraphArea()
        {
            Rectangle originalGraphArea = graphArea;
            graphArea = new Rectangle(this.ClientRectangle.X + graphPadding.Left, this.ClientRectangle.Y + graphPadding.Top, this.ClientRectangle.Width - graphPadding.Horizontal, this.ClientRectangle.Height - graphPadding.Vertical);
            ResizeGraph(originalGraphArea, graphArea);
        }

        public void ResizeGraph(Rectangle originalGraphArea, Rectangle graphArea)
        {
            float scaleFactorX = (float)graphArea.Width / (float)originalGraphArea.Width;
            float scaleFactorY = (float)graphArea.Height / (float)originalGraphArea.Height;

            horizontalAxis.axisLabelSpacing = (int)Math.Round(scaleFactorX * horizontalAxis.axisLabelSpacing);
            if (horizontalAxis.axisLabelSpacing == 0) { horizontalAxis.axisLabelSpacing = 1; }
            verticalAxis.axisLabelSpacing = (int)Math.Round(scaleFactorY * verticalAxis.axisLabelSpacing);
            if (verticalAxis.axisLabelSpacing == 0) { verticalAxis.axisLabelSpacing = 1; }

            horizontalAxis.scaleFactor *= scaleFactorX;
            verticalAxis.scaleFactor *= scaleFactorY;

            horizontalAxis.startLocation = (int)(scaleFactorX * horizontalAxis.startLocation);
            verticalAxis.startLocation = (int)(scaleFactorY * verticalAxis.startLocation);

            if (scaleFactorX != 1 && HorizontalAxisModified != null)
                HorizontalAxisModified(this, horizontalAxis);
            if (scaleFactorY != 1 && VerticalAxisModified != null)
                VerticalAxisModified(this, verticalAxis);
        }

        public void ResizeGraph(double xValue)
        {
            ResizeGraph(xValue, 15);
        }

        public void ResizeGraph(double xValue, double xScale)
        {
            if (traces.Count > 0)
            {
                double highX = traces[0].HighestX();
                double lowX = traces[0].LowestX();

                if (xValue + (xScale / 2) > highX)
                {
                    horizontalAxis.baseOffset = (int)(highX - xScale);
                }
                else if (xValue - (xScale / 2) < lowX)
                {
                    horizontalAxis.baseOffset = 0;
                }
                else
                {
                    horizontalAxis.baseOffset = (int)(xValue - (xScale / 2));
                }
                horizontalAxis.scaleFactor = graphArea.Width / xScale;

                if (HorizontalAxisModified != null)
                    HorizontalAxisModified(this, horizontalAxis);
            }
        }

        public void SetupAxes(AxisParameters xAxis, AxisParameters yAxis)
        {
            horizontalAxis = xAxis;
            verticalAxis = yAxis;
        }

        public void SetupAxes(int xValues, double xStart, int yValues, double yStart)
        {
            horizontalAxis = new AxisParameters()
            {
                axisLabelSpacing = (int)(xValues * 20 / (double)graphArea.Width),
                baseOffset = 0,
                scaleFactor = graphArea.Width / (double)xValues,
                startLocation = (int)(xStart * graphArea.Width) + graphArea.Left
            };
            verticalAxis = new AxisParameters()
            {
                axisLabelSpacing = (int)(yValues * 20 / (double)graphArea.Height),
                baseOffset = 0,
                scaleFactor = graphArea.Height / (double)yValues,
                startLocation = (int)(yStart * graphArea.Height) + graphArea.Top
            };
        }

        public virtual void SetupAxes()
        {
            if (!AxesUserModified)
            {
                //If the user has not forced the axes to be displayed in a certain way
                if (traces.Count == 0)
                {
                    //No traces to use for scaling
                    horizontalAxis = verticalAxis = new AxisParameters()
                    {
                        startLocation = graphPadding.Left,
                        baseOffset = 0,
                        scaleFactor = 1,
                        axisLabelSpacing = 1
                    };
                }
                else
                {
                    //Set the axes based on the highest and lowest parameters in each axis.
                    double highX = traces[0].HighestX();
                    double highY = traces[0].HighestY();
                    double lowX = traces[0].LowestX();
                    double lowY = traces[0].LowestY();

                    for (int traceIndex = 1; traceIndex < traces.Count; traceIndex++)
                    {
                        if (traces[traceIndex].HighestX() > highX)
                            highX = traces[traceIndex].HighestX();
                        if (traces[traceIndex].HighestY() > highY)
                            highY = traces[traceIndex].HighestY();
                        if (traces[traceIndex].LowestX() < lowX)
                            lowX = traces[traceIndex].LowestX();
                        if (traces[traceIndex].LowestY() < lowY)
                            lowY = traces[traceIndex].LowestY();
                    }

                    horizontalAxis = new AxisParameters()
                    {
                        baseOffset = (int)lowX,
                        startLocation = (int)(graphArea.Width * (-lowX / (highX - lowX))),
                        scaleFactor = graphArea.Width / (highX - lowX),
                        axisLabelSpacing = 30
                    };
                    verticalAxis = new AxisParameters()
                    {
                        baseOffset = (int)lowY,
                        startLocation = (int)(graphArea.Height * (highY / (highY - lowY))),
                        scaleFactor = graphArea.Height / (highY - lowY),
                        axisLabelSpacing = 30
                    };
                }
            }
        }


        public void Redraw(System.Windows.Forms.PaintEventArgs e)
        {
            OnPaint(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.Clear(global::MyFlowLayout.Properties.Settings.Default.BackColour);
            DrawAxes(g);
            traces = new List<GraphLine>(originalTraces);
            GraphLine[] LinesToDraw = CalculateLines(originalTraces);
            DrawLines(g, LinesToDraw);
        }

        protected virtual void DrawLines(Graphics g, GraphLine[] LinesToDraw)
        {
            foreach (var line in LinesToDraw)
            {
                line.DrawLine(g);
            }
        }

        protected virtual GraphLine[] CalculateLines(List<GraphLine> tracesToCalculateFrom)
        {
            GraphLine[] linesToDraw = new GraphLine[tracesToCalculateFrom.Count];

            int lineIndex = 0;
            foreach (var lineOfData in tracesToCalculateFrom)
            {
                linesToDraw[lineIndex++] = GetLine(lineOfData);
            }

            return linesToDraw;
        }

        private GraphLine GetLine(GraphLine lineOfData)
        {
            GraphLine newLine = new GraphLine(lineOfData.Index, lineOfData.Show, lineOfData.LineColour);

            foreach (DataPoint p in lineOfData.DataPoints)
            {
                newLine.AddDataPoint(GetPointOrdinate(p.X, horizontalAxis, true), GetPointOrdinate(p.Y, verticalAxis, false), p.cycles);
            }

            return newLine;
        }

        /// <summary>
        /// Gets the ordinate on the specified axis that is represented by the locating value
        /// </summary>
        /// <param name="locator">The ordinate that is to be displayed</param>
        /// <param name="axis">The axis on which the ordinate is required</param>
        /// <returns>The value of the ordinate that the point is to be drawn at</returns>
        static int GetPointOrdinate(double locator, AxisParameters axis, bool horizontal)
        {
            double locatorToRepresent = locator + (horizontal ? -1 : 1) * axis.baseOffset;
            int position = (int)(axis.startLocation + (locatorToRepresent * axis.scaleFactor));

            return position;
        }

        protected DataPoint GetPointData(Point clickLocation)
        {
            DataPoint point = new DataPoint()
            {
                cycles = 0,
            };

            int x = clickLocation.X;
            int y = clickLocation.Y;

            point.X = Math.Round(((x - HorizontalAxis.startLocation) / HorizontalAxis.scaleFactor) - HorizontalAxis.baseOffset);
            point.Y = ((y - VerticalAxis.startLocation) / VerticalAxis.scaleFactor) + VerticalAxis.baseOffset;

            if (traces.Count > 0)
                point.index = GetClosestLineIndex(point.X, point.Y);

            return point;
        }

        private int GetClosestLineIndex(double xPosition, double yPosition)
        {
            int XPointIndex = (int)xPosition;
            int lineIndexHolder = -1;
            double shortestDistanceToLine = 0; //Can be assigned here as it is always set in the loop.
            double distanceToLine;
            double YData = GetYData(XPointIndex, yPosition);
            bool initialValueAssigned = false;
            for (int lineIndex = 0; lineIndex < traces.Count; lineIndex++)
            {
                if (traces[lineIndex].Show && traces[lineIndex].DataPoints.Count > XPointIndex)
                {
                    if (initialValueAssigned)
                    {
                        distanceToLine = Math.Abs(traces[lineIndex].DataPoints[XPointIndex].Y - YData);
                        if (distanceToLine < shortestDistanceToLine)
                        {
                            shortestDistanceToLine = distanceToLine;
                            lineIndexHolder = traces[lineIndex].Index;
                        }
                    }
                    else
                    {
                        shortestDistanceToLine = Math.Abs(traces[lineIndex].DataPoints[XPointIndex].Y - YData);
                        lineIndexHolder = traces[lineIndex].Index;
                        initialValueAssigned = true;
                    }
                }
            }
            return lineIndexHolder;
        }

        protected virtual double GetYData(int xPosition, double yPosition)
        {
            return yPosition;
        }


        private void DrawAxes(Graphics g)
        {
            int horizontalNumber = horizontalAxis.baseOffset;

            string labelText;
            PointF labelPoint;

            var axisPen = new Pen(Color.Black);
            var minorAxisPen = new Pen(Color.LightGray);
            minorAxisPen.DashPattern = new float[] { 3, 3 };
            var labelBrush = new SolidBrush(Color.Black);

            //horizontal axis
            //draw the labels
            //right of the vertical axis
            for (double labelLocation = horizontalAxis.startLocation + (horizontalAxis.baseOffset * horizontalAxis.scaleFactor); labelLocation <= graphArea.Right; labelLocation += (horizontalAxis.scaleFactor * horizontalAxis.axisLabelSpacing))
            {
                labelPoint = new PointF((float)(labelLocation - 10), (float)(verticalAxis.startLocation));
                labelText = Convert.ToString(Math.Round(((labelLocation - horizontalAxis.startLocation) / horizontalAxis.scaleFactor) + horizontalAxis.baseOffset));
                g.DrawString(labelText, SystemFonts.DefaultFont, labelBrush, labelPoint);
                //draw the vertical dividing lines
                g.DrawLine(minorAxisPen, (float)labelLocation, graphArea.Top, (float)labelLocation, graphArea.Bottom);
            }

            //left of the vertical axis
            for (double labelLocation = horizontalAxis.startLocation + (horizontalAxis.baseOffset * horizontalAxis.scaleFactor); labelLocation >= graphArea.Left; labelLocation -= (horizontalAxis.scaleFactor * horizontalAxis.axisLabelSpacing))
            {
                labelPoint = new PointF((float)(labelLocation - 10), (float)(verticalAxis.startLocation));
                labelText = Convert.ToString(Math.Round(((labelLocation - horizontalAxis.startLocation) / horizontalAxis.scaleFactor) + horizontalAxis.baseOffset));
                g.DrawString(labelText, SystemFonts.DefaultFont, labelBrush, labelPoint);
                //draw the vertical dividing lines
                g.DrawLine(minorAxisPen, (float)labelLocation, graphArea.Top, (float)labelLocation, graphArea.Bottom);
            }


            //draw the major axis
            g.DrawLine(axisPen, graphArea.Right, (float)(verticalAxis.startLocation + (verticalAxis.baseOffset * verticalAxis.scaleFactor)), horizontalAxis.startLocation, (float)(verticalAxis.startLocation + (verticalAxis.baseOffset * verticalAxis.scaleFactor)));

            //vertical axis
            //draw the labels
            //above the axis
            for (double labelLocation = verticalAxis.startLocation + (verticalAxis.baseOffset * verticalAxis.scaleFactor); labelLocation <= graphArea.Bottom; labelLocation += (verticalAxis.scaleFactor * verticalAxis.axisLabelSpacing))
            {
                labelPoint = new PointF((float)(horizontalAxis.startLocation - 21), (float)(labelLocation - 5));
                labelText = Convert.ToString(Math.Round(((-labelLocation + verticalAxis.startLocation) / verticalAxis.scaleFactor) + verticalAxis.baseOffset));
                g.DrawString(labelText, SystemFonts.DefaultFont, labelBrush, labelPoint);
            }

            //below the axis
            for (double labelLocation = verticalAxis.startLocation + (verticalAxis.baseOffset * verticalAxis.scaleFactor); labelLocation >= graphArea.Top; labelLocation -= (verticalAxis.scaleFactor * verticalAxis.axisLabelSpacing))
            {
                labelPoint = new PointF((float)(horizontalAxis.startLocation - 21), (float)(labelLocation - 5));
                labelText = Convert.ToString(Math.Round(((-labelLocation + verticalAxis.startLocation) / verticalAxis.scaleFactor) + verticalAxis.baseOffset));
                g.DrawString(labelText, SystemFonts.DefaultFont, labelBrush, labelPoint);
            }

            //draw the major axis
            g.DrawLine(axisPen, horizontalAxis.startLocation, graphArea.Bottom, horizontalAxis.startLocation, graphArea.Top);

            axisPen.Dispose();
            minorAxisPen.Dispose();
            labelBrush.Dispose();
        }
    }
}
