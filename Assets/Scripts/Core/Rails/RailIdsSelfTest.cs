using System;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Self-test for rail id primitive semantics and serialization helpers.
    /// </summary>
    public static class RailIdsSelfTest
    {
        public static bool Run()
        {
            SegmentId s0 = SegmentId.Null;
            SegmentId s1 = new(10);
            SegmentId s2 = new(10);
            SegmentId s3 = new(11);

            if (s0.IsValid || !s1.IsValid)
            {
                return false;
            }

            if (s1 != s2 || s1 == s3)
            {
                return false;
            }

            if (s1.CompareTo(s3) >= 0 || s3.CompareTo(s1) <= 0)
            {
                return false;
            }

            NodeId n0 = NodeId.Null;
            NodeId n1 = new(5);
            NodeId n2 = new(6);
            if (n0.IsValid || !n1.IsValid || n1.CompareTo(n2) >= 0)
            {
                return false;
            }

            EdgeId e0 = EdgeId.Null;
            EdgeId e1 = new(22);
            EdgeId e2 = new(22);
            if (e0.IsValid || !e1.IsValid || e1 != e2)
            {
                return false;
            }

            Span<byte> buf = stackalloc byte[12];
            if (!RailIdCodec.TryWrite(s3, n2, e1, buf))
            {
                return false;
            }

            if (!RailIdCodec.TryRead(buf, out SegmentId rs, out NodeId rn, out EdgeId re))
            {
                return false;
            }

            return rs == s3 && rn == n2 && re == e1;
        }
    }
}
