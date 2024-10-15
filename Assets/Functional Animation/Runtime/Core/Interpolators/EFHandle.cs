namespace Aikom.FunctionalAnimation
{
    public struct EFHandle<TStruct, TProcessor> : IInterpolatorHandle<TStruct, TProcessor>
        where TStruct : unmanaged
        where TProcessor : unmanaged, IInterpolator<TStruct>
    {
        private Process _processId;
        Process IInterpolatorHandle<TStruct, TProcessor>.ProcessId { get => _processId; set => _processId = value; }

        public EFHandle(Process pid)
        {
            _processId = pid;
        }
    }
}
