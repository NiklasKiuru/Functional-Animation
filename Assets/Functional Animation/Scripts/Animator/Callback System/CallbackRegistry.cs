using Aikom.FunctionalAnimation;
using System;
using System.Collections.Generic;
using Unity.Collections;

public class CallbackRegistry : IManagedObject, IDisposable
{   
    private static bool _isDirty;
    private static int _runningIndex = 0;
    private static Dictionary<int, CallbackHandle> _callbacks = new Dictionary<int, CallbackHandle>();
    private NativeList<int> _buffer;
    private static Stack<CallbackHandle> _stack = new Stack<CallbackHandle>();
    private const int c_intialSize = 128;

    public CallbackRegistry(NativeList<int> buffer)
    {
        _isDirty = false;
        _buffer = buffer;
        for(int i = 0; i < c_intialSize; i++)
        {
            _callbacks.Add(_runningIndex, new CallbackHandle(null, false, _runningIndex));
            _runningIndex++;
        }

        UpdateManager.RegisterObject(this);
    }

    public void OnUpdate()
    {
        if(!_isDirty)
            return;

        for(int i = 0; i < _buffer.Length; i++)
        {
            if (_callbacks.TryGetValue(_buffer[i], out var handle))
            {
                handle.Callback.Invoke();
                if (!handle.IsPersistent)
                    _stack.Push(handle);
            }
        }

        _buffer.Clear();
        _isDirty = false;
    }

    internal static void SetDirty() => _isDirty = true;

    public static int RegisterCallback(Action callback, bool isPersistent)
    {
        if (callback == null)
            return 0;

        if(_stack.TryPop(out var handle))
        {
            _callbacks[handle.Id] = handle;
            handle.Callback = callback;
            handle.IsPersistent = isPersistent;
            return _runningIndex;
        }
        _callbacks.Add(_runningIndex, new CallbackHandle(callback, isPersistent, _runningIndex));
        _runningIndex++;
        return _runningIndex;
    }

    public void Dispose()
    {   
        if(_buffer.IsCreated)
            _buffer.Dispose();
        _runningIndex = 0;
        _callbacks.Clear();
        _stack.Clear();
    }

    private class CallbackHandle
    {
        public Action Callback;
        public bool IsPersistent;
        public int Id;

        public CallbackHandle(Action cb, bool persist, int id)
        {
            Callback = cb;
            IsPersistent = persist;
            Id = id;
        }
    }
}
