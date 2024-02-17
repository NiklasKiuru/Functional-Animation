
namespace Aikom.FunctionalAnimation
{
    public class HandleTracker<T> : IInterpolatorHandle<T> 
        where T : unmanaged
    {
        private bool _isAlive;
        private FunctionContainer _container;

        private IInterpolator<T> _processor;
        bool IInterpolatorHandle<T>.IsAlive { get => _isAlive; set => _isAlive = value; }
        int IInterpolatorHandle<T>.Id { get => _processor.InternalId; set => _processor.InternalId = value; }
        public IInterpolator<T> Processor
        {
            get
            {
                return _processor;
            }
        }

        internal HandleTracker(IInterpolator<T> proc, FunctionContainer cont)
        {
            _processor = proc;
            _isAlive = true;
            _container = cont;
        }

        public int GetGroupId() => _processor.GetGroupId();
        public FunctionContainer GetCachedContainer() => _container;
        public T GetValue()
        {   
            if(_isAlive) 
                return _processor.GetValue();
            return 
                default;
        }

        /// <summary>
        /// Forces restart of the cached processor
        /// </summary>
        public void Restart()
        {
            _processor.Register(_container);
            _isAlive = true;
        }
        
    }
}

