using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aikom.FunctionalAnimation.Utility
{
    public static class GraphUtility
    {
        public static void AlignAndCopyMultiGraphData<T, D, C>(ref Span<RangedFunction> data, C processor, params GraphData[] managedGraphs)
            where T : unmanaged
            where D : unmanaged
            where C : IVectorInterpolator<T, D>
        {
            var startingPoint = 0;
            var endingPoint = 0;
            for(int i = 0; i < processor.AxisCount; i++)
            {
                var graph = managedGraphs[i];
                if (processor.PointerCount(i) != 0 && graph != null)
                {
                    endingPoint += processor.PointerCount(i);
                    Span<RangedFunction> locData = stackalloc RangedFunction[graph.Length];
                    managedGraphs[i].CopyData(ref locData);
                    for(int j = startingPoint; j < endingPoint; j++)
                    {
                        data[j] = locData[j - startingPoint];
                    }
                    startingPoint = endingPoint;
                }
            }
        }

        public static int CalculateTotalMultiGraphLength<T, D, C>(C processor, params GraphData[] managedGraphs)
            where T : unmanaged
            where D : unmanaged
            where C : IVectorInterpolator<T, D>
        {

            var totalLength = 0;
            for (int i = 0; i < processor.AxisCount; i++)
            {
                var graph = managedGraphs[i];
                if (graph == null)
                {
                    if (processor.PointerCount(i) != 0)
                        throw new ArgumentException("Inconsistent axis validation. Given graph cannot be null if the corresponding axis is valid");
                    continue;
                }
                    
                totalLength += processor.PointerCount(i) != 0 ? graph.Length : 0;
            }
            return totalLength;
        }
    }
}
