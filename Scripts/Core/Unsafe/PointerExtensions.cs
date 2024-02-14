// Credits to meetem from https://meetemq.com/2023/01/14/using-pointers-in-c-unity/
// for this pointer utility class

namespace Aikom.FunctionalAnimation.Unsafe
{
    using System;
    using System.Runtime.CompilerServices;

    // Token: 0x02000002 RID: 2
    public static class PointerLibExtension
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void* GetPointer(this object obj)
        {
            return null;
        }

        // Token: 0x06000002 RID: 2 RVA: 0x00002054 File Offset: 0x00000254
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static void* GetInternalPointer(this object obj)
        {
            return null;
        }
    }
}

