using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    public class IndexPool
    {
        private HashSet<int> _indeces = new();
        private Stack<int> _unusedIndeces;
        private int _count = 0;

        public IndexPool(int preAlloc)
        {
            _unusedIndeces = new Stack<int>(preAlloc);
        }

        public int GetNewId()
        {
            if(_unusedIndeces.TryPop(out var newIndex))
                return newIndex;
            _indeces.Add(_count++);
            return _count;
        }

        public void Return(int id)
        {
            if (_indeces.Contains(id))
            {
                _unusedIndeces.Push(id);
                _indeces.Remove(id);
            }
        }
    }
}

