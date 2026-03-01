using Unity.Collections;

namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Self-test for tile-edge key/index O(1) insert/remove/query behavior.
    /// </summary>
    public static class RailSpatialIndexSelfTest
    {
        public static bool Run()
        {
            RailSpatialIndex index = RailSpatialIndex.Create(128, Allocator.Temp);
            try
            {
                if (!TileEdgeKey.TryFromUnitSegment(100, 200, 101, 200, out uint hKey))
                {
                    return false;
                }

                if (!TileEdgeKey.TryFromUnitSegment(100, 200, 100, 201, out uint vKey))
                {
                    return false;
                }

                SegmentId a = new(10);
                SegmentId b = new SegmentId(11);

                if (!index.TryAdd(hKey, a))
                {
                    return false;
                }

                if (!index.TryAdd(vKey, b))
                {
                    return false;
                }

                if (index.TryAdd(hKey, b))
                {
                    return false;
                }

                if (!index.TryGet(hKey, out SegmentId gotA) || gotA != a)
                {
                    return false;
                }

                if (!index.TryGet(vKey, out SegmentId gotB) || gotB != b)
                {
                    return false;
                }

                if (!index.Remove(hKey))
                {
                    return false;
                }

                if (index.TryGet(hKey, out _))
                {
                    return false;
                }

                return index.TryGet(vKey, out SegmentId stillB) && stillB == b;
            }
            finally
            {
                index.Dispose();
            }
        }
    }
}
