using Codice.Client.BaseCommands.Filters;
using System;
using System.ComponentModel;

namespace Aikom.FunctionalAnimation
{
    /// <summary>
    /// Public interface to control interpolator processes
    /// </summary>
    /// <typeparam name="TStruct"></typeparam>
    public interface IInterpolatorHandle<TStruct, TProcessor>
        where TStruct : unmanaged
        where TProcessor : unmanaged, IInterpolator<TStruct>
    {
        public Process ProcessId { get; internal set; }
    }

    public static class HandleExtensions
    {   
        public static bool IsAlive<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            return ProcessCache.CheckValidity(handle.ProcessId) && ProcessCache.GetContext(handle.ProcessId).Status == ExecutionStatus.Running;
        }

        /// <summary>
        /// Registers callbacks for this handle
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <param name="handle"></param>
        /// <param name="cb"></param>
        /// <param name="flags"></param>
        /// <remarks>All callbacks are automatically void once the process dies so there is no need for unregistering</remarks>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> RegisterCallback<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle, Action<TStruct> cb, EventFlags flags)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            if(cb != null && flags != EventFlags.None)
                EFAnimator.RegisterStaticCallback(handle, cb, flags);
            return handle;
        }

        /// <summary>
        /// Registers callbacks for this handle
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <param name="handle"></param>
        /// <param name="cb"></param>
        /// <param name="flags"></param>
        /// <remarks>All callbacks are automatically void once the process dies so there is no need for unregistering</remarks>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> RegisterCallback<TStruct, TProcessor, TObject>(this IInterpolatorHandle<TStruct, TProcessor> handle, TObject owner, Action<TStruct> cb, EventFlags flags)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
            where TObject : UnityEngine.Object
        {
            if (cb != null && flags != EventFlags.None && owner != null)
                EFAnimator.RegisterInstancedCallback(handle, owner, cb, flags);
            return handle;
        }

        /// <summary>
        /// Registers a callback that is invoked once the process has completed succesfully
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="handle"></param>
        /// <param name="owner"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> OnComplete<TStruct, TProcessor, TObject>(this IInterpolatorHandle<TStruct, TProcessor> handle, TObject owner, Action<TStruct> callback)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
            where TObject : UnityEngine.Object
            => RegisterCallback(handle, owner, callback, EventFlags.OnComplete);
        
        /// <summary>
        /// Registers a callback that is invoked once the process has completed succesfully
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> OnComplete<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle, Action<TStruct> callback)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
            => RegisterCallback(handle, callback, EventFlags.OnComplete);

        /// <summary>
        /// Restarts the process. This can only be used on allocated processes
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> Restart<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            // Process exists
            if (handle.IsAlive())
            {

            }
            return handle;
        }

        /// <summary>
        /// Inverts the direction of the current process
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> Invert<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle) 
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            ref var clock = ref ProcessCache.GetClock(handle.ProcessId);
            clock.InvertDirection();
            return handle;
        }

        /// <summary>
        /// Flips the values of interpolation process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> FlipValues<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle) 
            where TStruct : unmanaged 
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            ref var start = ref ProcessCache.GetStart<TStruct, TProcessor>(handle.ProcessId);
            ref var end = ref ProcessCache.GetStart<TStruct, TProcessor>(handle.ProcessId);

            var temp0 = start;
            var temp1 = end;

            start = temp1;
            end = temp0;

            return handle;
        }

        /// <summary>
        /// Flips the values of interpolation process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static TStruct GetValue<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            return ProcessCache.GetCurrent<TStruct, TProcessor>(handle.ProcessId);
        }

        /// <summary>
        /// Sets the current process as inactive for the given delay and continues after the delay has passed.
        /// Does not call OnPause or OnResume
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <param name="handle"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> Hibernate<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle, float delay)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            // Disable this handle internally temporarily
            ref var ctx = ref ProcessCache.GetContext(handle.ProcessId);
            ctx.Status = ExecutionStatus.Inactive;

            // This is guaranteed to die once delay has been reached
            var delayHandle = EF.Create(0f, 1f, new FloatInterpolator(), delay);

            // Sets a callback to set the status of this handle back to running state once the earlier process dies
            EFAnimator.RegisterStaticCallback<TStruct>(delayHandle.ProcessId, SetStatus, EventFlags.OnKill);
            return handle;

            void SetStatus(TStruct val) 
            {
                ref var ctx = ref ProcessCache.GetContext(handle.ProcessId);
                ctx.Status = ExecutionStatus.Running;
            };
        }

        /// <summary>
        /// Registers a callback that is invoked once the process has been killed
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="handle"></param>
        /// <param name="owner"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> OnKill<TStruct, TProcessor, TObject>(this IInterpolatorHandle<TStruct, TProcessor> handle, TObject owner, Action<TStruct> callback) 
            where TStruct : unmanaged
            where TObject : UnityEngine.Object
            where TProcessor : unmanaged, IInterpolator<TStruct>
            => RegisterCallback(handle, owner, callback, EventFlags.OnKill);

        /// <summary>
        /// Registers a callback that is invoked once the process has been killed
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="handle"></param>
        /// <param name="owner"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> OnKill<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle, Action<TStruct> callback)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
            => RegisterCallback(handle, callback, EventFlags.OnKill);

        /// <summary>
        /// Registers a callback that is invoked once the process resumes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="handle"></param>
        /// <param name="owner"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> OnResume<TStruct, TProcessor, TObject>(this IInterpolatorHandle<TStruct, TProcessor> handle, TObject owner, Action<TStruct> callback)
            where TStruct : unmanaged
            where TObject : UnityEngine.Object
            where TProcessor : unmanaged, IInterpolator<TStruct>
            => RegisterCallback(handle, owner, callback, EventFlags.OnResume);

        /// <summary>
        /// Registers a callback that is invoked once the process resumes
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <param name="handle"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> OnResume<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle, Action<TStruct> callback)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
            => RegisterCallback(handle, callback, EventFlags.OnResume);

        /// <summary>
        /// Registers a callback that is invoked once the process starts for the first time
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="handle"></param>
        /// <param name="owner"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> OnStart<TStruct, TProcessor, TObject>(this IInterpolatorHandle<TStruct, TProcessor> handle, TObject owner, Action<TStruct> callback)
            where TStruct : unmanaged
            where TObject : UnityEngine.Object
            where TProcessor : unmanaged, IInterpolator<TStruct>
            => RegisterCallback(handle, owner, callback, EventFlags.OnStart);

        /// <summary>
        /// Registers a callback that is invoked once the process starts for the first time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> OnStart<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle, Action<TStruct> callback)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
            => RegisterCallback(handle, callback, EventFlags.OnStart);

        /// <summary>
        /// Registers a callback that is invoked every time the process updates the current value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="handle"></param>
        /// <param name="owner"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> OnUpdate<TStruct, TProcessor, TObject>(this IInterpolatorHandle<TStruct, TProcessor> handle, TObject owner, Action<TStruct> callback) 
            where TStruct : unmanaged 
            where TObject : UnityEngine.Object 
            where TProcessor : unmanaged, IInterpolator<TStruct>
            => RegisterCallback(handle, owner, callback, EventFlags.OnUpdate);

        /// <summary>
        /// Registers a callback that is invoked every time the process updates the current value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="handle"></param>
        /// <param name="owner"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> OnUpdate<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle, Action<TStruct> callback)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
            => RegisterCallback(handle, callback, EventFlags.OnUpdate);

        /// <summary>
        /// Registers a callback that is invoked once the process is paused
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="handle"></param>
        /// <param name="owner"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> OnPause<TStruct, TProcessor, TObject>(this IInterpolatorHandle<TStruct, TProcessor> handle, TObject owner, Action<TStruct> callback) 
            where TStruct : unmanaged 
            where TObject : UnityEngine.Object 
            where TProcessor : unmanaged, IInterpolator<TStruct>
            => RegisterCallback(handle, owner, callback, EventFlags.OnPause);

        /// <summary>
        /// Registers a callback that is invoked once the process is paused
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> OnPause<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle, Action<TStruct> callback) 
            where TStruct : unmanaged 
            where TProcessor : unmanaged, IInterpolator<TStruct>
            => RegisterCallback(handle, callback, EventFlags.OnPause);

        /// <summary>
        /// Pauses the interpolation untill <see cref="Resume{T}(IInterpolatorHandle{T})"/> is called or the process is killed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        public static IInterpolatorHandle<TStruct, TProcessor> Pause<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            ref var ctx = ref ProcessCache.GetContext(handle.ProcessId);
            ctx.ActiveFlags |= EventFlags.OnPause; // Forces the next call cycle to call pause delegates
            ctx.Status = ExecutionStatus.Paused;
            return handle;
        }

        /// <summary>
        /// Resumes the interpolation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        public static IInterpolatorHandle<TStruct, TProcessor> Resume<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            ref var ctx = ref ProcessCache.GetContext(handle.ProcessId);
            ctx.ActiveFlags |= EventFlags.OnResume; // Forces the next call cycle to call resume delegates
            ctx.Status = ExecutionStatus.Running;
            return handle;
        }

        /// <summary>
        /// Completes the process. This will essentially just mark the process as completed.
        /// Disposes the process on next execution cycle and calls OnComplete if there are registered flags.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        public static IInterpolatorHandle<TStruct, TProcessor> Complete<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle) 
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            ref var ctx = ref ProcessCache.GetContext(handle.ProcessId);
            ctx.Status = ExecutionStatus.Completed;
            return handle;
        }

        /// <summary>
        /// Kills the process and ingores OnComplete flags
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        public static IInterpolatorHandle<TStruct, TProcessor> Kill<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            //EFAnimator.KillTargetExternal(handle);
            return handle;
        }

        /// <summary>
        /// Sets max loop count for the process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> SetLoopLimit<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle, int count)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            ref var clock = ref ProcessCache.GetClock(handle.ProcessId);
            clock.MaxLoops = count;
            return handle;
        }

        /// <summary>
        /// Called once a loop has been completed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="cb"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> OnLoopCompleted<TStruct, TProcessor>(this IInterpolatorHandle<TStruct, TProcessor> handle, Action<TStruct> cb)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
            => RegisterCallback(handle, cb, EventFlags.OnLoopCompleted);

        /// <summary>
        /// Called once a loop has been completed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="cb"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<TStruct, TProcessor> OnLoopCompleted<TStruct, TProcessor, TObject>(this IInterpolatorHandle<TStruct, TProcessor> handle, TObject owner, Action<TStruct> cb)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
            where TObject : UnityEngine.Object
            => RegisterCallback(handle, owner, cb, EventFlags.OnLoopCompleted);

    }
}


[Flags]
public enum EventFlags : ushort
{   
    None = 0,
    OnComplete = 1,
    OnStart = 2,
    OnPause = 4,
    OnResume = 8,
    OnKill = 16,
    OnUpdate = 32,
    OnLoopCompleted = 64
}


