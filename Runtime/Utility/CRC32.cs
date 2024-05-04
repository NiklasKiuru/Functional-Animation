using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Aikom.FunctionalAnimation.Utility
{
    /// <summary>
    /// Performs 32-bit reversed cyclic redundancy checks.
    /// </summary>
    /// <remarks>
    /// Source: https://rosettacode.org/wiki/CRC-32#C.23
    /// </remarks>
    public class CRC32
    {
        /// <summary>
        /// Generator polynomial (modulo 2) for the reversed CRC32 algorithm. 
        /// </summary>
        private const uint s_generator = 0xEDB88320;

        /// <summary>
        /// Contains a cache of calculated checksum chunks.
        /// </summary>
        private static readonly uint[] m_checksumTable;

        static CRC32()
        {
            // Constructs the checksum lookup table. Used to optimize the checksum.
            m_checksumTable = Enumerable.Range(0, 256).Select(i =>
            {
                var tableEntry = (uint)i;
                for (var j = 0; j < 8; ++j)
                {
                    tableEntry = ((tableEntry & 1) != 0)
                        ? (s_generator ^ (tableEntry >> 1))
                        : (tableEntry >> 1);
                }
                return tableEntry;
            }).ToArray();
        }
        
        /// <summary>
        /// Calculates the checksum of the byte stream.
        /// </summary>
        /// <param name="byteStream">The byte stream to calculate the checksum for.</param>
        /// <returns>A 32-bit reversed checksum.</returns>
        public static uint Get<T>(IEnumerable<T> byteStream)
        {
            try
            {
                // Initialize checksumRegister to 0xFFFFFFFF and calculate the checksum.
                return ~byteStream.Aggregate(0xFFFFFFFF, (checksumRegister, currentByte) =>
                          (m_checksumTable[(checksumRegister & 0xFF) ^ Convert.ToByte(currentByte)] ^ (checksumRegister >> 8)));
            }
            catch (FormatException e)
            {
                throw new Exception("Could not read the stream out as bytes.", e);
            }
            catch (InvalidCastException e)
            {
                throw new Exception("Could not read the stream out as bytes.", e);
            }
            catch (OverflowException e)
            {
                throw new Exception("Could not read the stream out as bytes.", e);
            }
        }

        /// <summary>
        /// Calculates the checksum of the byte stream
        /// </summary>
        /// <param name="byteStream"></param>
        /// <returns></returns>
        public static uint Get(Span<byte> byteStream)
        {
            uint result = 0xFFFFFFFF;
            foreach(var b in byteStream)
            {
                result = m_checksumTable[(result & 0xFF) ^ Convert.ToByte(b)] ^ (result >> 8);
            }
            return ~result;
        }

    }
}
