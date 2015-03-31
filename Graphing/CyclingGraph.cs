using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphing
{
    public class CyclingGraph : NormalisedGraph
    {
        GraphLine cycleValue;
        public event EventHandler<GraphLine> CycleValueChanged;
        public GraphLine CycleValue
        {
            get { return cycleValue; }
            set
            {
                cycleValue = value;
                if (CycleValueChanged != null)
                    CycleValueChanged(this, value);
            }
        }

        bool cycleGraph;
        public event EventHandler<bool> CycleChanged;
        public bool CycleGraph
        {
            get { return cycleGraph; }
            set
            {
                cycleGraph = value;
                if (CycleChanged != null)
                    CycleChanged(this, value);
            }
        }

        public CyclingGraph()
            : base()
        {
            cycleGraph = true;
            CycleChanged += CyclingGraph_CycleChanged;
            CycleValueChanged += CyclingGraph_CycleValueChanged;
        }

        void CyclingGraph_CycleChanged(object sender, bool e)
        {
            Invalidate();
        }

        void CyclingGraph_CycleValueChanged(object sender, GraphLine e)
        {
            Invalidate();
        }

        protected override GraphLine[] CalculateLines(List<GraphLine> tracesToCalculateFrom)
        {
            var newTraces = tracesToCalculateFrom;
            if (cycleGraph)
            {
                newTraces = GetCycledLines();
            }

            return base.CalculateLines(newTraces);
        }

        private List<GraphLine> GetCycledLines()
        {
            //The traces are still in data form; they have not yet been normalised.
            //For now, we assume that the traces are cumulative. If they are not then the mathematics becomes somewhat simpler.
            //We also assume that we want to cycle based on the trace value, rather than a constant. This can be altered later too.
            //The upper and lower cycling limits could be based on a different calculation (e.g. lap ahead, lap behind calculations,
            // or even the difference between the line ahead and the line behind being a percentage of the line ahead.
            //TODO: work out what sort of model accurately represents line cycling.

            //Use the normalisation trace for cycling on.
            GraphLine cycleLine = traces.Find(l => l.Index == NormalisationIndex);

            List<GraphLine> newTraces = new List<GraphLine>();
            List<DataPoint> cycledPoints;
            DataPoint pointToAlter;
            double cycleLimit;
            bool lineCycled;
            int pointIndex;
            for (int lineIndex = 0; lineIndex < traces.Count; lineIndex++)
            {
                pointIndex = 0;
                cycledPoints = new List<DataPoint>();
                while (pointIndex < traces[lineIndex].DataPoints.Count && pointIndex < cycleLine.DataPoints.Count)
                {
                    lineCycled = false;

                    pointToAlter = traces[lineIndex].DataPoints[pointIndex];
                    //Cycling is evaluated on every point.
                    if (pointIndex > 0) //The first points are never cycled
                    {
                        //The limit is given by the most recent lap time of the normalisation trace.
                        cycleLimit = (cycleLine.DataPoints[pointIndex].Y - cycleLine.DataPoints[pointIndex - 1].Y) / 2;
                        
                        //Continue to cycle either up or down until it is within bounds.
                        while (pointToAlter.Y > cycleLine.DataPoints[pointIndex].Y + cycleLimit || pointToAlter.Y < cycleLine.DataPoints[pointIndex].Y - cycleLimit)
                        {
                            if (pointToAlter.Y > cycleLine.DataPoints[pointIndex].Y + cycleLimit)
                            {
                                //Getting too far ahead so cycle down
                                pointToAlter.Y -= cycleLimit * 2;
                                lineCycled = true;
                            }
                            else if (pointToAlter.Y < cycleLine.DataPoints[pointIndex].Y - cycleLimit)
                            {
                                pointToAlter.Y += cycleLimit * 2;
                                lineCycled = true;
                            }
                        }
                    }

                    cycledPoints.Add(new DataPoint(pointToAlter.X, pointToAlter.Y, pointToAlter.index, lineCycled));
                    pointIndex++;
                }

                newTraces.Add(new GraphLine(cycledPoints, traces[lineIndex].Index, traces[lineIndex].Show, traces[lineIndex].LineColour));
            }
            
            return newTraces;
        }
    }
}
