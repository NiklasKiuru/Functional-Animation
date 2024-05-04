using Aikom.FunctionalAnimation.Utility;
using System;
using System.Text;
using UnityEngine;

namespace Aikom.FunctionalAnimation
{
    [Serializable]
    public struct FunctionAlias
    {
        [SerializeField] private string _value;
        [SerializeField] private uint _hash;

        /// <summary>
        /// Crc32 hash of this alias
        /// </summary>
        public uint Hash { get => _hash; }

        public string Value { get => _value; }

        /// <summary>
        /// Creates a new alias for a function from a name or overload
        /// </summary>
        /// <param name="overloadOrName"></param>
        public FunctionAlias(string overloadOrName)
        {   
            _value = overloadOrName;
            _hash = GetHash(overloadOrName);
        }

        /// <summary>
        /// Creates an alias from function enum
        /// </summary>
        /// <param name="func"></param>
        internal FunctionAlias(Function func)
        {
            _value = func.ToString();
            _hash = GetHash(_value);
        }

        public static uint GetHash(string str)
        {
            Span<byte> bytes = stackalloc byte[str.Length];
            for (int i = 0; i < str.Length; i++)
                bytes[i] = (byte)str[i];
            return CRC32.Get(bytes);
        }
    }
}
