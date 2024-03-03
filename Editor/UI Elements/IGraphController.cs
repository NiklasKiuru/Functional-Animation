
using System;

namespace Aikom.FunctionalAnimation.Editor
{
    public interface IGraphController
    {
        public GraphData GetSource();
        public void Refresh();

        public event Action OnFunctionRemoved;
    }
}