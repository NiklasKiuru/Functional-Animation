using Aikom.FunctionalAnimation;
using System;
using UnityEngine;
using Unity.Mathematics;

namespace Aikom.FunctionalAnimation
{
    public class MultiFunctionGraph
    {
        private int _dimension;
        private RangedFunction[] _array;
        private int2[] _strides;

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

        internal Span<RangedFunction> GetFunctionsInternal() => _array.AsSpan();

        public MultiFunctionGraph(int dimension, RangedFunction[] array)
        {
            _dimension = dimension;
            _array = array;
        }

        public MultiFunctionGraph(int dimension)
        {
            _dimension = dimension;
            _strides = new int2[dimension];
            for(int i = 0; i < dimension; i++)
                _strides[i] = new int2(i, i + 1);
            _array = new RangedFunction[dimension];
        }

        public MultiFunctionGraph(RangedFunction[] funcs, params int[] stride)
        {
            _dimension = stride.Length;
            _array = new RangedFunction[_dimension * EFSettings.MaxFunctions];
            var index = 0;
            for(int i = 0; i < _dimension; i++)
            {
                for(int j = 0; j < stride[i]; j++)
                {
                    _array[i * EFSettings.MaxFunctions + j] = funcs[index];
                    index++;
                }
            }
        }

        /// <summary>
        /// Sets a function into the correct position
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="index"></param>
        /// <param name="func"></param>
        public void Set(int pos, int index, RangedFunction func)
        {
            var start = _strides[pos].x;
            _array[start + index] = func;
        }

        private int GetSubGraphLength(int pos)
        {
            var stride = _strides[pos];
            return stride.y - stride.x;
        }
    }
}

