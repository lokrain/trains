using System;
using System.Runtime.InteropServices;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Wire contract self-test for SegmentSpec16 packed layout and little-endian roundtrip.
    /// </summary>
    public static class SegmentSpec16SelfTest
    {
        public static bool Run()
        {
            if (Marshal.SizeOf<SegmentSpec16>() != SegmentSpec16.WireSize)
            {
                return false;
            }

            SegmentSpec16 input = new SegmentSpec16
            {
                Ax = 17,
                Ay = 33,
                Bx = 42,
                By = 77,
                Kind = SegmentKind.Bridge,
                Flags = SegmentFlags.None,
                SpeedClass = 9,
                SegmentId = new SegmentId(123456)
            };

            Span<byte> payload = stackalloc byte[SegmentSpec16.WireSize];
            if (!input.TryWriteLittleEndian(payload))
            {
                return false;
            }

            if (!SegmentSpec16.TryReadLittleEndian(payload, out SegmentSpec16 output))
            {
                return false;
            }

            return output.Ax == input.Ax
                && output.Ay == input.Ay
                && output.Bx == input.Bx
                && output.By == input.By
                && output.Kind == input.Kind
                && output.Flags == input.Flags
                && output.SpeedClass == input.SpeedClass
                && output.SegmentId == input.SegmentId;
        }
    }
}
