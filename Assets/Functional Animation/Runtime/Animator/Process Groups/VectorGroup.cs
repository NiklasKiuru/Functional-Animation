using Unity.Burst;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    public abstract class VectorGroup<TStruct, TBaseType, TSubtype> : GroupBase<TStruct, TBaseType>
        where TStruct : unmanaged
        where TBaseType : unmanaged, IVectorInterpolator<TSubtype, TStruct>
        where TSubtype : unmanaged
    {
        protected VectorGroup(int preallocSize) : base(preallocSize)
        {
        }

        private readonly static FunctionPointer<InterpolationDelegate> s_main = BurstCompiler.CompileFunctionPointer<InterpolationDelegate>(Main);

        protected override FunctionPointer<InterpolationDelegate> MainFunction => s_main;
        protected override bool IsMultiGraphTarget => true;

        private static void Main(in TBaseType proc, in NativeFunctionGraph graph, ref ValueVector<TStruct> val, ref ExecutionContext ctx)
        {
            var startingPoint = 0;
            var time = ctx.Progress;
            for (int axis = 0; axis < proc.AxisCount; axis++)
            {
                if (proc.PointerCount(axis) == 0)
                    continue;
                
                var endingPoint = startingPoint + proc.PointerCount(axis);
                var func = graph.GetFunction(time, new int2(startingPoint, endingPoint));
                proc.SetValue(axis, proc.InterpolateAxis(val.Start, val.End, func, time, axis), ref val.Current);
                startingPoint = endingPoint;
            }
        }
    }
}
