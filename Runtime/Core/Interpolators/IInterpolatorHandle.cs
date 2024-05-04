using System;

namespace Aikom.FunctionalAnimation
{
    /// <summary>
    /// Public interface to control interpolator processes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IInterpolatorHandle<T> : IGroupProcessor where T : unmanaged
    {
        /// <summary>
        /// Gets the current interpolation value of this handle process
        /// </summary>
        /// <remarks>It is not recommended to use this option frequently. 
        /// For frequent updates use <see cref="HandleExtensions.OnUpdate{T}(IInterpolatorHandle{T}, Action{T})"/></remarks>
        public T GetValue();
    }

    public static class HandleExtensions
    {   
        /// <summary>
        /// Registers callbacks for this handle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="cb"></param>
        /// <param name="flags"></param>
        /// <remarks>All callbacks are automatically void once the process dies so there is no need for unregistering</remarks>
        /// <returns></returns>
        public static IInterpolatorHandle<T> RegisterCallback<T>(this IInterpolatorHandle<T> handle, Action<T> cb, EventFlags flags)
            where T : unmanaged
        {
            if(cb != null && flags != EventFlags.None)
                EFAnimator.RegisterStaticCallback(handle, cb, flags);
            return handle;
        }

        /// <summary>
        /// Registers callbacks for this handle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="cb"></param>
        /// <param name="flags"></param>
        /// <remarks>All callbacks are automatically void once the process dies so there is no need for unregistering</remarks>
        /// <returns></returns>
        public static IInterpolatorHandle<T> RegisterCallback<T, D>(this IInterpolatorHandle<T> handle, D owner, Action<T> cb, EventFlags flags)
            where T : unmanaged
            where D : UnityEngine.Object
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
        public static IInterpolatorHandle<T> OnComplete<T, D>(this IInterpolatorHandle<T> handle, D owner, Action<T> callback)
            where T : unmanaged
            where D : UnityEngine.Object
            => RegisterCallback(handle, owner, callback, EventFlags.OnComplete);
        
        /// <summary>
        /// Registers a callback that is invoked once the process has completed succesfully
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<T> OnComplete<T>(this IInterpolatorHandle<T> handle, Action<T> callback)
            where T : unmanaged
            => RegisterCallback(handle, callback, EventFlags.OnComplete);

        /// <summary>
        /// Restarts the process. This can only be used on allocated processes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<T> Restart<T>(this IInterpolatorHandle<T> handle) 
            where T : unmanaged
        {   
            EFAnimator.RestartProcess(handle);
            return handle;
        }

        /// <summary>
        /// Sets the current process as inactive for the given delay and continues after the delay has passed.
        /// Does not call OnPause or OnResume
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<T> Hibernate<T>(this IInterpolatorHandle<T> handle, float delay)
            where T : unmanaged
        {   
            // This is guaranteed to die once delay has been reached
            var procId = EF.CreateNonAlloc(0, 1, delay, Function.Linear, TimeControl.PlayOnce, 1);
            EFAnimator.SetPassiveFlagsInternal(procId, EventFlags.OnKill);

            // Disable this handle internally temporarily
            EFAnimator.ForceExecutionStatusExternal(handle, ExecutionStatus.Inactive);

            // Sets a callback to set the status of this handle back to running state once the earlier process dies
            EFAnimator.RegisterStaticCallback<T>(procId, SetStatus, EventFlags.OnKill);
            return handle;

            void SetStatus(T val) => EFAnimator.ForceExecutionStatusExternal(handle, ExecutionStatus.Running);
        }

        /// <summary>
        /// Registers a callback that is invoked once the process has been killed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="handle"></param>
        /// <param name="owner"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<T> OnKill<T, D>(this IInterpolatorHandle<T> handle, D owner, Action<T> callback) 
            where T : unmanaged
            where D : UnityEngine.Object
            => RegisterCallback(handle, owner, callback, EventFlags.OnKill);

        /// <summary>
        /// Registers a callback that is invoked once the process has been killed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="handle"></param>
        /// <param name="owner"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<T> OnKill<T>(this IInterpolatorHandle<T> handle, Action<T> callback)
            where T : unmanaged
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
        public static IInterpolatorHandle<T> OnResume<T, D>(this IInterpolatorHandle<T> handle, D owner, Action<T> callback)
            where T : unmanaged
            where D : UnityEngine.Object
            => RegisterCallback(handle, owner, callback, EventFlags.OnResume);

        /// <summary>
        /// Registers a callback that is invoked once the process resumes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<T> OnResume<T>(this IInterpolatorHandle<T> handle, Action<T> callback)
            where T : unmanaged
            => RegisterCallback(handle, callback, EventFlags.OnResume);

        /// <summary>
        /// Registers a callback that is invoked once the process starts for the first time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="handle"></param>
        /// <param name="owner"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<T> OnStart<T, D>(this IInterpolatorHandle<T> handle, D owner, Action<T> callback)
            where T : unmanaged
            where D : UnityEngine.Object
            => RegisterCallback(handle, owner, callback, EventFlags.OnStart);

        /// <summary>
        /// Registers a callback that is invoked once the process starts for the first time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<T> OnStart<T>(this IInterpolatorHandle<T> handle, Action<T> callback)
            where T : unmanaged
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
        public static IInterpolatorHandle<T> OnUpdate<T, D>(this IInterpolatorHandle<T> handle, D owner, Action<T> callback)
            where T : unmanaged
            where D : UnityEngine.Object
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
        public static IInterpolatorHandle<T> OnUpdate<T>(this IInterpolatorHandle<T> handle, Action<T> callback)
            where T : unmanaged
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
        public static IInterpolatorHandle<T> OnPause<T, D>(this IInterpolatorHandle<T> handle, D owner, Action<T> callback)
            where T : unmanaged
            where D : UnityEngine.Object
            => RegisterCallback(handle, owner, callback, EventFlags.OnPause);

        /// <summary>
        /// Registers a callback that is invoked once the process is paused
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<T> OnPause<T>(this IInterpolatorHandle<T> handle, Action<T> callback)
            where T : unmanaged
            => RegisterCallback(handle, callback, EventFlags.OnPause);

        /// <summary>
        /// Pauses the interpolation untill <see cref="Resume{T}(IInterpolatorHandle{T})"/> is called or the process is killed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        public static IInterpolatorHandle<T> Pause<T>(this IInterpolatorHandle<T> handle) where T : unmanaged
        {
            CallbackRegistry.TryCall(new EventData<T>() { Id = handle.Id, Flags = EventFlags.OnPause, Value = handle.GetValue() });
            EFAnimator.ForceExecutionStatusExternal(handle, ExecutionStatus.Paused);
            return handle;
        }

        /// <summary>
        /// Resumes the interpolation
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        public static IInterpolatorHandle<T> Resume<T>(this IInterpolatorHandle<T> handle) where T : unmanaged
        {
            CallbackRegistry.TryCall(new EventData<T>() { Id = handle.Id, Flags = EventFlags.OnResume, Value = handle.GetValue() });
            EFAnimator.ForceExecutionStatusExternal(handle, ExecutionStatus.Running);
            return handle;
        }

        /// <summary>
        /// Completes the process. This will essentially just mark the process as completed.
        /// Disposes the process on next execution cycle and calls OnComplete if there are registered flags.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        public static IInterpolatorHandle<T> Complete<T>(this IInterpolatorHandle<T> handle) where T : unmanaged
        {
            EFAnimator.ForceExecutionStatusExternal(handle, ExecutionStatus.Completed);
            return handle;
        }

        /// <summary>
        /// Kills the process immediatly and ingores OnComplete flags
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        public static IInterpolatorHandle<T> Kill<T>(this IInterpolatorHandle<T> handle) where T : unmanaged
        {
            EFAnimator.KillTargetExternal(handle);
            return handle;
        }

        /// <summary>
        /// Sets max loop count for the process
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<T> SetLoopLimit<T>(this IInterpolatorHandle<T> handle, int count)
            where T : unmanaged
        {
            EFAnimator.SetMaxLoopCountExternal(handle, count);
            return handle;
        }
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
}


