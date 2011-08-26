/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
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

using System.IO;
using Gibbed.IO;

namespace Gibbed.Squish.DDS
{
    public class Header
    {
        public Header()
        {
            this.PixelFormat = new PixelFormat();
        }

        public uint GetSize()
        {
            return (18 * 4) + this.PixelFormat.GetSize() + (5 * 4);
        }

        public uint Size;
        public HeaderFlags Flags;
        public int Height;
        public int Width;
        public uint PitchOrLinearSize;
        public uint Depth;
        public uint MipMapCount;
        public byte[] Reserved1 = new byte[11 * 4];
        public PixelFormat PixelFormat;
        public uint SurfaceFlags;
        public uint CubemapFlags;
        public byte[] Reserved2 = new byte[3 * 4];

        public void Deserialize(Stream input, bool littleEndian)
        {
            this.Size = input.ReadValueU32(littleEndian);
            this.Flags = input.ReadValueEnum<HeaderFlags>(littleEndian);
            this.Height = input.ReadValueS32(littleEndian);
            this.Width = input.ReadValueS32(littleEndian);
            this.PitchOrLinearSize = input.ReadValueU32(littleEndian);
            this.Depth = input.ReadValueU32(littleEndian);
            this.MipMapCount = input.ReadValueU32(littleEndian);
            if (input.Read(this.Reserved1, 0, this.Reserved1.Length) != this.Reserved1.Length)
            {
                throw new EndOfStreamException();
            }
            this.PixelFormat.Deserialize(input, littleEndian);
            this.SurfaceFlags = input.ReadValueU32(littleEndian);
            this.CubemapFlags = input.ReadValueU32(littleEndian);
            if (input.Read(this.Reserved2, 0, this.Reserved2.Length) != this.Reserved2.Length)
            {
                throw new EndOfStreamException();
            }
        }
    }
}
