/* Copyright (c) 2020 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Runtime.InteropServices;

namespace Gibbed
{
    public static partial class Squish
    {
        private sealed class Native32
        {
            public const string DllName = "squish32";

            [DllImport(DllName, EntryPoint = "CompressMasked", CallingConvention = CallingConvention.Cdecl)]
            public static extern void CompressMasked(IntPtr rgba, int mask, IntPtr block, int flags, IntPtr metric);

            [DllImport(DllName, EntryPoint = "Compress", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Compress(IntPtr rgba, IntPtr block, int flags, IntPtr metric);

            [DllImport(DllName, EntryPoint = "Decompress", CallingConvention = CallingConvention.Cdecl)]
            public static extern void Decompress(IntPtr rgba, IntPtr block, int flags);

            [DllImport(DllName, EntryPoint = "GetStorageRequirements", CallingConvention = CallingConvention.Cdecl)]
            public static extern int GetStorageRequirements(int width, int height, int flags);

            [DllImport(DllName, EntryPoint = "CompressImage", CallingConvention = CallingConvention.Cdecl)]
            public static extern void CompressImage(IntPtr rgba, int width, int height, IntPtr blocks, int flags, IntPtr metric);

            [DllImport(DllName, EntryPoint = "CompressImage2", CallingConvention = CallingConvention.Cdecl)]
            public static extern void CompressImage2(IntPtr rgba, int width, int height, int pitch, IntPtr blocks, int flags, IntPtr metric);

            [DllImport(DllName, EntryPoint = "DecompressImage", CallingConvention = CallingConvention.Cdecl)]
            public static extern void DecompressImage(IntPtr rgba, int width, int height, IntPtr blocks, int flags);

            [DllImport(DllName, EntryPoint = "DecompressImage2", CallingConvention = CallingConvention.Cdecl)]
            public static extern void DecompressImage2(IntPtr rgba, int width, int height, int pitch, IntPtr blocks, int flags);

            [DllImport(DllName, EntryPoint = "ComputeMSE", CallingConvention = CallingConvention.Cdecl)]
            public static extern void ComputeMSE(IntPtr rgba, int width, int height, IntPtr dxt, int flags, out double colorMSE, out double alphaMSE);

            [DllImport(DllName, EntryPoint = "ComputeMSE2", CallingConvention = CallingConvention.Cdecl)]
            public static extern void ComputeMSE2(IntPtr rgba, int width, int height, int pitch, IntPtr dxt, int flags, out double colorMSE, out double alphaMSE);
        }
    }
}
