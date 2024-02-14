using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    public partial struct EF
    {   
        /// <summary>
        /// Creates a basic transition interpolator
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="ease"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float> Create(float from, float to, float duration, Function ease, TimeControl ctrl = TimeControl.PlayOnce)
        {
            var processor = new FloatInterpolator
            {
                From = from,
                To = to,
                Clock = new Clock(1 / duration, ctrl),
                Length = 1
            };
            var func = new RangedFunction(ease);
            return EFAnimator.RegisterTarget<float, FloatInterpolator>(ref processor, func);
        }

        /// <summary>
        /// Creates a basic transition interpolator using ranged functions
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="funcs"></param>
        /// <param name="ctrl"></param>
        /// <returns>If given ranged function array is invalid, return null</returns>
        public static IInterpolatorHandle<float> Create(float from, float to, float duration, RangedFunction[] funcs, TimeControl ctrl = TimeControl.PlayOnce)
        {   
            if(funcs == null || funcs.Length == 0)
                return null;
            var processor = new FloatInterpolator
            {
                From = from,
                To = to,
                Clock = new Clock(1 / duration, ctrl),
                Length = funcs.Length
            };
            return EFAnimator.RegisterTarget<float, FloatInterpolator>(ref processor, funcs);
        }

        /// <summary>
        /// Creates a basic transition interpolator using graph data
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="data"></param>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float> Create(float from, float to, float duration, GraphData data, TimeControl ctrl = TimeControl.PlayOnce)
        {
            var funcs = data.GetRangedFunctionArray();
            return Create(from, to, duration, funcs, ctrl);
        }

        /// <summary>
        /// Creates a basic transition interpolator for 3D vector. Each axis will be calculated with the same easing function
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="ease"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float3> Create(float3 from, float3 to, float duration, Function ease, TimeControl ctrl = TimeControl.PlayOnce)
        {
            var processor = new Vector3Interpolator
            {
                From = from,
                To = to,
                Clock = new Clock(1 / duration, ctrl),
                Stride = new int3(1, 1, 1),
                AxisCheck = new bool3(true, true, true)
            };
            var funcX = new RangedFunction(ease);
            var funcY = new RangedFunction(ease);
            var funcZ = new RangedFunction(ease);
            return EFAnimator.RegisterTarget<float3, Vector3Interpolator>(ref processor, funcX, funcY, funcZ);
        }

        /// <summary>
        /// Creates a basic transition interpolator for 3D vector using ranged functions. Each axis will be calculated with the same easing function
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="funcs"></param>
        /// <param name="ctrl"></param>
        /// <returns>If given ranged function array is invalid, returns null</returns>
        public static IInterpolatorHandle<float3> Create(float3 from, float3 to, float duration, RangedFunction[] funcs, TimeControl ctrl = TimeControl.PlayOnce)
        {
            if (funcs == null || funcs.Length == 0)
                return null;
            var processor = new Vector3Interpolator
            {
                From = from,
                To = to,
                Clock = new Clock(1 / duration, ctrl),
                Stride = new int3(1, 1, 1),
                AxisCheck = new bool3(true, true, true)
            };
            return EFAnimator.RegisterTarget<float3, Vector3Interpolator>(ref processor, funcs);
        }

        /// <summary>
        /// Creates a basic transition interpolator for 3D vector using graph data. Each axis will be calculated with the same easing function
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="data"></param>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float3> Create(float3 from, float3 to, float duration, GraphData data, TimeControl ctrl = TimeControl.PlayOnce)
        {
            var funcs = data.GetRangedFunctionArray();
            return Create(from, to, duration, funcs, ctrl);
        }

        /// <summary>
        /// Creates an axis controlled transition interpolator for a 3D vector.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="dataX"></param>
        /// <param name="dataY"></param>
        /// <param name="dataZ"></param>
        /// <param name="axisControl"></param>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float3> Create(float3 from, float3 to, float duration, 
            GraphData dataX, GraphData dataY, GraphData dataZ, bool3 axisControl, TimeControl ctrl = TimeControl.PlayOnce)
        {   
            var funcsX = dataX.GetRangedFunctionArray();
            var funcsY = dataY.GetRangedFunctionArray();
            var funcsZ = dataZ.GetRangedFunctionArray();

            var processor = new Vector3Interpolator
            {
                From = from,
                To = to,
                Clock = new Clock(1 / duration, ctrl),
                Stride = new int3(funcsX.Length, funcsY.Length, funcsZ.Length),
                AxisCheck = axisControl
            };
            // **TEMP**
            return EFAnimator.RegisterTarget<float3, Vector3Interpolator>(ref processor, funcsX);
        }
    }
}

