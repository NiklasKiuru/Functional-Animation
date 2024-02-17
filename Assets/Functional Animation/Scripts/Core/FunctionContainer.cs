using Aikom.FunctionalAnimation;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class FunctionContainer
    {
        private int _dimension;
        private RangedFunction[] _array;

        public int Dimension { get { return _dimension; } }
        public int Length { get { return _array.Length; } }
        public RangedFunction this[int index]
        {
            get
            {
                return _array[index];
            }
            set
            {
                _array[index] = value;
            }
        }

        public RangedFunction this[int dimension, int index]
        {
            get
            {
                var start = (dimension - 1) * EFSettings.MaxFunctions;
                return _array[start + index];
            }
        }

        public FunctionContainer(int dimension, RangedFunction[] array)
        {
            if (array.Length != dimension * EFSettings.MaxFunctions)
                throw new System.Exception("The container dimension must match maximum allowed capacity");

            _dimension = dimension;
            _array = array;
        }

        public FunctionContainer(int dimension)
        {
            _dimension = dimension;
            _array = new RangedFunction[dimension * EFSettings.MaxFunctions];
        }

        public void Add(int pos, int index, RangedFunction func)
        {
            _array[(pos * EFSettings.MaxFunctions) + index] = func;
        }
    }
}

