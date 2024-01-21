using System;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{   
    /// <summary>
    /// Container for used editor functions
    /// </summary>
    [Serializable]
    public class FunctionConstructor : ICloneable
    {
        [SerializeField] private FunctionWrapper[] _functions = new FunctionWrapper[0];

        /// <summary>
        /// Lenght of the function array this wrapper generates the final function from
        /// </summary>
        public int Length { get => _functions.Length; }

        /// <summary>
        /// Implimentation of the ICloneable interface
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var copy = new FunctionConstructor();
            copy._functions = new FunctionWrapper[_functions.Length];
            for(int i = 0; i < _functions.Length; i++)
            {
                copy._functions[i] = new FunctionWrapper();
                copy._functions[i]._function = _functions[i]._function;
                copy._functions[i]._invert = _functions[i]._invert;
            }
            return copy;
        }

        /// <summary>
        /// Generates a function from the selected functions in the editor. If the array is empty, a linear function is returned
        /// </summary>
        /// <returns></returns>
        public Func<float, float> Generate()
        {
            if(_functions.Length == 0)
                return EditorFunctions.Funcs[Function.Linear];
            if(_functions.Length == 1)
            {   
                var func = EditorFunctions.Funcs[_functions[0]._function];
                if (_functions[0]._invert)
                    return EF.Invert(func);
                else
                    return EditorFunctions.Funcs[_functions[0]._function];
            }
                

            var funcs = new Func<float, float>[_functions.Length];

            for(int i = 0; i < _functions.Length; i++)
            {
                if (_functions[i]._invert)
                    funcs[i] = EF.Invert(EditorFunctions.Funcs[_functions[i]._function]);
                else
                    funcs[i] = EditorFunctions.Funcs[_functions[i]._function];
            }

            return EF.Combine(funcs);
        }

        [Serializable]
        private class FunctionWrapper
        {
            [Tooltip("Used function type")]
            [SerializeField] internal Function _function;

            [Tooltip("Uninverted functions always start with value v = 0 at their respective linear time t = 0 and inverted functions return 1 - v at any time t")]
            [SerializeField] internal bool _invert;
        }
    }
}
