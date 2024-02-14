using System;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Event container for processes that use <see cref="EventFlags"/>
    /// </summary>
    public class QuickActions
    {
        private ActionContainer _onStart;
        private ActionContainer _onComplete;
        private ActionContainer _onUpdate;
        private ActionContainer _onKill;
        private ActionContainer _onResume;
        private ActionContainer _onPause;

        private int _totalCount = 0;

        public QuickActions()
        {
            _onStart = new ActionContainer();
            _onComplete = new ActionContainer();
            _onUpdate = new ActionContainer();
            _onKill = new ActionContainer();
            _onResume = new ActionContainer();
            _onPause = new ActionContainer();
        }

        /// <summary>
        /// Invokes all stored callback methods based on given flags
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="activeFlags"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Invoke<T>(in FlagIndexer<T> activeFlags) where T : struct
        {
            if ((activeFlags.Flags & EventFlags.OnStart) == EventFlags.OnStart)
                _onStart.InvokeAll(activeFlags.Value);
            if ((activeFlags.Flags & EventFlags.OnComplete) == EventFlags.OnComplete)
                _onComplete.InvokeAll(activeFlags.Value);
            if ((activeFlags.Flags & EventFlags.OnUpdate) == EventFlags.OnUpdate)
                _onUpdate.InvokeAll(activeFlags.Value);
            if ((activeFlags.Flags & EventFlags.OnKill) == EventFlags.OnKill)
                _onKill.InvokeAll(activeFlags.Value);
            if ((activeFlags.Flags & EventFlags.OnResume) == EventFlags.OnResume)
                _onResume.InvokeAll(activeFlags.Value);
            if ((activeFlags.Flags & EventFlags.OnPause) == EventFlags.OnPause)
                _onPause.InvokeAll(activeFlags.Value);
        }

        /// <summary>
        /// Adds a new callback into an action container
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cb"></param>
        /// <param name="target"></param>
        /// <param name="flag"></param>
        public void Add<T>(Action<T> cb, object target, EventFlags flag) where T : struct
        {
            switch(flag)
            {
                case EventFlags.None:
                    return;
                case EventFlags.OnStart:
                    _onStart.Add(cb, target); 
                    break;
                case EventFlags.OnComplete: 
                    _onComplete.Add(cb, target);
                    break;
                case EventFlags.OnPause: 
                    _onPause.Add(cb, target);
                    break;
                case EventFlags.OnResume: 
                    _onResume.Add(cb, target);
                    break;
                case EventFlags.OnKill: 
                    _onKill.Add(cb, target);
                    break;
                case EventFlags.OnUpdate: 
                    _onUpdate.Add(cb, target);
                    break;
            }
            _totalCount++;
        }

        /// <summary>
        /// Clears previous entries
        /// </summary>
        public void Clear()
        {   
            if(_totalCount == 0) 
                return;
            _onStart.Clear();
            _onComplete.Clear();
            _onPause.Clear();
            _onResume.Clear();
            _onKill.Clear();
            _onUpdate.Clear();
            _totalCount = 0;
        }

        /// <summary>
        /// Container for callback methods
        /// </summary>
        private class ActionContainer
        {
            public object[] Actions;
            public object[] Targets;

            private int _count = 0;

            public ActionContainer()
            {
                Actions = new object[4];
                Targets = new object[4];
            }

            /// <summary>
            /// Adds a new reciever and a call
            /// </summary>
            /// <param name="action"></param>
            /// <param name="target"></param>
            public void Add(object action, object target)
            {
                if(Actions.Length == _count)
                {
                    Array.Resize(ref Actions, _count * 2);
                    Array.Resize(ref Targets, _count * 2);
                }
                Actions[_count] = action;
                Targets[_count] = target;
                _count++;
            }

            /// <summary>
            /// Fires all methods as long as the intended reciever is still alive
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="value"></param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void InvokeAll<T>(T value) where T : struct
            {
                for(int i = 0; i < Actions.Length; i++)
                {
                    if (i == _count)
                        return;
                    if (Targets[i] != null)
                        UnsafeUtility.As<object, Action<T>>(ref Actions[i])?.Invoke(value);
                }
            }

            /// <summary>
            /// Clears all arrays if there are assigned callers in them
            /// </summary>
            public void Clear()
            {
                if (_count == 0)
                    return;
                for(int i = 0; i < Actions.Length; i++)
                {
                    Actions[i] = null;
                    Targets[i] = null;
                }
                _count = 0;
            }
        }
    }
}

