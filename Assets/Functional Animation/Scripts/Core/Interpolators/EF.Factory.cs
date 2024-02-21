using Unity.Mathematics;
using Aikom.FunctionalAnimation.Utility;
using System;

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
            return Create(from, to, duration, new RangedFunction(ease), ctrl);
        }

        /// <summary>
        /// Creates a basic transition interpolator using a ranged function
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="funcs"></param>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float> Create(float from, float to, float duration, RangedFunction func, TimeControl ctrl = TimeControl.PlayOnce)
        {
            var processor = CreateBasic(from, to, duration, ctrl);
            var funcs = new FunctionContainer(1);
            funcs.Set(0, 0, func);
            return EFAnimator.RegisterTarget<float, FloatInterpolator>(processor, funcs);
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
            var processor = CreateBasic(from, to, duration, ctrl, data.Length);
            var array = data.GetRangedFunctionArray();
            var funcs = new FunctionContainer(1, array);
            return EFAnimator.RegisterTarget<float, FloatInterpolator>(processor, funcs);
        }

        /// <summary>
        /// Creates a basic transition interpolator for 3D vector. Each axis will be calculated with the same easing function
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="ease"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float2> Create(float2 from, float2 to, float duration, Function ease, TimeControl ctrl = TimeControl.PlayOnce)
        {
            return Create(from, to, duration, new RangedFunction(ease), ctrl);
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
        public static IInterpolatorHandle<float2> Create(float2 from, float2 to, float duration, RangedFunction func, TimeControl ctrl = TimeControl.PlayOnce)
        {
            var processor = CreateBasic(from, to, duration, ctrl, new int2(1, 1), new bool2(true, true));
            var funcContainer = new FunctionContainer(3);
            funcContainer.Set(0, 0, func);
            funcContainer.Set(1, 0, func);
            return EFAnimator.RegisterTarget<float2, Vector2Interpolator>(processor, funcContainer);
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
        public static IInterpolatorHandle<float2> Create(float2 from, float2 to, float duration, GraphData data, TimeControl ctrl = TimeControl.PlayOnce)
        {
            if (data == null)
                throw new System.ArgumentNullException();
            var stride = new int2(data.Length, data.Length);
            var processor = CreateBasic(from, to, duration, ctrl, stride, new bool2(true, true));
            var funcs = data.GetRangedFunctionArray();
            var cont = new FunctionContainer(funcs, stride.x, stride.y);
            return EFAnimator.RegisterTarget<float2, Vector2Interpolator>(processor, cont);
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
        public static IInterpolatorHandle<float2> Create(float2 from, float2 to, float duration,
            GraphData dataX, GraphData dataY, bool2 axisControl, TimeControl ctrl = TimeControl.PlayOnce)
        {
            if (axisControl.x && dataX == null) throw new System.NullReferenceException();
            if (axisControl.y && dataY == null) throw new System.NullReferenceException();

            var stride = new int2(axisControl.x ? dataX.Length : 0, axisControl.y ? dataY.Length : 0);
            var arr = new RangedFunction[2 * EFSettings.MaxFunctions];

            if (axisControl.x) dataX.GetRangedFunctionsNonAlloc(ref arr);
            if (axisControl.y) dataY.GetRangedFunctionsNonAlloc(ref arr, EFSettings.MaxFunctions);

            var processor = CreateBasic(from, to, duration, ctrl, stride, axisControl);
            var cont = new FunctionContainer(2, arr);
            return EFAnimator.RegisterTarget<float2, Vector2Interpolator>(processor, cont);
        }

        /// <summary>
        /// Creates an axis controlled transition interpolator for a 3D vector with one function for each axis.
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
        public static IInterpolatorHandle<float2> Create(float2 from, float2 to, float duration,
            Func2 funcs, bool2 axisControl, TimeControl ctrl = TimeControl.PlayOnce)
        {
            var cont = new FunctionContainer(2);
            if (axisControl.x) cont.Set(0, 0, new RangedFunction(funcs.X));
            if (axisControl.y) cont.Set(1, 0, new RangedFunction(funcs.Y));
            var processor = CreateBasic(from, to, duration, ctrl, new int2(axisControl.x ? 1 : 0, axisControl.y ? 1 : 0), axisControl);
            return EFAnimator.RegisterTarget<float2, Vector2Interpolator>(processor, cont);
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
            return Create(from, to, duration, new RangedFunction(ease), ctrl);
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
        public static IInterpolatorHandle<float3> Create(float3 from, float3 to, float duration, RangedFunction func, TimeControl ctrl = TimeControl.PlayOnce)
        {
            var processor = CreateBasic(from, to, duration, ctrl, new int3(1, 1, 1), new bool3(true, true, true));
            var funcContainer = new FunctionContainer(3);
            funcContainer.Set(0, 0, func);
            funcContainer.Set(1, 0, func);
            funcContainer.Set(2, 0, func);
            return EFAnimator.RegisterTarget<float3, Vector3Interpolator>(processor, funcContainer);
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
            if(data == null)
                throw new System.ArgumentNullException();
            var stride = new int3(data.Length, data.Length, data.Length);
            var processor = CreateBasic(from, to, duration, ctrl, stride, new bool3(true, true, true));
            var funcs = data.GetRangedFunctionArray();
            var cont = new FunctionContainer(funcs, stride.x, stride.y, stride.z);
            return EFAnimator.RegisterTarget<float3, Vector3Interpolator>(processor, cont);
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
            if(axisControl.x && dataX == null) throw new System.NullReferenceException();
            if(axisControl.y && dataY == null) throw new System.NullReferenceException();
            if(axisControl.z && dataZ == null) throw new System.NullReferenceException();

            var stride = new int3(axisControl.x ? dataX.Length : 0, axisControl.y ? dataY.Length : 0, axisControl.z ? dataZ.Length : 0);
            var arr = new RangedFunction[3 * EFSettings.MaxFunctions];

            if (axisControl.x) dataX.GetRangedFunctionsNonAlloc(ref arr);
            if (axisControl.y) dataY.GetRangedFunctionsNonAlloc (ref arr, EFSettings.MaxFunctions);
            if (axisControl.z) dataZ.GetRangedFunctionsNonAlloc(ref arr, 2 * EFSettings.MaxFunctions);

            var processor = CreateBasic(from, to, duration, ctrl, stride, axisControl);
            var cont = new FunctionContainer(3, arr);
            return EFAnimator.RegisterTarget<float3, Vector3Interpolator>(processor, cont);
        }

        /// <summary>
        /// Creates an axis controlled transition interpolator for a 3D vector with one function for each axis.
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
            Func3 funcs, bool3 axisControl, TimeControl ctrl = TimeControl.PlayOnce)
        {
            var cont = new FunctionContainer(3);
            if (axisControl.x) cont.Set(0, 0, new RangedFunction(funcs.X));
            if (axisControl.y) cont.Set(1, 0, new RangedFunction(funcs.Y));
            if (axisControl.z) cont.Set(2, 0, new RangedFunction(funcs.Z));
            var processor = CreateBasic(from, to, duration, ctrl, new int3(axisControl.x ? 1 : 0, axisControl.y ? 1 : 0, axisControl.z ? 1 : 0), axisControl);
            return EFAnimator.RegisterTarget<float3, Vector3Interpolator>(processor, cont);
        }

        //

        /// <summary>
        /// Creates a basic transition interpolator for 3D vector. Each axis will be calculated with the same easing function
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="ease"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<float4> Create(float4 from, float4 to, float duration, Function ease, TimeControl ctrl = TimeControl.PlayOnce)
        {
            return Create(from, to, duration, new RangedFunction(ease), ctrl);
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
        public static IInterpolatorHandle<float4> Create(float4 from, float4 to, float duration, RangedFunction func, TimeControl ctrl = TimeControl.PlayOnce)
        {
            var processor = CreateBasic(from, to, duration, ctrl, new int4(1, 1, 1, 1), new bool4(true, true, true, true));
            var funcContainer = new FunctionContainer(4);
            funcContainer.Set(0, 0, func);
            funcContainer.Set(1, 0, func);
            funcContainer.Set(2, 0, func);
            funcContainer.Set(3, 0, func);
            return EFAnimator.RegisterTarget<float4, Vector4Interpolator>(processor, funcContainer);
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
        public static IInterpolatorHandle<float4> Create(float4 from, float4 to, float duration, GraphData data, TimeControl ctrl = TimeControl.PlayOnce)
        {
            if (data == null)
                throw new System.ArgumentNullException();
            var stride = new int4(data.Length, data.Length, data.Length, data.Length);
            var processor = CreateBasic(from, to, duration, ctrl, stride, new bool4(true, true, true, true));
            var funcs = data.GetRangedFunctionArray();
            var cont = new FunctionContainer(funcs, stride.x, stride.y, stride.z, stride.w);
            return EFAnimator.RegisterTarget<float4, Vector4Interpolator>(processor, cont);
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
        public static IInterpolatorHandle<float4> Create(float4 from, float4 to, float duration,
            GraphData dataX, GraphData dataY, GraphData dataZ, GraphData dataW, bool4 axisControl, TimeControl ctrl = TimeControl.PlayOnce)
        {
            if (axisControl.x && dataX == null) throw new System.NullReferenceException();
            if (axisControl.y && dataY == null) throw new System.NullReferenceException();
            if (axisControl.z && dataZ == null) throw new System.NullReferenceException();
            if (axisControl.w && dataW == null) throw new System.NullReferenceException();

            var stride = new int4(axisControl.x ? dataX.Length : 0, 
                axisControl.y ? dataY.Length : 0, 
                axisControl.z ? dataZ.Length : 0, 
                axisControl.w ? dataW.Length : 0);
            var arr = new RangedFunction[4 * EFSettings.MaxFunctions];

            if (axisControl.x) dataX.GetRangedFunctionsNonAlloc(ref arr);
            if (axisControl.y) dataY.GetRangedFunctionsNonAlloc(ref arr, EFSettings.MaxFunctions);
            if (axisControl.z) dataZ.GetRangedFunctionsNonAlloc(ref arr, 2 * EFSettings.MaxFunctions);
            if (axisControl.w) dataW.GetRangedFunctionsNonAlloc(ref arr, 3 * EFSettings.MaxFunctions);

            var processor = CreateBasic(from, to, duration, ctrl, stride, axisControl);
            var cont = new FunctionContainer(4, arr);
            return EFAnimator.RegisterTarget<float4, Vector4Interpolator>(processor, cont);
        }

        /// <summary>
        /// Creates an axis controlled transition interpolator for a 3D vector with one function for each axis.
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
        public static IInterpolatorHandle<float4> Create(float4 from, float4 to, float duration,
            Func4 funcs, bool4 axisControl, TimeControl ctrl = TimeControl.PlayOnce)
        {
            var cont = new FunctionContainer(4);
            if (axisControl.x) cont.Set(0, 0, new RangedFunction(funcs.X));
            if (axisControl.y) cont.Set(1, 0, new RangedFunction(funcs.Y));
            if (axisControl.z) cont.Set(2, 0, new RangedFunction(funcs.Z));
            if (axisControl.w) cont.Set(3, 0, new RangedFunction(funcs.W));
            var processor = CreateBasic(from, to, duration, ctrl, new int4(axisControl.x ? 1 : 0, 
                axisControl.y ? 1 : 0, axisControl.z ? 1 : 0, axisControl.w ? 1 : 0), axisControl);
            return EFAnimator.RegisterTarget<float4, Vector4Interpolator>(processor, cont);
        }

        #region NonAlloc

        /// <summary>
        /// Creates an uncontrollable single use Interpolator instance
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="func"></param>
        /// <param name="ctrl"></param>
        /// <param name="maxLoopCount"></param>
        /// <returns>Process id</returns>
        public static ProcessId CreateNonAlloc(float from, float to, float duration, Function func, TimeControl ctrl, int maxLoopCount)
        {
            using var container = new FunctionContainer(1);
            container.Set(0, 0, new RangedFunction(func));
            var processor = CreateBasic(from, to, duration, ctrl);
            processor = (FloatInterpolator)processor.SetMaxLoopCount(maxLoopCount);
            return EFAnimator.RegisterTargetNonAlloc<float, FloatInterpolator>(ref processor, container);
        }

        /// <summary>
        /// Creates an uncontrollable single use Interpolator instance
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="func"></param>
        /// <param name="onUpdate"></param>
        /// <returns></returns>
        public static ProcessId CreateNonAlloc(float from, float to, float duration, Function func, TimeControl ctrl, int maxLoopCount, Action<float> onUpdate)
        {
            var proc = CreateNonAlloc(from, to, duration, func, ctrl, maxLoopCount);
            EFAnimator.SetPassiveFlagsInternal(proc, EventFlags.OnUpdate);
            CallbackRegistry.RegisterCallback(proc.Id, onUpdate, EventFlags.OnUpdate);
            return proc;
        }

        /// <summary>
        /// Creates an uncontrollable single use Interpolator instance
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="func"></param>
        /// <param name="ctrl"></param>
        /// <param name="maxLoopCount"></param>
        /// <returns>Process id</returns>
        public static ProcessId CreateNonAlloc(float2 from, float2 to, float duration, Function func, TimeControl ctrl, int maxLoopCount)
        {
            using var container = new FunctionContainer(2);
            container.Set(0, 0, new RangedFunction(func));
            container.Set(1, 0, new RangedFunction(func));
            var processor = CreateBasic(from, to, duration, ctrl, new int2(1,1), new bool2(true, true));
            processor = (Vector2Interpolator)processor.SetMaxLoopCount(maxLoopCount);
            return EFAnimator.RegisterTargetNonAlloc<float2, Vector2Interpolator>(ref processor, container);
        }

        /// <summary>
        /// Creates an uncontrollable single use Interpolator instance
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="func"></param>
        /// <param name="onUpdate"></param>
        /// <returns></returns>
        public static ProcessId CreateNonAlloc(float2 from, float2 to, float duration, Function func, TimeControl ctrl, int maxLoopCount, Action<float> onUpdate)
        {
            var proc = CreateNonAlloc(from, to, duration, func, ctrl, maxLoopCount);
            EFAnimator.SetPassiveFlagsInternal(proc, EventFlags.OnUpdate);
            CallbackRegistry.RegisterCallback(proc.Id, onUpdate, EventFlags.OnUpdate);
            return proc;
        }

        /// <summary>
        /// Creates an uncontrollable single use Interpolator instance
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="func"></param>
        /// <param name="ctrl"></param>
        /// <param name="maxLoopCount"></param>
        /// <returns>Process id</returns>
        public static ProcessId CreateNonAlloc(float3 from, float3 to, float duration, Function func, TimeControl ctrl, int maxLoopCount)
        {
            using var container = new FunctionContainer(3);
            container.Set(0, 0, new RangedFunction(func));
            container.Set(1, 0, new RangedFunction(func));
            container.Set(2, 0, new RangedFunction(func));
            var processor = CreateBasic(from, to, duration, ctrl, new int3(1, 1, 1), new bool3(true, true, true));
            processor = (Vector3Interpolator)processor.SetMaxLoopCount(maxLoopCount);
            return EFAnimator.RegisterTargetNonAlloc<float3, Vector3Interpolator>(ref processor, container);
        }

        /// <summary>
        /// Creates an uncontrollable single use Interpolator instance
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="func"></param>
        /// <param name="onUpdate"></param>
        /// <returns></returns>
        public static ProcessId CreateNonAlloc(float3 from, float3 to, float duration, Function func, TimeControl ctrl, int maxLoopCount, Action<float> onUpdate)
        {
            var proc = CreateNonAlloc(from, to, duration, func, ctrl, maxLoopCount);
            EFAnimator.SetPassiveFlagsInternal(proc, EventFlags.OnUpdate);
            CallbackRegistry.RegisterCallback(proc.Id, onUpdate, EventFlags.OnUpdate);
            return proc;
        }


        /// <summary>
        /// Creates an uncontrollable single use Interpolator instance
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="func"></param>
        /// <param name="ctrl"></param>
        /// <param name="maxLoopCount"></param>
        /// <returns>Process id</returns>
        public static ProcessId CreateNonAlloc(float4 from, float4 to, float duration, Function func, TimeControl ctrl, int maxLoopCount)
        {
            using var container = new FunctionContainer(4);
            container.Set(0, 0, new RangedFunction(func));
            container.Set(1, 0, new RangedFunction(func));
            container.Set(2, 0, new RangedFunction(func));
            container.Set(3, 0, new RangedFunction(func));
            var processor = CreateBasic(from, to, duration, ctrl, new int4(1, 1, 1, 1), new bool4(true, true, true, true));
            processor = (Vector4Interpolator)processor.SetMaxLoopCount(maxLoopCount);
            return EFAnimator.RegisterTargetNonAlloc<float4, Vector4Interpolator>(ref processor, container);
        }

        /// <summary>
        /// Creates an uncontrollable single use Interpolator instance
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="func"></param>
        /// <param name="onUpdate"></param>
        /// <returns></returns>
        public static ProcessId CreateNonAlloc(float4 from, float4 to, float duration, Function func, TimeControl ctrl, int maxLoopCount, Action<float> onUpdate)
        {
            var proc = CreateNonAlloc(from, to, duration, func, ctrl, maxLoopCount);
            EFAnimator.SetPassiveFlagsInternal(proc, EventFlags.OnUpdate);
            CallbackRegistry.RegisterCallback(proc.Id, onUpdate, EventFlags.OnUpdate);
            return proc;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Creates a basic float interpolator
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="ctrl"></param>
        /// <returns></returns>
        private static FloatInterpolator CreateBasic(float from, float to, float duration, TimeControl ctrl, int stride = 1)
        {
            return new FloatInterpolator
            {
                From = from,
                To = to,
                Clock = new Clock(1 / duration, ctrl),
                Stride = stride,
            };
        }

        /// <summary>
        /// Creates a basic vec2 interpolator
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="ctrl"></param>
        /// <param name="stride"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        private static Vector2Interpolator CreateBasic(float2 from, float2 to, float duration, TimeControl ctrl, int2 stride, bool2 axis)
        {
            return new Vector2Interpolator
            {
                From = from,
                To = to,
                Clock = new Clock(1/duration, ctrl),
                Stride = stride,
                AxisCheck = axis
            };
        }

        /// <summary>
        /// Creates a basic vec2 interpolator
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="ctrl"></param>
        /// <param name="stride"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        private static Vector3Interpolator CreateBasic(float3 from, float3 to, float duration, TimeControl ctrl, int3 stride, bool3 axis)
        {
            return new Vector3Interpolator
            {
                From = from,
                To = to,
                Clock = new Clock(1 / duration, ctrl),
                Stride = stride,
                AxisCheck = axis
            };
        }

        /// <summary>
        /// Creates a basic vec2 interpolator
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <param name="ctrl"></param>
        /// <param name="stride"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        private static Vector4Interpolator CreateBasic(float4 from, float4 to, float duration, TimeControl ctrl, int4 stride, bool4 axis)
        {
            return new Vector4Interpolator
            {
                From = from,
                To = to,
                Clock = new Clock(1 / duration, ctrl),
                Stride = stride,
                AxisCheck = axis
            };
        }


        #endregion
    }
}

