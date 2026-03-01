using System;
using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Self-test for rail wire contract compatibility primitives.
    /// </summary>
    public static class RailWireContractSelfTest
    {
        public static bool Run()
        {
            SegmentSpec16 spec = new()
            {
                Ax = 1,
                Ay = 2,
                Bx = 3,
                By = 4,
                Kind = SegmentKind.Tunnel,
                Flags = SegmentFlags.None,
                SpeedClass = 5,
                SegmentId = new SegmentId(999)
            };

            Span<byte> payload = stackalloc byte[SegmentSpec16.WireSize + 4];
            payload[SegmentSpec16.WireSize + 0] = 0xAA;
            payload[SegmentSpec16.WireSize + 1] = 0xBB;
            payload[SegmentSpec16.WireSize + 2] = 0xCC;
            payload[SegmentSpec16.WireSize + 3] = 0xDD;

            if (!spec.TryWriteLittleEndian(payload.Slice(0, SegmentSpec16.WireSize)))
            {
                return false;
            }

            if (!SegmentSpec16.TryReadLittleEndian(payload.Slice(0, SegmentSpec16.WireSize), out SegmentSpec16 decoded))
            {
                return false;
            }

            if (decoded.SegmentId != spec.SegmentId
                || decoded.Ax != spec.Ax
                || decoded.Ay != spec.Ay
                || decoded.Bx != spec.Bx
                || decoded.By != spec.By
                || decoded.Kind != spec.Kind
                || decoded.Flags != spec.Flags
                || decoded.SpeedClass != spec.SpeedClass)
            {
                return false;
            }

            return payload[SegmentSpec16.WireSize + 0] == 0xAA
                && payload[SegmentSpec16.WireSize + 1] == 0xBB
                && payload[SegmentSpec16.WireSize + 2] == 0xCC
                && payload[SegmentSpec16.WireSize + 3] == 0xDD;
        }
    }
}
