using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Aikom.FunctionalAnimation
{
    internal class CallbackRegistry
    {
        private static Dictionary<int, QuickActions> _callbacks = new Dictionary<int, QuickActions>();
        private static Stack<QuickActions> _stack = new Stack<QuickActions>();

        /// <summary>
        /// Preallocates 
        /// </summary>
        /// <param name="allocSize"></param>
        public static void Prime(int allocSize)
        {
            var arr = new QuickActions[allocSize];
            for(int i = 0; i < arr.Length; i++)
            {
                arr[i] = new QuickActions();
            }
            _stack = new Stack<QuickActions>(arr);
        }

        /// <summary>
        /// Registers a callback with interpolators ID
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="D"></typeparam>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        /// <param name="owner"></param>
        /// <param name="flag"></param>
        public static void RegisterCallback<T, D>(int id, Action<T> callback, D owner, EventFlags flag)
            where T : struct
            where D : class
        {
            if (callback == null)
                return;

            // Gets a free one from the pool
            if (!_callbacks.ContainsKey(id) && _stack.TryPop(out var handle))
            {   
                handle.Clear();
                handle.Add(callback, owner, flag);
                _callbacks.Add(id, handle);
            }
            // Adds flags into an existing handle
            else if (_callbacks.TryGetValue(id, out var existingHandle))
            {
                existingHandle.Add(callback, owner, flag);
            }
            // Creates a new handle
            else
            {
                var handler = new QuickActions();
                handler.Add(callback, owner, flag);
                _callbacks.Add(id, handler);
            }
        }

        /// <summary>
        /// Calls all valid flagged callbacks if there are active recievers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="activeFlags"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TryCall<T>(EventData<T> activeFlags) where T : struct
        {
            if (_callbacks.TryGetValue(activeFlags.Id, out var action))
            {
                action.Invoke(activeFlags);
            }
        }

        /// <summary>
        /// Removes all callback handles associated with this ID
        /// </summary>
        /// <param name="id"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnregisterCallbacks(int id)
        {
            if (_callbacks.TryGetValue(id, out var handle))
            {
                _callbacks.Remove(id);
                _stack.Push(handle);
            }
        }
    }
}

