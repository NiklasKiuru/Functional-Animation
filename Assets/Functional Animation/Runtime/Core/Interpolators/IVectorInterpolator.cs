using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aikom.FunctionalAnimation
{
    public interface IVectorInterpolator<TSubtype, TBaseType> : IInterpolator<TBaseType>
        where TBaseType : unmanaged
        where TSubtype : unmanaged
    {
        public int AxisCount { get; }
        public void SetValue(int index, TSubtype value, ref TBaseType current);
        public int PointerCount(int index);
        public TSubtype InterpolateAxis(TBaseType from, TBaseType to, RangedFunction func, float time, int index);
    }
}
