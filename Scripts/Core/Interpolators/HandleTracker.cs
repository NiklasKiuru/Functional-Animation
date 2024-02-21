
namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Referrable object for handles
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HandleTracker<T> : IInterpolatorHandle<T> 
        where T : unmanaged
    {
        private bool _isAlive;
        private FunctionContainer _container;

        private IInterpolator<T> _processor;
        bool IGroupProcessor.IsAlive { get => _isAlive; set => _isAlive = value; }
        int IGroupProcessor.Id { get => _processor.Id; set => _processor.Id = value; }

        internal HandleTracker(IInterpolator<T> proc, FunctionContainer cont)
        {
            _processor = proc;
            _isAlive = true;
            _container = cont;
        }

        /// <summary>
        /// IGroupProcessor group id implimentation
        /// </summary>
        /// <returns></returns>
        public int GetGroupId() => _processor.GetGroupId();

        /// <summary>
        /// Gets a realtime value of the interpolator
        /// </summary>
        /// <returns></returns>
        public T GetValue()
        {   
            // Since this is a reference type _isAlive is always up to date
            // where as _processor has no real alive state
            if(_isAlive) 
                return _processor.GetRealTimeValue();
            return 
                default;
        }

        /// <summary>
        /// Forces restart of the cached processor
        /// </summary>
        public void Restart()
        {
            _processor = _processor.ReRegister(_container);
            _isAlive = true;
        }
        
    }
}

