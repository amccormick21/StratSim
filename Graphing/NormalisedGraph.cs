using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Graphing
{
    public class NormalisedGraph : Graph
    {
        NormalisationType normalisationType;
        public event EventHandler<NormalisationType> NormalisationTypeChanged;
        public NormalisationType NormalisationType
        {
            get { return normalisationType; }
            private set
            {
                normalisationType = value;
                if (NormalisationTypeChanged != null)
                    NormalisationTypeChanged(this, value);
            }
        }

        int normalisationIndex;
        public event EventHandler<int> NormalisationIndexChanged;
        public int NormalisationIndex
        {
            get { return normalisationIndex; }
            private set
            {
                normalisationIndex = value;
                if (NormalisationIndexChanged != null)
                    NormalisationIndexChanged(this, value);
            }
        }

        List<int> normalisationLaps;
        public event EventHandler<List<int>> NormalisationZonesChanged;
        public List<int> NormalisationLaps
        {
            get { return normalisationLaps; }
            set {
                normalisationLaps = value;
                if (NormalisationZonesChanged != null)
                    NormalisationZonesChanged(this, value);
            }
        }


        public NormalisedGraph()
            :base()
        {
            NormalisationLaps = new List<int>();
            NormalisationTypeChanged += NormalisedGraph_NormalisationTypeChanged;
            NormalisationIndexChanged += NormalisedGraph_NormalisationIndexChanged;
            NormalisationZonesChanged += NormalisedGraph_NormalisationZonesChanged;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        protected override void SetupContextMenu()
        {
            base.SetupContextMenu();
            var button = new ToolStripButton("Normalise");
            button.Click += NormaliseCM_Click;
            this.ContextMenuStrip.Items.Add(button);
        }

        private void NormaliseCM_Click(object sender, EventArgs e)
        {
            EditNormalisationLaps(latestClickLocation);
            Invalidate();
        }

        private void EditNormalisationLaps(Point latestClickLocation)
        {
            if (latestClickLocation != null)
            {
                DataPoint clickData = GetPointData(latestClickLocation);
                int indexOfData = NormalisationLaps.FindIndex(l => l == (int)clickData.X);
                if (indexOfData != -1) //It is in the list
                {
                    if (indexOfData != NormalisationLaps.Count - 1) //It is not the last in the list
                        RemoveNormalisationPoint(indexOfData);
                }
                else
                {
                    AddNormalisationPoint((int)clickData.X);
                }
            }
        }

        private void AddNormalisationPoint(int valueToAdd)
        {
            NormalisationLaps.Add(valueToAdd);
            if (NormalisationZonesChanged != null)
                NormalisationZonesChanged(this, NormalisationLaps);
        }

        private void RemoveNormalisationPoint(int indexToRemoveAt)
        {
            NormalisationLaps.RemoveAt(indexToRemoveAt);
            if (NormalisationZonesChanged != null)
                NormalisationZonesChanged(this, NormalisationLaps);
        }

        private void NormalisedGraph_NormalisationZonesChanged(object sender, List<int> e)
        {
            NormalisationLaps.Sort();
            Invalidate();
        }

        void NormalisedGraph_NormalisationIndexChanged(object sender, int e)
        {
            Invalidate();
        }

        void NormalisedGraph_NormalisationTypeChanged(object sender, NormalisationType e)
        {
            Invalidate();
        }

        public override void SetupAxes()
        {
            base.SetupAxes();

            if (!AxesUserModified)
            {
                normalisationType = NormalisationType.None;
            }
        }

        public void SetNormalisationType(NormalisationType normalisationType)
        {
            this.normalisationType = normalisationType;
            AxesUserModified = true;
        }
        public void SetNormalisationIndex(int normalisationIndex)
        {
            NormalisationIndex = normalisationIndex;
        }

        public void NormaliseOnBestTrace()
        {
            var bestTrace = GetBestTrace();
            if (bestTrace != null)
            {
                int bestIndex = bestTrace.Index;
                SetNormalisationIndex(bestIndex);
            }
        }

        protected override GraphLine[] CalculateLines(List<GraphLine> tracesToCalculateFrom)
        {
            List<GraphLine> newTraces;
            traces = tracesToCalculateFrom;
            switch (NormalisationType)
            {
                case NormalisationType.OnAverageValue:
                    newTraces = GetNormalisedLinesOnAverageValue();
                    break;
                case NormalisationType.OnEveryValue:
                    newTraces = GetNormalisedLinesOnEveryValue();
                    break;
                default:
                    newTraces = null;
                    break;
            }
            return base.CalculateLines(newTraces);
        }

        private List<GraphLine> GetNormalisedLinesOnEveryValue()
        {
            GraphLine normalisationTrace = traces.Find(t => t.Index == NormalisationIndex);

            List<GraphLine> normalisedLines = new List<GraphLine>();
            List<DataPoint> normalisedPoints;
            DataPoint pointToNormalise;
            int pointIndex;
            double lastNormalisationValue = 0;
            for (int lineIndex = 0; lineIndex < traces.Count; lineIndex++)
            {
                normalisedPoints = new List<DataPoint>();
                pointIndex = 0;
                while (pointIndex < traces[lineIndex].DataPoints.Count)
                {
                    if (pointIndex < normalisationTrace.DataPoints.Count)
                        lastNormalisationValue = normalisationTrace.DataPoints[pointIndex].Y;
                    else
                        lastNormalisationValue += (lastNormalisationValue / pointIndex + 1); //Add the average value so far
                    pointToNormalise = traces[lineIndex].DataPoints[pointIndex];
                    normalisedPoints.Add(new DataPoint(pointToNormalise.X, pointToNormalise.Y - lastNormalisationValue, pointToNormalise.index, pointToNormalise.cycles));
                    pointIndex++;
                }
                    normalisedLines.Add(new GraphLine(normalisedPoints, traces[lineIndex].Index, traces[lineIndex].Show, traces[lineIndex].LineColour));
            }
            return normalisedLines;
        }

        private List<GraphLine> GetNormalisedLinesOnAverageValue()
        {
            GraphLine normalisationTrace = traces.Find(t => t.Index == NormalisationIndex);
            double[] normalisationParameters;
            List<GraphLine> normalisedLines = new List<GraphLine>();
            List<DataPoint> normalisedPoints;
            DataPoint pointToNormalise;
            int pointIndex;
            for (int lineIndex = 0; lineIndex < traces.Count; lineIndex++)
            {
                normalisedPoints = new List<DataPoint>();
                pointIndex = 0;
                while (pointIndex < traces[lineIndex].DataPoints.Count)
                {
                    normalisationParameters = GetAverageNormalisationValues(normalisationTrace, pointIndex);
                    pointToNormalise = traces[lineIndex].DataPoints[pointIndex];
                    normalisedPoints.Add(new DataPoint(pointToNormalise.X, pointToNormalise.Y - normalisationParameters[0]*pointIndex - normalisationParameters[1], pointToNormalise.index, pointToNormalise.cycles));
                    pointIndex++;
                }
                normalisedLines.Add(new GraphLine(normalisedPoints, traces[lineIndex].Index, traces[lineIndex].Show, traces[lineIndex].LineColour));
            }
            return normalisedLines;
        }

        protected override double GetYData(int xPosition, double yPosition)
        {
            double YData;
            GraphLine normalisationTrace = traces.Find(t => t.Index == NormalisationIndex);

            switch (NormalisationType)
            {
                case NormalisationType.OnAverageValue:
                    double[] normalisationParameters = GetAverageNormalisationValues(normalisationTrace, xPosition);
                    YData = yPosition + (xPosition * normalisationParameters[0]) + normalisationParameters[1];
                    break;
                case NormalisationType.OnEveryValue:
                    double offsetValue = normalisationTrace.DataPoints[xPosition].Y;
                    YData = yPosition + offsetValue;
                    break;
                default:
                    YData = base.GetYData(xPosition, yPosition);
                    break;
            }

            return YData;
        }

        /// <summary>
        /// Returns an array [m, c] of the average normalisation trace for the given lap
        /// </summary>
        /// <param name="normalisationTrace"></param>
        /// <param name="pointIndex"></param>
        /// <returns></returns>
        private double[] GetAverageNormalisationValues(GraphLine normalisationTrace, int pointIndex)
        {
            int endNormalisationX = normalisationLaps.Find(l => l >= pointIndex);
            int startNormalisationX = normalisationLaps.Find(l => l < pointIndex);
            if (startNormalisationX == -1) { startNormalisationX = 0; }
            double differenceY, averageValue, constant;
            // Get the normalising value
            // This is easy if the normalisation trace has points near the end
            if (endNormalisationX < normalisationTrace.DataPoints.Count)
            {
                differenceY = normalisationTrace.DataPoints[endNormalisationX].Y - normalisationTrace.DataPoints[startNormalisationX].Y;
                averageValue = differenceY / (endNormalisationX - startNormalisationX);
            }
            else //Do the best we can based on laps in this sector
            {
                differenceY = normalisationTrace.DataPoints.Last().Y - normalisationTrace.DataPoints[startNormalisationX].Y;
                averageValue = differenceY / (normalisationTrace.DataPoints.Count - startNormalisationX);
            }
            constant = normalisationTrace.DataPoints[startNormalisationX].Y - averageValue * startNormalisationX;
            return new double[] { averageValue, constant };
        }
    }
}
