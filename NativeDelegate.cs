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

namespace Gibbed
{
    public static partial class Squish
    {
        private static class NativeDelegate
        {
            public delegate void CompressMasked(IntPtr rgba, int mask, IntPtr block, int flags, IntPtr metric);
            public delegate void Compress(IntPtr rgba, IntPtr block, int flags, IntPtr metric);
            public delegate void Decompress(IntPtr rgba, IntPtr block, int flags);
            public delegate int GetStorageRequirements(int width, int height, int flags);
            public delegate void CompressImage(IntPtr rgba, int width, int height, IntPtr blocks, int flags, IntPtr metric);
            public delegate void CompressImage2(IntPtr rgba, int width, int height, int pitch, IntPtr blocks, int flags, IntPtr metric);
            public delegate void DecompressImage(IntPtr rgba, int width, int height, IntPtr blocks, int flags);
            public delegate void DecompressImage2(IntPtr rgba, int width, int height, int pitch, IntPtr blocks, int flags);
            public delegate void ComputeMSE(IntPtr rgba, int width, int height, IntPtr dxt, int flags, out double colorMSE, out double alphaMSE);
            public delegate void ComputeMSE2(IntPtr rgba, int width, int height, int pitch, IntPtr dxt, int flags, out double colorMSE, out double alphaMSE);
        }
    }
}
