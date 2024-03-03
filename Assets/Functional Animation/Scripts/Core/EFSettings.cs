
namespace Aikom.FunctionalAnimation
{
    public struct EFSettings
    {
        private const int c_preallocSize = 256;
        private const int c_maxFuncBuffer = 8;

        /// <summary>
        /// Defines the size of a processor group on start up
        /// </summary>
        public static int GroupAllocationSize { get { return c_preallocSize; } }

        /// <summary>
        /// Size of the function buffer in graphs
        /// </summary>
        public static int MaxFunctions { get {  return c_maxFuncBuffer; } }
    }
}

