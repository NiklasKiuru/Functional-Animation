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
        /// Process id of this handle
        /// </summary>
        public int Id { get; internal set; }

        /// <summary>
        /// Alive status of the process
        /// </summary>
        public bool IsAlive { get; internal set; }

        /// <summary>
        /// Gets the current interpolation value of this handle process
        /// </summary>
        /// <remarks>It is not recommended to use this option frequently. 
        /// For frequent updates use <see cref="OnUpdate{T, D}(IInterpolatorHandle{T}, D, Action{T})"/></remarks>
        public T GetValue();
    }

    public static class HandleExtensions
    {
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
            where D : class
        {
            CallbackRegistry.RegisterCallback(handle.Id, callback, owner, EventFlags.OnComplete);
            EFAnimator.SetPassiveFlagsExternal(ref handle, EventFlags.OnComplete);
            return handle;
        }

        /// <summary>
        /// Restarts the process. This can only be used on allocated processes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static IInterpolatorHandle<T> Restart<T>(this IInterpolatorHandle<T> handle) where T : unmanaged
        {   
            EFAnimator.RestartProcess(handle);
            return handle;
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
            where D : class
        {
            CallbackRegistry.RegisterCallback(handle.Id, callback, owner, EventFlags.OnKill);
            EFAnimator.SetPassiveFlagsExternal(ref handle, EventFlags.OnKill);
            return handle;
        }

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
            where D : class
        {
            CallbackRegistry.RegisterCallback(handle.Id, callback, owner, EventFlags.OnResume);
            EFAnimator.SetPassiveFlagsExternal(ref handle, EventFlags.OnResume);
            return handle;
        }

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
            where D : class
        {
            CallbackRegistry.RegisterCallback(handle.Id, callback, owner, EventFlags.OnStart);
            EFAnimator.SetPassiveFlagsExternal(ref handle, EventFlags.OnStart);
            return handle;
        }

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
            where D : class
        {
            CallbackRegistry.RegisterCallback(handle.Id, callback, owner, EventFlags.OnUpdate);
            EFAnimator.SetPassiveFlagsExternal(ref handle, EventFlags.OnUpdate);
            return handle;
        }

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
            where D : class
        {
            CallbackRegistry.RegisterCallback(handle.Id, callback, owner, EventFlags.OnPause);
            EFAnimator.SetPassiveFlagsExternal(ref handle, EventFlags.OnPause);
            return handle;
        }

        /// <summary>
        /// Pauses the interpolation untill <see cref="Resume{T}(IInterpolatorHandle{T})"/> is called or the process is killed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        public static IInterpolatorHandle<T> Pause<T>(this IInterpolatorHandle<T> handle) where T : unmanaged
        {
            CallbackRegistry.TryCall(new EventData<T>() { Id = handle.Id, Flags = EventFlags.OnPause, Value = handle.GetValue() });
            EFAnimator.ForceExecutionStatusExternal(ref handle, ExecutionStatus.Paused);
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
            EFAnimator.ForceExecutionStatusExternal(ref handle, ExecutionStatus.Running);
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
            EFAnimator.ForceExecutionStatusExternal(ref handle, ExecutionStatus.Completed);
            return handle;
        }

        /// <summary>
        /// Kills the process immediatly and ingores OnComplete flags
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        public static IInterpolatorHandle<T> Kill<T>(this IInterpolatorHandle<T> handle) where T : unmanaged
        {
            EFAnimator.KillTargetExternal(ref handle);
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


