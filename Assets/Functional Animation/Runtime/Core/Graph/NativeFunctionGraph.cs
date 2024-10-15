using Codice.CM.SEIDInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    /// <summary>
    /// Native container for GraphData
    /// </summary>
    public unsafe struct NativeFunctionGraph : IDisposable
    {
        [NativeDisableUnsafePtrRestriction]
        private RangedFunction* _buffer;
        private int _length;
        private Allocator _allocator;
        [MarshalAs(UnmanagedType.U1)]
        private bool _isMultiGraph;

        public bool IsCreated { get =>  _buffer != null; }
        public int Length { get => _length; }

        public RangedFunction this[int index]
        {
            get
            {
                if(index < 0 || index >= _length)
                    throw new IndexOutOfRangeException();
                return UnsafeUtility.ReadArrayElement<RangedFunction>(_buffer, index);
            }
            set 
            {
                UnsafeUtility.WriteArrayElement<RangedFunction>(_buffer, index, value); 
            }
        }

        public NativeFunctionGraph(GraphData data, Allocator alloc, bool isMultiGraph = false)
        {
            _isMultiGraph = isMultiGraph;
            _length = data.Length;
            _allocator = alloc;
            _buffer = (RangedFunction*)UnsafeUtility.Malloc(data.Length, UnsafeUtility.AlignOf<RangedFunction>(), alloc);
            Span<RangedFunction> span = stackalloc RangedFunction[data.Length];
            data.CopyData(ref span);
            for(int i = 0; i < data.Length; i++)
            {
                this[i] = span[i];
            }
        }

        public NativeFunctionGraph(RangedFunction[] array, Allocator alloc, bool isMultiGraph = false)
        {
            _isMultiGraph = isMultiGraph;
            _length = array.Length;
            _allocator = alloc;
            _buffer = (RangedFunction*)UnsafeUtility.Malloc(array.Length, UnsafeUtility.AlignOf<RangedFunction>(), alloc);
            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
            var ptr = handle.AddrOfPinnedObject();

            try
            {
                UnsafeUtility.MemCpy((byte*)_buffer, (byte*)(void*)ptr, _length);
            }
            finally 
            { 
                handle.Free(); 
            }
        }

        public NativeFunctionGraph(Span<RangedFunction> span, Allocator alloc, bool isMultiGraph = false)
        {
            _isMultiGraph = isMultiGraph;
            _length = span.Length;
            _allocator = alloc;
            _buffer = (RangedFunction*)UnsafeUtility.Malloc(span.Length, UnsafeUtility.AlignOf<RangedFunction>(), alloc);
            for (int i = 0; i < _length; i++)
            {
                this[i] = span[i];
            }
        }

        /// <summary>
        /// Frees allocated memory and marks the structure as deallocated
        /// </summary>
        public void Dispose()
        {
            if (IsCreated)
            {
                UnsafeUtility.Free(_buffer, _allocator);
                _buffer = null;
                _length = 0;
            }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            else
                throw new InvalidOperationException("The graph has already been disposed");
#endif
        }

        /// <summary>
        /// Evaluates the function graph at specifc point in time
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public float Evaluate(float time)
        {
            if(!IsCreated)
                return 0;
            var func = GetFunctionInternal(time, 0, _length);
            return func.Evaluate(time);
        }

        /// <summary>
        /// Gets a function based on time
        /// </summary>
        /// <param name="time">Normalized time value between [0, 1]</param>
        /// <returns></returns>
        public RangedFunction GetFunction(float time)
        {
            if (!IsCreated)
                return default;
            return GetFunctionInternal(time, 0, _length);
        }

        /// <summary>
        /// Gets a function within a specific range and time. If the graph is not a multigraph returns the value only based on time
        /// </summary>
        /// <param name="time"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public RangedFunction GetFunction(float time, int2 range)
        {
            if(!IsCreated)
                return default;
            if (!_isMultiGraph)
                return GetFunctionInternal(time, 0, _length);
            return GetFunctionInternal(time, range.x, range.y);
        }

        /// <summary>
        /// Gets a function based on time without safety checks
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private RangedFunction GetFunctionInternal(float time, int start, int end)
        {
            for (int j = start; j < end; j++)
            {
                var rangedFunc = this[j];
                var startingNode = rangedFunc.Start;
                var endingNode = rangedFunc.End;
                if (time >= startingNode.x && time <= endingNode.x)
                {
                    return rangedFunc;
                }
            }
            return this[_length - 1];
        }
    }
}
