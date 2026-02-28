#nullable enable
using System.Text;

namespace OpenTTD.Core.Net.Protocol
{
    /// <summary>
    /// Deterministic CRC64 known-vector self-test utility.
    /// Intended for CI smoke hooks and startup diagnostics.
    /// </summary>
    public static class Crc64SelfTest
    {
        private const ulong Ecma123456789 = 0x6C40DF5F0B497347ul;

        /// <summary>
        /// Validates CRC64 implementation against known ECMA vectors.
        /// </summary>
        public static bool Run()
        {
            if (Crc64.Compute(new byte[0]) != 0ul)
            {
                return false;
            }

            byte[] bytes = Encoding.ASCII.GetBytes("123456789");
            if (Crc64.Compute(bytes) != Ecma123456789)
            {
                return false;
            }

            return true;
        }
    }
}
