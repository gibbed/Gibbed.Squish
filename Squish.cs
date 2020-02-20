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
        [System.Flags]
        public enum Flags : int
        {
            None = 0,

            /// <summary>
            /// Use DXT1 compression.
            /// </summary>
            DXT1 = 1 << 0,

            /// <summary>
            /// Use DXT3 compression.
            /// </summary>
            DXT3 = 1 << 1,

            /// <summary>
            /// Use DXT5 compression.
            /// </summary>
            DXT5 = 1 << 2,

            /// <summary>
            /// Use BC4 compression.
            /// </summary>
            BC4 = 1 << 3,

            /// <summary>
            /// Use BC5 compression.
            /// </summary>
            BC5 = 1 << 4,

            /// <summary>
            /// Use a slow but high quality colour compressor (the default).
            /// </summary>
            ColourClusterFit = 1 << 5,

            /// <summary>
            /// Use a fast but low quality colour compressor.
            /// </summary>
            ColourRangeFit = 1 << 6,

            /// <summary>
            /// Weight the colour by alpha during cluster fit (disabled by default).
            /// </summary>
            WeightColourByAlpha = 1 << 7,

            /// <summary>
            /// Use a very slow but very high quality colour compressor.
            /// </summary>
            ColourIterativeClusterFit = 1 << 8,

            /// <summary>
            /// Source is BGRA rather than RGBA.
            /// </summary>
            SourceBGRA = 1 << 9,
        }

        private static bool Is64Bit()
        {
            return Marshal.SizeOf(IntPtr.Zero) == 8;
        }

        private static NativeDelegates Delegates;

        static Squish()
        {
            if (Is64Bit() == false)
            {
                NativeDelegates delegates;
                delegates.CompressMasked = Native32.CompressMasked;
                delegates.Compress = Native32.Compress;
                delegates.Decompress = Native32.Decompress;
                delegates.GetStorageRequirements = Native32.GetStorageRequirements;
                delegates.CompressImage = Native32.CompressImage;
                delegates.CompressImage2 = Native32.CompressImage2;
                delegates.DecompressImage = Native32.DecompressImage;
                delegates.DecompressImage2 = Native32.DecompressImage2;
                delegates.ComputeMSE = Native32.ComputeMSE;
                delegates.ComputeMSE2 = Native32.ComputeMSE2;
                Delegates = delegates;
            }
            else
            {
                NativeDelegates delegates;
                delegates.CompressMasked = Native64.CompressMasked;
                delegates.Compress = Native64.Compress;
                delegates.Decompress = Native64.Decompress;
                delegates.GetStorageRequirements = Native64.GetStorageRequirements;
                delegates.CompressImage = Native64.CompressImage;
                delegates.CompressImage2 = Native64.CompressImage2;
                delegates.DecompressImage = Native64.DecompressImage;
                delegates.DecompressImage2 = Native64.DecompressImage2;
                delegates.ComputeMSE = Native64.ComputeMSE;
                delegates.ComputeMSE2 = Native64.ComputeMSE2;
                Delegates = delegates;
            }
        }

        /// <summary>
        /// Compresses a 4x4 block of pixels.
        ///
        /// The source pixels should be presented as a contiguous array of 16 rgba
        /// values, with each component as 1 byte each. In memory this should be:
        ///
        ///   { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
        ///
        /// The mask parameter enables only certain pixels within the block. The lowest
        /// bit enables the first pixel and so on up to the 16th bit. Bits beyond the
        /// 16th bit are ignored. Pixels that are not enabled are allowed to take
        /// arbitrary colours in the output block. An example of how this can be used
        /// is in the CompressImage function to disable pixels outside the bounds of
        /// the image when the width or height is not divisible by 4.
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for the compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        /// </summary>
        /// <param name="rgba">The rgba values of the 16 source pixels.</param>
        /// <param name="mask">The valid pixel mask.</param>
        /// <param name="block">Storage for the compressed DXT block.</param>
        /// <param name="flags">Compression flags.</param>
        public static void CompressMasked(byte[] rgba, int mask, byte[] block, Flags flags)
        {
            CompressMasked(rgba, 0, mask, block, 0, flags, null, 0);
        }

        /// <summary>
        /// Compresses a 4x4 block of pixels.
        ///
        /// The source pixels should be presented as a contiguous array of 16 rgba
        /// values, with each component as 1 byte each. In memory this should be:
        ///
        ///   { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
        ///
        /// The mask parameter enables only certain pixels within the block. The lowest
        /// bit enables the first pixel and so on up to the 16th bit. Bits beyond the
        /// 16th bit are ignored. Pixels that are not enabled are allowed to take
        /// arbitrary colours in the output block. An example of how this can be used
        /// is in the CompressImage function to disable pixels outside the bounds of
        /// the image when the width or height is not divisible by 4.
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for the compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        ///
        /// The metric parameter can be used to weight the relative importance of each
        /// colour channel, or pass NULL to use the default uniform weight of
        /// { 1.0f, 1.0f, 1.0f }. This replaces the previous flag-based control that
        /// allowed either uniform or "perceptual" weights with the fixed values
        /// { 0.2126f, 0.7152f, 0.0722f }. If non-NULL, the metric should point to a
        /// contiguous array of 3 floats.
        /// </summary>
        /// <param name="rgba">The rgba values of the 16 source pixels.</param>
        /// <param name="mask">The valid pixel mask.</param>
        /// <param name="block">Storage for the compressed DXT block.</param>
        /// <param name="flags">Compression flags.</param>
        /// <param name="metric">An optional perceptual metric.</param>
        public static void CompressMasked(byte[] rgba, int mask, byte[] block, Flags flags, float[] metric)
        {
            CompressMasked(rgba, 0, mask, block, 0, flags, metric, 0);
        }

        /// <summary>
        /// Compresses a 4x4 block of pixels.
        ///
        /// The source pixels should be presented as a contiguous array of 16 rgba
        /// values, with each component as 1 byte each. In memory this should be:
        ///
        ///   { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
        ///
        /// The mask parameter enables only certain pixels within the block. The lowest
        /// bit enables the first pixel and so on up to the 16th bit. Bits beyond the
        /// 16th bit are ignored. Pixels that are not enabled are allowed to take
        /// arbitrary colours in the output block. An example of how this can be used
        /// is in the CompressImage function to disable pixels outside the bounds of
        /// the image when the width or height is not divisible by 4.
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for the compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        /// </summary>
        /// <param name="rgba">The rgba values of the 16 source pixels.</param>
        /// <param name="rgbaOffset">An index into the rgba values.</param>
        /// <param name="mask">The valid pixel mask.</param>
        /// <param name="block">Storage for the compressed DXT block.</param>
        /// <param name="blockOffset">An index into the storage for the compressed DXT block.</param>
        /// <param name="flags">Compression flags.</param>
        public static void CompressMasked(byte[] rgba, int rgbaOffset, int mask, byte[] block, int blockOffset, Flags flags)
        {
            CompressMasked(rgba, rgbaOffset, mask, block, blockOffset, flags, null, 0);
        }

        /// <summary>
        /// Compresses a 4x4 block of pixels.
        ///
        /// The source pixels should be presented as a contiguous array of 16 rgba
        /// values, with each component as 1 byte each. In memory this should be:
        ///
        ///   { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
        ///
        /// The mask parameter enables only certain pixels within the block. The lowest
        /// bit enables the first pixel and so on up to the 16th bit. Bits beyond the
        /// 16th bit are ignored. Pixels that are not enabled are allowed to take
        /// arbitrary colours in the output block. An example of how this can be used
        /// is in the CompressImage function to disable pixels outside the bounds of
        /// the image when the width or height is not divisible by 4.
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for the compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        ///
        /// The metric parameter can be used to weight the relative importance of each
        /// colour channel, or pass NULL to use the default uniform weight of
        /// { 1.0f, 1.0f, 1.0f }. This replaces the previous flag-based control that
        /// allowed either uniform or "perceptual" weights with the fixed values
        /// { 0.2126f, 0.7152f, 0.0722f }. If non-NULL, the metric should point to a
        /// contiguous array of 3 floats.
        /// </summary>
        /// <param name="rgba">The rgba values of the 16 source pixels.</param>
        /// <param name="rgbaOffset">An index into the rgba values.</param>
        /// <param name="mask">The valid pixel mask.</param>
        /// <param name="block">Storage for the compressed DXT block.</param>
        /// <param name="blockOffset">An index into the storage for the compressed DXT block.</param>
        /// <param name="flags">Compression flags.</param>
        /// <param name="metric">An optional perceptual metric.</param>
        /// <param name="metricOffset">An index into the optional perceptual metric.</param>
        public static void CompressMasked(byte[] rgba, int rgbaOffset, int mask, byte[] block, int blockOffset, Flags flags, float[] metric, int metricOffset)
        {
            if (rgba == null)
            {
                throw new ArgumentNullException(nameof(rgba));
            }

            const int rgbaSize = 16 * 4;
            if (rgbaOffset < 0 || rgbaOffset + rgbaSize > rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            var blockSize = GetStorageRequirements(4, 4, flags);
            if (blockOffset < 0 || blockOffset + blockSize > block.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(blockOffset));
            }

            if (metric != null)
            {
                const int metricSize = 3;
                if (metricOffset < 0 || metricOffset + metricSize > metric.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(metricOffset));
                }
            }

            GCHandle rgbaHandle = default;
            GCHandle blockHandle = default;
            GCHandle metricHandle = default;
            try
            {
                rgbaHandle = GCHandle.Alloc(rgba, GCHandleType.Pinned);
                var rgbaAddress = rgbaHandle.AddrOfPinnedObject() + rgbaOffset;

                blockHandle = GCHandle.Alloc(block, GCHandleType.Pinned);
                var blockAddress = blockHandle.AddrOfPinnedObject() + blockOffset;

                IntPtr metricAddress;
                if (metric == null)
                {
                    metricAddress = IntPtr.Zero;
                }
                else
                {
                    metricHandle = GCHandle.Alloc(metric, GCHandleType.Pinned);
                    metricAddress = metricHandle.AddrOfPinnedObject() + metricOffset;
                }

                Delegates.CompressMasked(rgbaAddress, mask, blockAddress, (int)flags, metricAddress);
            }
            finally
            {
                if (metricHandle.IsAllocated == true)
                {
                    metricHandle.Free();
                }

                if (blockHandle.IsAllocated == true)
                {
                    blockHandle.Free();
                }

                if (rgbaHandle.IsAllocated == true)
                {
                    rgbaHandle.Free();
                }
            }
        }

        /// <summary>
        /// Compresses a 4x4 block of pixels.
        ///
        /// The source pixels should be presented as a contiguous array of 16 rgba
        /// values, with each component as 1 byte each. In memory this should be:
        ///
        ///     { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for the compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        ///
        /// This method is an inline that calls CompressMasked with a mask of 0xffff,
        /// provided for compatibility with older versions of Squish.
        /// </summary>
        /// <param name="rgba">The rgba values of the 16 source pixels.</param>
        /// <param name="block">Storage for the compressed DXT block.</param>
        /// <param name="flags">Compression flags.</param>
        public static void Compress(byte[] rgba, byte[] block, Flags flags)
        {
            Compress(rgba, 0, block, 0, flags, null, 0);
        }

        /// <summary>
        /// Compresses a 4x4 block of pixels.
        ///
        /// The source pixels should be presented as a contiguous array of 16 rgba
        /// values, with each component as 1 byte each. In memory this should be:
        ///
        ///     { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for the compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        ///
        /// The metric parameter can be used to weight the relative importance of each
        /// colour channel, or pass NULL to use the default uniform weight of
        /// { 1.0f, 1.0f, 1.0f }. This replaces the previous flag-based control that
        /// allowed either uniform or "perceptual" weights with the fixed values
        /// { 0.2126f, 0.7152f, 0.0722f }. If non-NULL, the metric should point to a
        /// contiguous array of 3 floats.
        ///
        /// This method is an inline that calls CompressMasked with a mask of 0xffff,
        /// provided for compatibility with older versions of Squish.
        /// </summary>
        /// <param name="rgba">The rgba values of the 16 source pixels.</param>
        /// <param name="block">Storage for the compressed DXT block.</param>
        /// <param name="flags">Compression flags.</param>
        /// <param name="metric">An optional perceptual metric.</param>
        public static void Compress(byte[] rgba, byte[] block, Flags flags, float[] metric)
        {
            Compress(rgba, 0, block, 0, flags, metric, 0);
        }

        /// <summary>
        /// Compresses a 4x4 block of pixels.
        ///
        /// The source pixels should be presented as a contiguous array of 16 rgba
        /// values, with each component as 1 byte each. In memory this should be:
        ///
        ///     { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for the compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        ///
        /// This method is an inline that calls CompressMasked with a mask of 0xffff,
        /// provided for compatibility with older versions of Squish.
        /// </summary>
        /// <param name="rgba">The rgba values of the 16 source pixels.</param>
        /// <param name="rgbaOffset">An index into the rgba values.</param>
        /// <param name="block">Storage for the compressed DXT block.</param>
        /// <param name="blockOffset">An index into the storage for the compressed DXT block.</param>
        /// <param name="flags">Compression flags.</param>
        public static void Compress(byte[] rgba, int rgbaOffset, byte[] block, int blockOffset, Flags flags)
        {
            Compress(rgba, rgbaOffset, block, blockOffset, flags, null, 0);
        }

        /// <summary>
        /// Compresses a 4x4 block of pixels.
        ///
        /// The source pixels should be presented as a contiguous array of 16 rgba
        /// values, with each component as 1 byte each. In memory this should be:
        ///
        ///     { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for the compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        ///
        /// The metric parameter can be used to weight the relative importance of each
        /// colour channel, or pass NULL to use the default uniform weight of
        /// { 1.0f, 1.0f, 1.0f }. This replaces the previous flag-based control that
        /// allowed either uniform or "perceptual" weights with the fixed values
        /// { 0.2126f, 0.7152f, 0.0722f }. If non-NULL, the metric should point to a
        /// contiguous array of 3 floats.
        ///
        /// This method is an inline that calls CompressMasked with a mask of 0xffff,
        /// provided for compatibility with older versions of Squish.
        /// </summary>
        /// <param name="rgba">The rgba values of the 16 source pixels.</param>
        /// <param name="rgbaOffset">An index into the rgba values.</param>
        /// <param name="block">Storage for the compressed DXT block.</param>
        /// <param name="blockOffset">An index into the storage for the compressed DXT block.</param>
        /// <param name="flags">Compression flags.</param>
        /// <param name="metric">An optional perceptual metric.</param>
        /// <param name="metricOffset">An index into the optional perceptual metric.</param>
        public static void Compress(byte[] rgba, int rgbaOffset, byte[] block, int blockOffset, Flags flags, float[] metric, int metricOffset)
        {
            if (rgba == null)
            {
                throw new ArgumentNullException(nameof(rgba));
            }

            const int rgbaSize = 16 * 4;
            if (rgbaOffset < 0 || rgbaOffset + rgbaSize > rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            var blockSize = GetStorageRequirements(4, 4, flags);
            if (blockOffset < 0 || blockOffset + blockSize > block.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(blockOffset));
            }

            if (metric != null)
            {
                const int metricSize = 3;
                if (metricOffset < 0 || metricOffset + metricSize > metric.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(metricOffset));
                }
            }

            GCHandle rgbaHandle = default;
            GCHandle blockHandle = default;
            GCHandle metricHandle = default;
            try
            {
                rgbaHandle = GCHandle.Alloc(rgba, GCHandleType.Pinned);
                var rgbaAddress = rgbaHandle.AddrOfPinnedObject() + rgbaOffset;

                blockHandle = GCHandle.Alloc(block, GCHandleType.Pinned);
                var blockAddress = blockHandle.AddrOfPinnedObject() + blockOffset;

                IntPtr metricAddress;
                if (metric == null)
                {
                    metricAddress = IntPtr.Zero;
                }
                else
                {
                    metricHandle = GCHandle.Alloc(metric, GCHandleType.Pinned);
                    metricAddress = metricHandle.AddrOfPinnedObject() + metricOffset;
                }

                Delegates.Compress(rgbaAddress, blockAddress, (int)flags, metricAddress);
            }
            finally
            {
                if (metricHandle.IsAllocated == true)
                {
                    metricHandle.Free();
                }

                if (blockHandle.IsAllocated == true)
                {
                    blockHandle.Free();
                }

                if (rgbaHandle.IsAllocated == true)
                {
                    rgbaHandle.Free();
                }
            }
        }

        /// <summary>
        /// Decompresses a 4x4 block of pixels.
        ///
        /// The decompressed pixels will be written as a contiguous array of 16 rgba
        /// values, with each component as 1 byte each. In memory this is:
        ///
        ///     { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
        ///
        /// The flags parameter should specify DXT, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. All other flags
        /// are ignored.
        /// </summary>
        /// <param name="rgba">Storage for the 16 decompressed pixels.</param>
        /// <param name="block">The compressed DXT block.</param>
        /// <param name="flags">Compression flags.</param>
        public static void Decompress(byte[] rgba, byte[] block, Flags flags)
        {
            Decompress(rgba, 0, block, 0, flags);
        }

        /// <summary>
        /// Decompresses a 4x4 block of pixels.
        ///
        /// The decompressed pixels will be written as a contiguous array of 16 rgba
        /// values, with each component as 1 byte each. In memory this is:
        ///
        ///     { r1, g1, b1, a1, .... , r16, g16, b16, a16 }
        ///
        /// The flags parameter should specify DXT, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. All other flags
        /// are ignored.
        /// </summary>
        /// <param name="rgba">Storage for the 16 decompressed pixels.</param>
        /// <param name="rgbaOffset">An index into the storage for the 16 decompressed pixels.</param>
        /// <param name="block">The compressed DXT block.</param>
        /// <param name="blockOffset">An index into the compressed DXT block.</param>
        /// <param name="flags">Compression flags.</param>
        public static void Decompress(byte[] rgba, int rgbaOffset, byte[] block, int blockOffset, Flags flags)
        {
            if (rgba == null)
            {
                throw new ArgumentNullException(nameof(rgba));
            }

            const int rgbaSize = 16 * 4;
            if (rgbaOffset < 0 || rgbaOffset + rgbaSize > rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            var blockSize = GetStorageRequirements(4, 4, flags);
            if (blockOffset < 0 || blockOffset + blockSize > block.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(blockOffset));
            }

            GCHandle rgbaHandle = default;
            GCHandle blockHandle = default;
            try
            {
                rgbaHandle = GCHandle.Alloc(rgba, GCHandleType.Pinned);
                var rgbaAddress = rgbaHandle.AddrOfPinnedObject() + rgbaOffset;

                blockHandle = GCHandle.Alloc(block, GCHandleType.Pinned);
                var blockAddress = blockHandle.AddrOfPinnedObject() + blockOffset;
                Delegates.Decompress(rgbaAddress, blockAddress, (int)flags);
            }
            finally
            {
                if (blockHandle.IsAllocated == true)
                {
                    blockHandle.Free();
                }

                if (rgbaHandle.IsAllocated == true)
                {
                    rgbaHandle.Free();
                }
            }
        }

        /// <summary>
        /// Computes the amount of compressed storage required.
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. All other flags
        /// are ignored.
        ///
        /// Most DXT images will be a multiple of 4 in each dimension, but this
        /// function supports arbitrary size images by allowing the outer blocks to
        /// be only partially used.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="flags">Compression flags.</param>
        /// <returns>Amount of compressed storage required.</returns>
        public static int GetStorageRequirements(int width, int height, Flags flags)
        {
            return Delegates.GetStorageRequirements(width, height, (int)flags);
        }

        /// <summary>
        /// Compresses an image in memory.
        ///
        /// The source pixels should be presented as a contiguous array of width*height
        /// rgba values, with each component as 1 byte each. In memory this should be:
        ///
        ///     { r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width*height
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for each compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        ///
        /// Internally this function calls CompressMasked for each block, which
        /// allows for pixels outside the image to take arbitrary values. The function
        /// GetStorageRequirements can be called to compute the amount of memory
        /// to allocate for the compressed output.
        ///
        /// Note on compression quality: When compressing textures with
        /// Squish it is recommended to apply a gamma-correction
        /// beforehand. This will reduce the blockiness in dark areas. The
        /// level of necessary gamma-correction is platform dependent. For
        /// example, a gamma correction with gamma = 0.5 before compression
        /// and gamma = 2.0 after decompression yields good results on the
        /// Windows platform but for other platforms like MacOS X a different
        /// gamma value may be more suitable.
        /// </summary>
        /// <param name="rgba">The pixels of the source.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="blocks">Storage for the compressed output.</param>
        /// <param name="flags">Compression flags.</param>
        public static void CompressImage(byte[] rgba, int width, int height, byte[] blocks, Flags flags)
        {
            CompressImage(rgba, 0, width, height, blocks, 0, flags, null, 0);
        }

        /// <summary>
        /// Compresses an image in memory.
        ///
        /// The source pixels should be presented as a contiguous array of width*height
        /// rgba values, with each component as 1 byte each. In memory this should be:
        ///
        ///     { r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width*height
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for each compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        ///
        /// The metric parameter can be used to weight the relative importance of each
        /// colour channel, or pass null to use the default uniform weight of
        /// { 1.0f, 1.0f, 1.0f }. This replaces the previous flag-based control that
        /// allowed either uniform or "perceptual" weights with the fixed values
        /// { 0.2126f, 0.7152f, 0.0722f }. If non-null, the metric should point to a
        /// contiguous array of 3 floats.
        ///
        /// Internally this function calls CompressMasked for each block, which
        /// allows for pixels outside the image to take arbitrary values. The function
        /// GetStorageRequirements can be called to compute the amount of memory
        /// to allocate for the compressed output.
        ///
        /// Note on compression quality: When compressing textures with
        /// Squish it is recommended to apply a gamma-correction
        /// beforehand. This will reduce the blockiness in dark areas. The
        /// level of necessary gamma-correction is platform dependent. For
        /// example, a gamma correction with gamma = 0.5 before compression
        /// and gamma = 2.0 after decompression yields good results on the
        /// Windows platform but for other platforms like MacOS X a different
        /// gamma value may be more suitable.
        /// </summary>
        /// <param name="rgba">The pixels of the source.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="blocks">Storage for the compressed output.</param>
        /// <param name="flags">Compression flags.</param>
        /// <param name="metric">An optional perceptual metric.</param>
        public static void CompressImage(byte[] rgba, int width, int height, byte[] blocks, Flags flags, float[] metric)
        {
            CompressImage(rgba, 0, width, height, blocks, 0, flags, metric, 0);
        }

        /// <summary>
        /// Compresses an image in memory.
        ///
        /// The source pixels should be presented as a contiguous array of width*height
        /// rgba values, with each component as 1 byte each. In memory this should be:
        ///
        ///     { r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width*height
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for each compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        ///
        /// Internally this function calls CompressMasked for each block, which
        /// allows for pixels outside the image to take arbitrary values. The function
        /// GetStorageRequirements can be called to compute the amount of memory
        /// to allocate for the compressed output.
        ///
        /// Note on compression quality: When compressing textures with
        /// Squish it is recommended to apply a gamma-correction
        /// beforehand. This will reduce the blockiness in dark areas. The
        /// level of necessary gamma-correction is platform dependent. For
        /// example, a gamma correction with gamma = 0.5 before compression
        /// and gamma = 2.0 after decompression yields good results on the
        /// Windows platform but for other platforms like MacOS X a different
        /// gamma value may be more suitable.
        /// </summary>
        /// <param name="rgba">The pixels of the source.</param>
        /// <param name="rgbaOffset">An index into the pixels of the source.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="blocks">Storage for the compressed output.</param>
        /// <param name="blocksOffset">An index into the storage for the compressed output.</param>
        /// <param name="flags">Compression flags.</param>
        public static void CompressImage(byte[] rgba, int rgbaOffset, int width, int height, byte[] blocks, int blocksOffset, Flags flags)
        {
            CompressImage(rgba, rgbaOffset, width, height, blocks, blocksOffset, flags, null, 0);
        }

        /// <summary>
        /// Compresses an image in memory.
        ///
        /// The source pixels should be presented as a contiguous array of width*height
        /// rgba values, with each component as 1 byte each. In memory this should be:
        ///
        ///     { r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width*height
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for each compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        ///
        /// The metric parameter can be used to weight the relative importance of each
        /// colour channel, or pass null to use the default uniform weight of
        /// { 1.0f, 1.0f, 1.0f }. This replaces the previous flag-based control that
        /// allowed either uniform or "perceptual" weights with the fixed values
        /// { 0.2126f, 0.7152f, 0.0722f }. If non-null, the metric should point to a
        /// contiguous array of 3 floats.
        ///
        /// Internally this function calls CompressMasked for each block, which
        /// allows for pixels outside the image to take arbitrary values. The function
        /// GetStorageRequirements can be called to compute the amount of memory
        /// to allocate for the compressed output.
        ///
        /// Note on compression quality: When compressing textures with
        /// Squish it is recommended to apply a gamma-correction
        /// beforehand. This will reduce the blockiness in dark areas. The
        /// level of necessary gamma-correction is platform dependent. For
        /// example, a gamma correction with gamma = 0.5 before compression
        /// and gamma = 2.0 after decompression yields good results on the
        /// Windows platform but for other platforms like MacOS X a different
        /// gamma value may be more suitable.
        /// </summary>
        /// <param name="rgba">The pixels of the source.</param>
        /// <param name="rgbaOffset">An index into the pixels of the source.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="blocks">Storage for the compressed output.</param>
        /// <param name="blocksOffset">An index into the storage for the compressed output.</param>
        /// <param name="flags">Compression flags.</param>
        /// <param name="metric">An optional perceptual metric.</param>
        /// <param name="metricOffset">An index into the optional perceptual metric.</param>
        public static void CompressImage(byte[] rgba, int rgbaOffset, int width, int height, byte[] blocks, int blocksOffset, Flags flags, float[] metric, int metricOffset)
        {
            if (rgba == null)
            {
                throw new ArgumentNullException(nameof(rgba));
            }

            if (rgbaOffset < 0 || rgbaOffset >= rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            var rgbaSize = height * width * 4;
            if (rgbaOffset + rgbaSize > rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            if (blocks == null)
            {
                throw new ArgumentNullException(nameof(blocks));
            }

            if (blocksOffset < 0 || blocksOffset >= blocks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(blocksOffset));
            }

            var blocksSize = GetStorageRequirements(width, height, flags);
            if (blocksOffset + blocksSize > blocks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(blocksOffset));
            }

            if (metric != null)
            {
                if (metricOffset < 0 || metricOffset + 3 > metric.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(metricOffset));
                }
            }

            GCHandle rgbaHandle = default;
            GCHandle blocksHandle = default;
            GCHandle metricHandle = default;
            try
            {
                rgbaHandle = GCHandle.Alloc(rgba, GCHandleType.Pinned);
                var rgbaAddress = rgbaHandle.AddrOfPinnedObject() + rgbaOffset;

                blocksHandle = GCHandle.Alloc(blocks, GCHandleType.Pinned);
                var blocksAddress = blocksHandle.AddrOfPinnedObject() + blocksOffset;

                IntPtr metricAddress;
                if (metric == null)
                {
                    metricAddress = IntPtr.Zero;
                }
                else
                {
                    metricHandle = GCHandle.Alloc(metric, GCHandleType.Pinned);
                    metricAddress = metricHandle.AddrOfPinnedObject() + metricOffset;
                }

                Delegates.CompressImage(rgbaAddress, width, height, blocksAddress, (int)flags, metricAddress);
            }
            finally
            {
                if (metricHandle.IsAllocated == true)
                {
                    metricHandle.Free();
                }

                if (blocksHandle.IsAllocated == true)
                {
                    blocksHandle.Free();
                }

                if (rgbaHandle.IsAllocated == true)
                {
                    rgbaHandle.Free();
                }
            }
        }

        /// <summary>
        /// Compresses an image in memory.
        ///
        /// The source pixels should be presented as a contiguous array of width*height
        /// rgba values, with each component as 1 byte each. In memory this should be:
        ///
        ///     { r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width*height
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for each compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        ///
        /// Internally this function calls CompressMasked for each block, which
        /// allows for pixels outside the image to take arbitrary values. The function
        /// GetStorageRequirements can be called to compute the amount of memory
        /// to allocate for the compressed output.
        ///
        /// Note on compression quality: When compressing textures with
        /// Squish it is recommended to apply a gamma-correction
        /// beforehand. This will reduce the blockiness in dark areas. The
        /// level of necessary gamma-correction is platform dependent. For
        /// example, a gamma correction with gamma = 0.5 before compression
        /// and gamma = 2.0 after decompression yields good results on the
        /// Windows platform but for other platforms like MacOS X a different
        /// gamma value may be more suitable.
        /// </summary>
        /// <param name="rgba">The pixels of the source.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="pitch">The pitch of the source image.</param>
        /// <param name="blocks">Storage for the compressed output.</param>
        /// <param name="flags">Compression flags.</param>
        public static void CompressImage(byte[] rgba, int width, int height, int pitch, byte[] blocks, Flags flags)
        {
            CompressImage(rgba, 0, width, height, pitch, blocks, 0, flags, null, 0);
        }

        /// <summary>
        /// Compresses an image in memory.
        ///
        /// The source pixels should be presented as a contiguous array of width*height
        /// rgba values, with each component as 1 byte each. In memory this should be:
        ///
        ///     { r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width*height
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for each compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        ///
        /// The metric parameter can be used to weight the relative importance of each
        /// colour channel, or pass null to use the default uniform weight of
        /// { 1.0f, 1.0f, 1.0f }. This replaces the previous flag-based control that
        /// allowed either uniform or "perceptual" weights with the fixed values
        /// { 0.2126f, 0.7152f, 0.0722f }. If non-null, the metric should point to a
        /// contiguous array of 3 floats.
        ///
        /// Internally this function calls CompressMasked for each block, which
        /// allows for pixels outside the image to take arbitrary values. The function
        /// GetStorageRequirements can be called to compute the amount of memory
        /// to allocate for the compressed output.
        ///
        /// Note on compression quality: When compressing textures with
        /// Squish it is recommended to apply a gamma-correction
        /// beforehand. This will reduce the blockiness in dark areas. The
        /// level of necessary gamma-correction is platform dependent. For
        /// example, a gamma correction with gamma = 0.5 before compression
        /// and gamma = 2.0 after decompression yields good results on the
        /// Windows platform but for other platforms like MacOS X a different
        /// gamma value may be more suitable.
        /// </summary>
        /// <param name="rgba">The pixels of the source.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="pitch">The pitch of the source image.</param>
        /// <param name="blocks">Storage for the compressed output.</param>
        /// <param name="flags">Compression flags.</param>
        /// <param name="metric">An optional perceptual metric.</param>
        public static void CompressImage(byte[] rgba, int width, int height, int pitch, byte[] blocks, Flags flags, float[] metric)
        {
            CompressImage(rgba, 0, width, height, pitch, blocks, 0, flags, metric, 0);
        }

        /// <summary>
        /// Compresses an image in memory.
        ///
        /// The source pixels should be presented as a contiguous array of width*height
        /// rgba values, with each component as 1 byte each. In memory this should be:
        ///
        ///     { r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width*height
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for each compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        ///
        /// Internally this function calls CompressMasked for each block, which
        /// allows for pixels outside the image to take arbitrary values. The function
        /// GetStorageRequirements can be called to compute the amount of memory
        /// to allocate for the compressed output.
        ///
        /// Note on compression quality: When compressing textures with
        /// Squish it is recommended to apply a gamma-correction
        /// beforehand. This will reduce the blockiness in dark areas. The
        /// level of necessary gamma-correction is platform dependent. For
        /// example, a gamma correction with gamma = 0.5 before compression
        /// and gamma = 2.0 after decompression yields good results on the
        /// Windows platform but for other platforms like MacOS X a different
        /// gamma value may be more suitable.
        /// </summary>
        /// <param name="rgba">The pixels of the source.</param>
        /// <param name="rgbaOffset">An index into the pixels of the source.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="pitch">The pitch of the source image.</param>
        /// <param name="blocks">Storage for the compressed output.</param>
        /// <param name="blocksOffset">An index into the storage for the compressed output.</param>
        /// <param name="flags">Compression flags.</param>
        public static void CompressImage(byte[] rgba, int rgbaOffset, int width, int height, int pitch, byte[] blocks, int blocksOffset, Flags flags)
        {
            CompressImage(rgba, rgbaOffset, width, height, pitch, blocks, blocksOffset, flags, null, 0);
        }

        /// <summary>
        /// Compresses an image in memory.
        ///
        /// The source pixels should be presented as a contiguous array of width*height
        /// rgba values, with each component as 1 byte each. In memory this should be:
        ///
        ///     { r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width*height
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. When using DXT1
        /// compression, 8 bytes of storage are required for each compressed DXT block.
        /// DXT3 and DXT5 compression require 16 bytes of storage per block.
        ///
        /// The flags parameter can also specify a preferred colour compressor to use
        /// when fitting the RGB components of the data. Possible colour compressors
        /// are: ColourClusterFit (the default), ColourRangeFit (very fast, low
        /// quality) or ColourIterativeClusterFit (slowest, best quality).
        ///
        /// When using ColourClusterFit or ColourIterativeClusterFit, an additional
        /// flag can be specified to weight the importance of each pixel by its alpha
        /// value. For images that are rendered using alpha blending, this can
        /// significantly increase the perceived quality.
        ///
        /// The metric parameter can be used to weight the relative importance of each
        /// colour channel, or pass null to use the default uniform weight of
        /// { 1.0f, 1.0f, 1.0f }. This replaces the previous flag-based control that
        /// allowed either uniform or "perceptual" weights with the fixed values
        /// { 0.2126f, 0.7152f, 0.0722f }. If non-null, the metric should point to a
        /// contiguous array of 3 floats.
        ///
        /// Internally this function calls CompressMasked for each block, which
        /// allows for pixels outside the image to take arbitrary values. The function
        /// GetStorageRequirements can be called to compute the amount of memory
        /// to allocate for the compressed output.
        ///
        /// Note on compression quality: When compressing textures with
        /// Squish it is recommended to apply a gamma-correction
        /// beforehand. This will reduce the blockiness in dark areas. The
        /// level of necessary gamma-correction is platform dependent. For
        /// example, a gamma correction with gamma = 0.5 before compression
        /// and gamma = 2.0 after decompression yields good results on the
        /// Windows platform but for other platforms like MacOS X a different
        /// gamma value may be more suitable.
        /// </summary>
        /// <param name="rgba">The pixels of the source.</param>
        /// <param name="rgbaOffset">An index into the pixels of the source.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="pitch">The pitch of the source image.</param>
        /// <param name="blocks">Storage for the compressed output.</param>
        /// <param name="blocksOffset">An index into the storage for the compressed output.</param>
        /// <param name="flags">Compression flags.</param>
        /// <param name="metric">An optional perceptual metric.</param>
        /// <param name="metricOffset">An index into the optional perceptual metric.</param>
        public static void CompressImage(byte[] rgba, int rgbaOffset, int width, int height, int pitch, byte[] blocks, int blocksOffset, Flags flags, float[] metric, int metricOffset)
        {
            if (rgba == null)
            {
                throw new ArgumentNullException(nameof(rgba));
            }

            if (rgbaOffset < 0 || rgbaOffset >= rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            var rgbaSize = height * pitch;
            if (rgbaOffset + rgbaSize > rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            if (blocks == null)
            {
                throw new ArgumentNullException(nameof(blocks));
            }

            if (blocksOffset < 0 || blocksOffset >= blocks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(blocksOffset));
            }

            var blocksSize = GetStorageRequirements(width, height, flags);
            if (blocksOffset + blocksSize > blocks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(blocksOffset));
            }

            if (metric != null)
            {
                if (metricOffset < 0 || metricOffset + 3 > metric.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(metricOffset));
                }
            }

            GCHandle rgbaHandle = default;
            GCHandle blocksHandle = default;
            GCHandle metricHandle = default;
            try
            {
                rgbaHandle = GCHandle.Alloc(rgba, GCHandleType.Pinned);
                var rgbaAddress = rgbaHandle.AddrOfPinnedObject() + rgbaOffset;

                blocksHandle = GCHandle.Alloc(blocks, GCHandleType.Pinned);
                var blocksAddress = blocksHandle.AddrOfPinnedObject() + blocksOffset;

                IntPtr metricAddress;
                if (metric == null)
                {
                    metricAddress = IntPtr.Zero;
                }
                else
                {
                    metricHandle = GCHandle.Alloc(metric, GCHandleType.Pinned);
                    metricAddress = metricHandle.AddrOfPinnedObject() + metricOffset;
                }

                Delegates.CompressImage2(rgbaAddress, width, height, pitch, blocksAddress, (int)flags, metricAddress);
            }
            finally
            {
                if (metricHandle.IsAllocated == true)
                {
                    metricHandle.Free();
                }

                if (blocksHandle.IsAllocated == true)
                {
                    blocksHandle.Free();
                }

                if (rgbaHandle.IsAllocated == true)
                {
                    rgbaHandle.Free();
                }
            }
        }

        /// <summary>
        /// Decompresses an image in memory.
        ///
        /// The decompressed pixels will be written as a contiguous array of width*height
        /// 16 rgba values, with each component as 1 byte each. In memory this is:
        ///
        ///     { r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width*height
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. All other flags
        /// are ignored.
        ///
        /// Internally this function calls Decompress for each block.
        /// </summary>
        /// <param name="rgba">Storage for the decompressed pixels.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="blocks">The compressed DXT blocks.</param>
        /// <param name="flags">Compression flags.</param>
        public static void DecompressImage(byte[] rgba, int width, int height, byte[] blocks, Flags flags)
        {
            DecompressImage(rgba, 0, width, height, blocks, 0, flags);
        }

        /// <summary>
        /// Decompresses an image in memory.
        ///
        /// The decompressed pixels will be written as a contiguous array of width*height
        /// 16 rgba values, with each component as 1 byte each. In memory this is:
        ///
        ///     { r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width*height
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. All other flags
        /// are ignored.
        ///
        /// Internally this function calls Decompress for each block.
        /// </summary>
        /// <param name="rgba">Storage for the decompressed pixels.</param>
        /// <param name="rgbaOffset">An index into the storage for the decompressed pixels.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="blocks">The compressed DXT blocks.</param>
        /// <param name="blocksOffset">An index into the compressed DXT blocks.</param>
        /// <param name="flags">Compression flags.</param>
        public static void DecompressImage(byte[] rgba, int rgbaOffset, int width, int height, byte[] blocks, int blocksOffset, Flags flags)
        {
            if (rgba == null)
            {
                throw new ArgumentNullException(nameof(rgba));
            }

            if (rgbaOffset < 0 || rgbaOffset >= rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            var rgbaSize = height * width * 4;
            if (rgbaOffset + rgbaSize > rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            if (blocks == null)
            {
                throw new ArgumentNullException(nameof(blocks));
            }

            if (blocksOffset < 0 || blocksOffset >= blocks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(blocksOffset));
            }

            var blocksSize = GetStorageRequirements(width, height, flags);
            if (blocksOffset + blocksSize > blocks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(blocksOffset));
            }

            GCHandle rgbaHandle = default;
            GCHandle blocksHandle = default;
            try
            {
                rgbaHandle = GCHandle.Alloc(rgba, GCHandleType.Pinned);
                var rgbaAddress = rgbaHandle.AddrOfPinnedObject() + rgbaOffset;

                blocksHandle = GCHandle.Alloc(blocks, GCHandleType.Pinned);
                var blocksAddress = blocksHandle.AddrOfPinnedObject() + blocksOffset;

                Delegates.DecompressImage(rgbaAddress, width, height, blocksAddress, (int)flags);
            }
            finally
            {
                if (blocksHandle.IsAllocated == true)
                {
                    blocksHandle.Free();
                }

                if (rgbaHandle.IsAllocated == true)
                {
                    rgbaHandle.Free();
                }
            }
        }

        /// <summary>
        /// Decompresses an image in memory.
        ///
        /// The decompressed pixels will be written as a contiguous array of width*height
        /// 16 rgba values, with each component as 1 byte each. In memory this is:
        ///
        ///     { r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width*height
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. All other flags
        /// are ignored.
        ///
        /// Internally this function calls Decompress for each block.
        /// </summary>
        /// <param name="rgba">Storage for the decompressed pixels.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="pitch">The pitch of the decompressed pixels.</param>
        /// <param name="blocks">The compressed DXT blocks.</param>
        /// <param name="flags">Compression flags.</param>
        public static void DecompressImage(byte[] rgba, int width, int height, int pitch, byte[] blocks, Flags flags)
        {
            DecompressImage(rgba, width, height, pitch, blocks, flags);
        }

        /// <summary>
        /// Decompresses an image in memory.
        ///
        /// The decompressed pixels will be written as a contiguous array of width*height
        /// 16 rgba values, with each component as 1 byte each. In memory this is:
        ///
        ///     { r1, g1, b1, a1, .... , rn, gn, bn, an } for n = width*height
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. All other flags
        /// are ignored.
        ///
        /// Internally this function calls Decompress for each block.
        /// </summary>
        /// <param name="rgba">Storage for the decompressed pixels.</param>
        /// <param name="rgbaOffset">An index into the storage for the decompressed pixels.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="pitch">The pitch of the decompressed pixels.</param>
        /// <param name="blocks">The compressed DXT blocks.</param>
        /// <param name="blocksOffset">An index into the compressed DXT blocks.</param>
        /// <param name="flags">Compression flags.</param>
        public static void DecompressImage(byte[] rgba, int rgbaOffset, int width, int height, int pitch, byte[] blocks, int blocksOffset, Flags flags)
        {
            if (rgba == null)
            {
                throw new ArgumentNullException(nameof(rgba));
            }

            if (rgbaOffset < 0 || rgbaOffset >= rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            var rgbaSize = height * pitch;
            if (rgbaOffset + rgbaSize > rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            if (blocks == null)
            {
                throw new ArgumentNullException(nameof(blocks));
            }

            if (blocksOffset < 0 || blocksOffset >= blocks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(blocksOffset));
            }

            var blocksSize = GetStorageRequirements(width, height, flags);
            if (blocksOffset + blocksSize > blocks.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(blocksOffset));
            }

            GCHandle rgbaHandle = default;
            GCHandle blocksHandle = default;
            try
            {
                rgbaHandle = GCHandle.Alloc(rgba, GCHandleType.Pinned);
                var rgbaAddress = rgbaHandle.AddrOfPinnedObject() + rgbaOffset;

                blocksHandle = GCHandle.Alloc(blocks, GCHandleType.Pinned);
                var blocksAddress = blocksHandle.AddrOfPinnedObject() + blocksOffset;

                Delegates.DecompressImage2(rgbaAddress, width, height, pitch, blocksAddress, (int)flags);
            }
            finally
            {
                if (blocksHandle.IsAllocated == true)
                {
                    blocksHandle.Free();
                }

                if (rgbaHandle.IsAllocated == true)
                {
                    rgbaHandle.Free();
                }
            }
        }

        /// <summary>
        /// Computes MSE of an compressed image in memory.
        ///
        /// The colour MSE and alpha MSE are computed across all pixels. The colour MSE is
        /// averaged across all rgb values (i.e. colourMSE = sum sum_k ||dxt.k - rgba.k||/3)
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. All other flags
        /// are ignored.
        ///
        /// Internally this function calls Decompress for each block.
        /// </summary>
        /// <param name="rgba">The original image pixels.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="dxt">The compressed dxt blocks</param>
        /// <param name="flags">Compression flags.</param>
        /// <param name="colorMSE">The MSE of the colour values.</param>
        /// <param name="alphaMSE">The MSE of the alpha values.</param>
        public static void ComputeMSE(byte[] rgba, int width, int height, byte[] dxt, Flags flags, out double colorMSE, out double alphaMSE)
        {
            ComputeMSE(rgba, 0, width, height, dxt, 0, flags, out colorMSE, out alphaMSE);
        }

        /// <summary>
        /// Computes MSE of an compressed image in memory.
        ///
        /// The colour MSE and alpha MSE are computed across all pixels. The colour MSE is
        /// averaged across all rgb values (i.e. colourMSE = sum sum_k ||dxt.k - rgba.k||/3)
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. All other flags
        /// are ignored.
        ///
        /// Internally this function calls Decompress for each block.
        /// </summary>
        /// <param name="rgba">The original image pixels.</param>
        /// <param name="rgbaOffset">An index into the original image pixels.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="dxt">The compressed dxt blocks</param>
        /// <param name="dxtOffset">An index into the compressed dxt blocks</param>
        /// <param name="flags">Compression flags.</param>
        /// <param name="colorMSE">The MSE of the colour values.</param>
        /// <param name="alphaMSE">The MSE of the alpha values.</param>
        public static void ComputeMSE(byte[] rgba, int rgbaOffset, int width, int height, byte[] dxt, int dxtOffset, Flags flags, out double colorMSE, out double alphaMSE)
        {
            if (rgba == null)
            {
                throw new ArgumentNullException(nameof(rgba));
            }

            if (rgbaOffset < 0 || rgbaOffset >= rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            var rgbaSize = height * width * 4;
            if (rgbaOffset + rgbaSize > rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            if (dxt == null)
            {
                throw new ArgumentNullException(nameof(dxt));
            }

            if (dxtOffset < 0 || dxtOffset >= dxt.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(dxtOffset));
            }

            var dxtSize = GetStorageRequirements(width, height, flags);
            if (dxtOffset + dxtSize > dxt.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(dxtOffset));
            }

            GCHandle rgbaHandle = default;
            GCHandle dxtHandle = default;
            try
            {
                rgbaHandle = GCHandle.Alloc(rgba, GCHandleType.Pinned);
                var rgbaAddress = rgbaHandle.AddrOfPinnedObject() + rgbaOffset;

                dxtHandle = GCHandle.Alloc(dxt, GCHandleType.Pinned);
                var dxtAddress = dxtHandle.AddrOfPinnedObject() + dxtOffset;

                Delegates.ComputeMSE(rgbaAddress, width, height, dxtAddress, (int)flags, out colorMSE, out alphaMSE);
            }
            finally
            {
                if (dxtHandle.IsAllocated == true)
                {
                    dxtHandle.Free();
                }

                if (rgbaHandle.IsAllocated == true)
                {
                    rgbaHandle.Free();
                }
            }
        }

        /// <summary>
        /// Computes MSE of an compressed image in memory.
        ///
        /// The colour MSE and alpha MSE are computed across all pixels. The colour MSE is
        /// averaged across all rgb values (i.e. colourMSE = sum sum_k ||dxt.k - rgba.k||/3)
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. All other flags
        /// are ignored.
        ///
        /// Internally this function calls Decompress for each block.
        /// </summary>
        /// <param name="rgba">The original image pixels.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="pitch">The pitch of the source image.</param>
        /// <param name="dxt">The compressed dxt blocks</param>
        /// <param name="flags">Compression flags.</param>
        /// <param name="colorMSE">The MSE of the colour values.</param>
        /// <param name="alphaMSE">The MSE of the alpha values.</param>
        public static void ComputeMSE(byte[] rgba, int width, int height, int pitch, byte[] dxt, Flags flags, out double colorMSE, out double alphaMSE)
        {
            ComputeMSE(rgba, 0, width, height, pitch, dxt, 0, flags, out colorMSE, out alphaMSE);
        }

        /// <summary>
        /// Computes MSE of an compressed image in memory.
        ///
        /// The colour MSE and alpha MSE are computed across all pixels. The colour MSE is
        /// averaged across all rgb values (i.e. colourMSE = sum sum_k ||dxt.k - rgba.k||/3)
        ///
        /// The flags parameter should specify DXT1, DXT3, DXT5, BC4, or BC5 compression,
        /// however, DXT1 will be used by default if none is specified. All other flags
        /// are ignored.
        ///
        /// Internally this function calls Decompress for each block.
        /// </summary>
        /// <param name="rgba">The original image pixels.</param>
        /// <param name="rgbaOffset">An index into the original image pixels.</param>
        /// <param name="width">The width of the source image.</param>
        /// <param name="height">The height of the source image.</param>
        /// <param name="pitch">The pitch of the source image.</param>
        /// <param name="dxt">The compressed dxt blocks</param>
        /// <param name="dxtOffset">An index into the compressed dxt blocks</param>
        /// <param name="flags">Compression flags.</param>
        /// <param name="colorMSE">The MSE of the colour values.</param>
        /// <param name="alphaMSE">The MSE of the alpha values.</param>
        public static void ComputeMSE(byte[] rgba, int rgbaOffset, int width, int height, int pitch, byte[] dxt, int dxtOffset, Flags flags, out double colorMSE, out double alphaMSE)
        {
            if (rgba == null)
            {
                throw new ArgumentNullException(nameof(rgba));
            }

            if (rgbaOffset < 0 || rgbaOffset >= rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            var rgbaSize = height * pitch;
            if (rgbaOffset + rgbaSize > rgba.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(rgbaOffset));
            }

            if (dxt == null)
            {
                throw new ArgumentNullException(nameof(dxt));
            }

            if (dxtOffset < 0 || dxtOffset >= dxt.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(dxtOffset));
            }

            var dxtSize = GetStorageRequirements(width, height, flags);
            if (dxtOffset + dxtSize > dxt.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(dxtOffset));
            }

            GCHandle rgbaHandle = default;
            GCHandle dxtHandle = default;
            try
            {
                rgbaHandle = GCHandle.Alloc(rgba, GCHandleType.Pinned);
                var rgbaAddress = rgbaHandle.AddrOfPinnedObject() + rgbaOffset;

                dxtHandle = GCHandle.Alloc(dxt, GCHandleType.Pinned);
                var dxtAddress = dxtHandle.AddrOfPinnedObject() + dxtOffset;

                Delegates.ComputeMSE2(rgbaAddress, width, height, pitch, dxtAddress, (int)flags, out colorMSE, out alphaMSE);
            }
            finally
            {
                if (dxtHandle.IsAllocated == true)
                {
                    dxtHandle.Free();
                }

                if (rgbaHandle.IsAllocated == true)
                {
                    rgbaHandle.Free();
                }
            }
        }
    }
}
