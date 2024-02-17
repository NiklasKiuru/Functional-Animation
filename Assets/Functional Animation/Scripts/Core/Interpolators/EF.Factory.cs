using Codice.CM.Common;
using System.Collections.Generic;
using Unity.Mathematics;
using Aikom.FunctionalAnimation.Utility;
using UnityEngine;

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
                Stride = 1
            };
            var array = new RangedFunction[EFSettings.MaxFunctions];
            array[0] = new RangedFunction(ease);
            var func = new FunctionContainer(1, array);
            return EFAnimator.RegisterTarget<float, FloatInterpolator>(processor, func);
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
            var processor = new FloatInterpolator
            {
                From = from,
                To = to,
                Clock = new Clock(1 / duration, ctrl),
                Stride = 1
            };

            var array = new RangedFunction[EFSettings.MaxFunctions];
            array[0] = func;
            var funcs = new FunctionContainer(1, array);
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
            var processor = new FloatInterpolator
            {
                From = from,
                To = to,
                Clock = new Clock(1 / duration, ctrl),
                Stride = data.Length
            };

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
            var funcContainer = new FunctionContainer(3);
            funcContainer.Add(0, 0, new RangedFunction(ease));
            funcContainer.Add(1, 0, new RangedFunction(ease));
            funcContainer.Add(2, 0, new RangedFunction(ease));
            return EFAnimator.RegisterTarget<float3, Vector3Interpolator>(processor, funcContainer);
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

            var funcContainer = new FunctionContainer(3);

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
            if(axisControl.x && dataX == null) throw new System.NullReferenceException();
            if(axisControl.y && dataY == null) throw new System.NullReferenceException();
            if(axisControl.z && dataZ == null) throw new System.NullReferenceException();

            var lenX = axisControl.x ? dataX.Length : 0;
            var lenY = axisControl.y ? dataY.Length : 0;
            var lenZ = axisControl.z ? dataZ.Length : 0;

            var arr = new RangedFunction[lenX + lenY + lenZ];

            if (axisControl.x) dataX.GetRangedFunctionsNonAlloc(ref arr);
            if (axisControl.y) dataY.GetRangedFunctionsNonAlloc (ref arr, lenX);
            if (axisControl.z) dataZ.GetRangedFunctionsNonAlloc(ref arr, lenX + lenY);

            var processor = new Vector3Interpolator
            {
                From = from,
                To = to,
                Clock = new Clock(1 / duration, ctrl),
                Stride = new int3(lenX, lenY, lenZ),
                AxisCheck = axisControl
            };
            return EFAnimator.RegisterTarget<float3, Vector3Interpolator>(processor, null);
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
            var list = new List<RangedFunction>();
            for(int i = 0; i < 3; i++)
            {
                if (axisControl[i])
                    list.Add(new RangedFunction(funcs[i]));
            }
            var processor = new Vector3Interpolator
            {
                From = from,
                To = to,
                Clock = new Clock(1 / duration, ctrl),
                Stride = new int3(axisControl.x ? 1 : 0, axisControl.y ? 1 : 0, axisControl.z ? 1 : 0),
                AxisCheck = axisControl
            };
            return EFAnimator.RegisterTarget<float3, Vector3Interpolator>(processor, null);
        }
    }
}

