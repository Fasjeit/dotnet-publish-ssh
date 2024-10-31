using System;
using System.Buffers.Binary;
using System.Linq;

namespace DotnetPublishBase
{
    public static class ByteHelperExtensions
    {
        internal static string ToHexString(this byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", string.Empty);
        }

        internal static byte[] ReverseEndianness(this byte[] data)
        {
            return data.Select(b => BinaryPrimitives.ReverseEndianness(b)).ToArray();
        }
    }
}
