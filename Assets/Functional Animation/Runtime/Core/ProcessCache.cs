using Aikom.FunctionalAnimation.Extensions;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Aikom.FunctionalAnimation
{
    /// <summary>
    /// Pseudo ECS cache system
    /// </summary>
    [BurstCompile]
    public static class ProcessCache
    {
        private static NativeArray<Clock> _clocks;
        private static NativeArray<ExecutionContext> _contexts;
        private static NativeArray<Process> _pids;
        private static NativeArray<int> _groupReferences;
        private static NativeArray<PluginValueCache> _valueCache;
        private static NativeArray<NativeFunctionGraph> _functionHeap;

        private static int _runningIndex;
        private static NativeQueue<int> _indexQue;
        private static NativeHashMap<int, int> _groupHashIndexMap;

        internal static NativeArray<int> Groupreferences { get { return _groupReferences; } }
        internal static NativeArray<Process> Pids { get { return _pids; } }
        internal static NativeArray<NativeFunctionGraph> FunctionHeap { get { return _functionHeap; } }
        internal static NativeArray<Clock> Clocks { get { return _clocks; } }
        internal static NativeArray<ExecutionContext> Contexts { get { return _contexts; } }

        /// <summary>
        /// Is the cache allocated?
        /// </summary>
        private static bool IsCreated => _pids.IsCreated;

        /// <summary>
        /// The maximum amount of possible active processes currently in the system. 
        /// This value is always less than the cache capacity but most likely larger than current active process count
        /// </summary>
        public static int MaxCount => _runningIndex;

        /// <summary>
        /// Creates all native arrays for the cache
        /// </summary>
        /// <param name="initialCapacity"></param>
        internal static void Create(int initialCapacity)
        {
            if(IsCreated) 
                return;
            _clocks = new NativeArray<Clock>(initialCapacity, Allocator.Persistent);
            _contexts = new NativeArray<ExecutionContext>(initialCapacity, Allocator.Persistent);
            _pids = new NativeArray<Process>(initialCapacity, Allocator.Persistent);
            _groupReferences = new NativeArray<int>(initialCapacity, Allocator.Persistent);
            _functionHeap = new NativeArray<NativeFunctionGraph>(initialCapacity, Allocator.Persistent);
            _indexQue = new NativeQueue<int>(Allocator.Persistent);
            _groupHashIndexMap = new NativeHashMap<int, int>();
        }

        /// <summary>
        /// Destroys and deallocates all unmanaged data
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        internal static void Destroy() 
        {
            if (!IsCreated)
                throw new InvalidOperationException("The cache has already been disposed");
            _clocks.Dispose();
            _contexts.Dispose();
            _pids.Dispose();
            _indexQue.Dispose();
            _groupHashIndexMap.Dispose();
            _groupReferences.Dispose();

            for(int i = 0; i < _valueCache.Length; i++)
                if(_valueCache.IsCreated)
                    _valueCache[i].Dispose();
            for(int i = 0; i < _functionHeap.Length; i++)
                if (_functionHeap[i].IsCreated)
                    _functionHeap[i].Dispose();

            _functionHeap.Dispose();
            _valueCache.Dispose();
        }

        /// <summary>
        /// Creates a new plugin value cache
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <typeparam name="TProcessor"></typeparam>
        /// <param name="capacity"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static int CreatePluginCache<TStruct, TProcessor>(int capacity)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            var groupHashKey = IGroupProcessor.GetStaticGroupID<TStruct, TProcessor>();
            if (_groupHashIndexMap.ContainsKey(groupHashKey))
                throw new InvalidOperationException("Plugin value cache already exists");
            var pos = _valueCache.Length;
            _valueCache.ResizeArray(_valueCache.Length + 1);
            _groupHashIndexMap.Add(groupHashKey, pos);
            _valueCache[pos] = PluginValueCache.Create<TStruct, TProcessor>(capacity, Allocator.Persistent);
            return pos;
        }

        /// <summary>
        /// Checks if managed containers have enough space and in case there isn't reallocates all containers
        /// </summary>
        private static void CheckAndReallocate()
        {
            // Last index is in use and there are no free indecies
            if (_runningIndex - 1 >= _pids.Length && _indexQue.Count == 0)
            {
                var newCapacity = _pids.Length * 2;
                _pids.ResizeArray(newCapacity);
                _clocks.ResizeArray(newCapacity);
                _contexts.ResizeArray(newCapacity);
                _functionHeap.ResizeArray(newCapacity);
                _groupReferences.ResizeArray(newCapacity);
            }
        }

        /// <summary>
        /// Registers new process data
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <typeparam name="TProcessor"></typeparam>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="proc"></param>
        /// <returns></returns>
        internal static Process RegisterProcess<TStruct, TProcessor>(TStruct start, TStruct end, TProcessor proc)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            var group = GetCache<TStruct, TProcessor>();
            var groupId = group.RegisterValues(start, end, proc);
            Process processId;
            if(_indexQue.TryDequeue(out var index))
            {
                processId = new Process(index, groupId).IncrimentVersion();
            }
            else
            {
                _runningIndex++;
                processId = new Process(_runningIndex, groupId);
            }
            _pids[processId.Id] = processId;
            _contexts[processId.Id] = new ExecutionContext() { Status = ExecutionStatus.Running };
            return processId;
        }

        /// <summary>
        /// Returns the starting value of a process
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <typeparam name="TProcessor"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public unsafe static ref TStruct GetStart<TStruct, TProcessor>(Process id)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            CheckValidityAndThrow(id);
            var cache = GetCache<TStruct, TProcessor>();
            return ref *(TStruct*)((byte*)cache._values + (id.GroupId * (long)sizeof(TStruct) * 3));
        }

        /// <summary>
        /// Gets the target value of a process
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <typeparam name="TProcessor"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public unsafe static ref TStruct GetEnd<TStruct, TProcessor>(Process id)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            CheckValidityAndThrow(id);
            var cache = GetCache<TStruct, TProcessor>();
            return ref *(TStruct*)((byte*)cache._values + (id.GroupId * (long)sizeof(TStruct) * 3) + sizeof(TStruct));
        }

        /// <summary>
        /// Gets the current value of a process
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <typeparam name="TProcessor"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public unsafe static TStruct GetCurrent<TStruct, TProcessor>(Process id)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            CheckValidityAndThrow(id);
            var cache = GetCache<TStruct, TProcessor>();
            return cache.GetCurrent<TStruct>(id.GroupId);
        }

        /// <summary>
        /// Checks if the used process id is valid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool CheckValidity(Process id)
            => id.Id < _pids.Length && id.Id > 0 && id.Equals(_pids[id.Id]);

        /// <summary>
        /// Sets new graph data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="span"></param>
        /// <param name="isMultiGraph"></param>
        public static void SetGraph(Process id, Span<RangedFunction> span, bool isMultiGraph)
        {
            CheckValidityAndThrow(id);
            var graph = _functionHeap[id.Id];
            if(graph.IsCreated)
                graph.Dispose();
            graph = new NativeFunctionGraph(span, Allocator.Persistent, isMultiGraph);
            _functionHeap[id.Id] = graph;
        }

        /// <summary>
        /// Returns the current process context
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal unsafe static ref ExecutionContext GetContext(Process id)
        {
            CheckValidityAndThrow(id);
            return ref *(ExecutionContext*)((byte*)_contexts.GetUnsafePtr() + (long)id.Id * (long)sizeof(ExecutionContext));
        }

        /// <summary>
        /// Returns the allocated native function graph of the process
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public unsafe static ref NativeFunctionGraph GetGraph(Process id)
        {
            CheckValidityAndThrow(id);
            return ref *(NativeFunctionGraph*)((byte*)_functionHeap.GetUnsafePtr() + (long)id.Id * (long)sizeof(NativeFunctionGraph));
        }

        /// <summary>
        /// Gets all time data of a process
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public unsafe static ref Clock GetClock(Process id)
        {
            CheckValidityAndThrow(id);
            return ref *(Clock*)((byte*)_clocks.GetUnsafePtr() + (long)id.Id * (long)sizeof(Clock));
        }

        /// <summary>
        /// Checks process validity and throws
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private static void CheckValidityAndThrow(Process id)
        {
            if(id.Id >= _pids.Length || id.Id < 0)
                throw new IndexOutOfRangeException(nameof(id));
            if (!id.Equals(_pids[id.Id]))
                throw new ArgumentException("Given process identity overlaps with another existing one");
        }

        public static int GetValueCacheSize(int index)
        {
            return _valueCache[index].Capacity;
        }

        /// <summary>
        /// Returns a plugin value cache
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <typeparam name="TProcessor"></typeparam>
        /// <returns></returns>
        internal static PluginValueCache GetCache<TStruct, TProcessor>()
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
            => _valueCache[_groupHashIndexMap[IGroupProcessor.GetStaticGroupID<TStruct, TProcessor>()]];

        internal static PluginValueCache GetCache(IProcessGroupHandle plugin)
        {
            return _valueCache[plugin.GroupId];
        }
    }

    public struct ValueVector<TStruct>
        where TStruct : unmanaged
    {
        public TStruct Start; 
        public TStruct End; 
        public TStruct Current;
    }

    public struct ValueRange<TStruct>
        where TStruct : unmanaged
    {
        public TStruct Start;
        public TStruct End;
    }

    [BurstCompile]
    public unsafe struct PluginValueCache
    {
        public bool IsCreated { get { return _processors == null; } }
        public int Capacity => _capacity;

        internal void* _values;
        internal void* _processors;
        Allocator _allocator;
        int _capacity;

        private int _runningIndex;
        private NativeQueue<int> _indexQue;

        internal int RegisterValues<TStruct, TProcessor>(TStruct start, TStruct end, TProcessor proc)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            CheckAndReallocate<TStruct, TProcessor>();
            if(!_indexQue.TryDequeue(out var index))
            {
                _runningIndex++;
                index = _runningIndex;
            }

            SetStart(start, index);
            SetEnd(end, index);
            SetProcessor<TStruct, TProcessor>(proc, index);
            return index;
        }

        private void CheckAndReallocate<TStruct, TProcessor>()
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            // Last index is in use and there are no free indecies
            if (_runningIndex - 1 >= _capacity && _indexQue.Count == 0)
            {
                var newCapacity = _capacity * 2;
                var valPtr = MallocAndClear<ValueVector<TStruct>>(newCapacity, _allocator);
                var procPtr = MallocAndClear<TProcessor>(newCapacity, _allocator);
                UnsafeUtility.MemCpy((byte*)valPtr, (byte*)_values, _capacity * UnsafeUtility.SizeOf<ValueVector<TStruct>>());
                UnsafeUtility.MemCpy((byte*)procPtr, (byte*)_processors, _capacity * UnsafeUtility.SizeOf<TProcessor>());
                UnsafeUtility.Free(_values, _allocator);
                UnsafeUtility.Free(_processors, _allocator);
                _values = valPtr; 
                _processors = procPtr;
                _capacity = newCapacity;
            }
        }

        public void SetStart<TStruct>(TStruct value, int index)
        {
            UnsafeUtility.WriteArrayElement(_values, index * 3, value);
        }

        public void SetEnd<TStruct>(TStruct value, int index)
        {
            UnsafeUtility.WriteArrayElement(_values, index * 3 + 1, value);
        }

        public void SetProcessor<TStruct, TProcessor>(TProcessor processor, int index)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            UnsafeUtility.WriteArrayElement(_processors, index, processor);
        }

        public TStruct GetCurrent<TStruct>(int index)
            where TStruct : unmanaged
        {
            return UnsafeUtility.ReadArrayElement<TStruct>(_values, index * 3 + 2);
        }

        public TProcessor GetProcessor<TStruct, TProcessor>(int index)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            return UnsafeUtility.ReadArrayElement<TProcessor>(_processors, index);
        }

        public ValueVector<TStruct> GetValues<TStruct>(int index)
            where TStruct : unmanaged
        {
            return UnsafeUtility.ReadArrayElement<ValueVector<TStruct>>(_values, index);
        }

        internal static PluginValueCache Create<TStruct, TProcessor>(int capacity, Allocator alloc)
            where TStruct : unmanaged
            where TProcessor : unmanaged, IInterpolator<TStruct>
        {
            var cache = new PluginValueCache();
            cache._values = MallocAndClear<ValueVector<TStruct>>(capacity, alloc);
            cache._processors = MallocAndClear<TProcessor>(capacity, alloc);
            cache._allocator = alloc;
            cache._capacity = capacity;
            cache._runningIndex = 0;
            cache._indexQue = new NativeQueue<int>(alloc);

            return cache;

            
        }

        private static void* MallocAndClear<T>(int capacity, Allocator alloc)
                where T : unmanaged
        {
            var ptr = UnsafeUtility.Malloc(capacity, UnsafeUtility.AlignOf<T>(), alloc);
            UnsafeUtility.MemClear(ptr, (long)capacity * UnsafeUtility.SizeOf<T>());
            return ptr;
        }

        private unsafe void FreeNoChecks()
        {
            UnsafeUtility.Free(_values, _allocator);
            UnsafeUtility.Free(_processors, _allocator);
        }

        internal void Dispose()
        {
            if (IsCreated)
            {
                FreeNoChecks();
                _values = null;
                _processors = null;
                _indexQue.Dispose();
            }
        }
    }
}
