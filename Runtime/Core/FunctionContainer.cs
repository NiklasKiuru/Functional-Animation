using Aikom.FunctionalAnimation;
using System;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class FunctionContainer : IDisposable
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

        public FunctionContainer(RangedFunction[] funcs, params int[] stride)
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
            _array[(pos * EFSettings.MaxFunctions) + index] = func;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}

