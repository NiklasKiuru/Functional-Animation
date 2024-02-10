using Aikom.FunctionalAnimation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public interface IInterpolatorHandle<T>
{
    public ExecutionStatus Status { get; set; }
}

public static class HandleExtensions
{
    public static IInterpolatorHandle<T> OnComplete<T>(this IInterpolatorHandle<T> handle, Action callback) where T : unmanaged
    {
        CallbackRegistry.RegisterCallback(callback, false);
        return handle;
    }

    public static IInterpolatorHandle<T> OnStart<T>(this IInterpolatorHandle<T> handle, Action callback) where T : unmanaged
    {
        CallbackRegistry.RegisterCallback(callback, false);
        return handle;
    }

    public static IInterpolatorHandle<T> OnValueReached<T>(this IInterpolatorHandle<T> handle, Action callback, T value) where T : unmanaged
    {
        CallbackRegistry.RegisterCallback(callback, true);
        return handle;
    }

    public static IInterpolatorHandle<T> Pause<T>(this IInterpolatorHandle<T> handle) where T : unmanaged
    {   
        handle.Status = ExecutionStatus.Paused;
        return handle;
    }

    public static IInterpolatorHandle<T> Resume<T>(this IInterpolatorHandle<T> handle) where T : unmanaged
    {
        handle.Status = ExecutionStatus.Running;
        return handle;
    }

    public static IInterpolatorHandle<T> Kill<T>(this IInterpolatorHandle<T> handle) where T : unmanaged
    {
        handle.Status = ExecutionStatus.Completed;
        return handle;
    }

    public static IInterpolatorHandle<T> OnUpdate<T>(this IInterpolatorHandle<T> handle, Action<T> callback) where T : unmanaged
    {
        //CallbackRegistry.RegisterCallback(callback, true);
        return handle;
    }
}

[Flags]
public enum EventFlags
{
    None = 0,
    OnComplete = 1,
    OnStart = 2,
    OnValueReached = 4,
    OnPause = 8,
    OnResume = 16,
    OnKill = 32
}


