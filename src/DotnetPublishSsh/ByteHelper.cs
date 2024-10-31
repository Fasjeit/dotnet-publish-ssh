using System;

namespace DotnetPublishSsh
{
    internal static class ByteHelperExtensions
    {
        internal static string ToHexString(this byte[] data)
        {
            return Convert.ToHexString(data);
        }

        internal static byte[] ReverseEndianness(this byte[] data)
        {
            Array.Reverse(data);
            return data;
        }
    }
}
