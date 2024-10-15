using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Aikom.FunctionalAnimation
{
    internal class TimeSystem : IDisposable
    {
        private NativeArray<Clock> _clocks;
        private NativeArray<ExecutionContext> _contexts;

        internal TimeSystem(int initialCapacity)
        {
            _clocks = new NativeArray<Clock>(initialCapacity, Allocator.Persistent);
            _contexts = new NativeArray<ExecutionContext>(initialCapacity, Allocator.Persistent);
        }

        public void Dispose()
        {
            if(_clocks.IsCreated)
                _clocks.Dispose();
            if( _contexts.IsCreated)
                _contexts.Dispose();
        }

        public NativeArray<ExecutionContext> RunClocks(float delta)
        {
            var job = new TimeJob
            {
                Delta = delta,
                Clocks = _clocks,
                Contexts = _contexts
            };
            job.Run();
            return _contexts;
        }

    }

    [BurstCompile]
    internal struct TimeJob : IJob
    {
        public NativeArray<Clock> Clocks;
        public NativeArray<ExecutionContext> Contexts;
        public float Delta;

        public void Execute()
        {
            for(int i = 0; i < Clocks.Length; i++)
            {
                var ctx = Contexts[i];
                if (ctx.Status == ExecutionStatus.Inactive || ctx.Status == ExecutionStatus.Paused)
                    continue;
                var clock = Clocks[i];
                var activeFlags = ctx.ActiveFlags;

                if (clock.Time == 0 && (ctx.PassiveFlags & EventFlags.OnStart) == EventFlags.OnStart)
                    activeFlags |= EventFlags.OnStart;

                var previousLoop = clock.CurrentLoop;
                var time = clock.Tick(Delta);

                if (previousLoop < clock.CurrentLoop)
                {
                    if ((ctx.PassiveFlags & EventFlags.OnLoopCompleted) == EventFlags.OnLoopCompleted)
                        activeFlags |= EventFlags.OnLoopCompleted;
                }
                if (clock.IsCompleted)
                {
                    ctx.Status = ExecutionStatus.Completed;
                    activeFlags |= EventFlags.OnKill;
                    if((ctx.PassiveFlags & EventFlags.OnComplete) == EventFlags.OnComplete)
                        activeFlags |= EventFlags.OnComplete;
                }

                ctx.Progress = time;
                ctx.ActiveFlags = activeFlags;

                Clocks[i] = clock;
                Contexts[i] = ctx;
            }
        }
    }
}
