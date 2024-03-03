using Aikom.FunctionalAnimation.Utility;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    /// <summary>
    /// Custom indexable n x m generic matrix
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public struct MatrixRxC<TData> where TData : struct
    {
        private TData[][] _data;
        private int _rows;
        private int _columns;
        
        public int Rows => _rows;
        public int Columns => _columns;

        public MatrixRxC(int rows, int columns)
        {   
            _rows = rows;
            _columns = columns;
            _data = new TData[rows][];
            for (int i = 0; i < rows; i++)
            {
                _data[i] = new TData[columns];
            }
        }

        public int Length => _columns * _rows;

        public TData this[int row, int column]
        {
            get => _data[row][column];
            set => _data[row][column] = value;
        }

        public void SetRow(int row, TData[] data)
        {
            _data[row] = data;
        }

        public TData[] GetRow(int row)
        {
            return _data[row];
        }

        public void SetColumn(int column, TData[] data)
        {
            for (int i = 0; i < _rows; i++)
            {
                _data[i][column] = data[i];
            }
        }

        public TData[] GetColumn(int column)
        {
            var data = new TData[_rows];
            for (int i = 0; i < _rows; i++)
            {
                data[i] = _data[i][column];
            }
            return data;
        }
    }
}

