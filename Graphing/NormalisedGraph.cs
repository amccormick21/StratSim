using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        public NormalisedGraph()
            :base()
        {
            NormalisationTypeChanged += NormalisedGraph_NormalisationTypeChanged;
            NormalisationIndexChanged += NormalisedGraph_NormalisationIndexChanged;
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
            for (int lineIndex = 0; lineIndex < traces.Count; lineIndex++)
            {
                normalisedPoints = new List<DataPoint>();
                pointIndex = 0;
                while (pointIndex < traces[lineIndex].DataPoints.Count && pointIndex < normalisationTrace.DataPoints.Count)
                {
                    pointToNormalise = traces[lineIndex].DataPoints[pointIndex];
                    normalisedPoints.Add(new DataPoint(pointToNormalise.X, pointToNormalise.Y - normalisationTrace.DataPoints[pointIndex].Y, pointToNormalise.index, pointToNormalise.isCycled));
                    pointIndex++;
                }
                    normalisedLines.Add(new GraphLine(normalisedPoints, traces[lineIndex].Index, traces[lineIndex].Show, traces[lineIndex].LineColour));
            }
            return normalisedLines;
        }

        private List<GraphLine> GetNormalisedLinesOnAverageValue()
        {
            GraphLine normalisationTrace = traces.Find(t => t.Index == NormalisationIndex);
            double finalValue = normalisationTrace.DataPoints[normalisationTrace.DataPoints.Count - 1].Y;
            double averageValue = finalValue / (normalisationTrace.DataPoints.Count-1);

            List<GraphLine> normalisedLines = new List<GraphLine>();
            List<DataPoint> normalisedPoints;
            DataPoint pointToNormalise;
            int pointIndex;
            for (int lineIndex = 0; lineIndex < traces.Count; lineIndex++)
            {
                normalisedPoints = new List<DataPoint>();
                pointIndex = 0;
                while (pointIndex < traces[lineIndex].DataPoints.Count && pointIndex < normalisationTrace.DataPoints.Count)
                {
                    pointToNormalise = traces[lineIndex].DataPoints[pointIndex];
                    normalisedPoints.Add(new DataPoint(pointToNormalise.X, pointToNormalise.Y - averageValue*pointIndex, pointToNormalise.index, pointToNormalise.isCycled));
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
                case Graphing.NormalisationType.OnAverageValue:
                    double finalValue = normalisationTrace.DataPoints[normalisationTrace.DataPoints.Count - 1].Y;
                    double averageValue = finalValue / (normalisationTrace.DataPoints.Count-1);
                    YData = yPosition + (xPosition * averageValue);
                    break;
                case Graphing.NormalisationType.OnEveryValue:
                    double offsetValue = normalisationTrace.DataPoints[xPosition].Y;
                    YData = yPosition + offsetValue;
                    break;
                default:
                    YData = base.GetYData(xPosition, yPosition);
                    break;
            }

            return YData;
        }
    }
}
