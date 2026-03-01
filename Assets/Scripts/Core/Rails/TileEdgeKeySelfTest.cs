namespace OpenTTD.Core.Rails
{
    /// <summary>
    /// Self-test for tile-edge key packing and canonical unit-segment mapping.
    /// </summary>
    public static class TileEdgeKeySelfTest
    {
        public static bool Run()
        {
            uint keyH = TileEdgeKey.Pack(123, 456, TileEdgeKey.Dir.Horizontal);
            TileEdgeKey.Unpack(keyH, out uint uxH, out uint uyH, out TileEdgeKey.Dir dirH);
            if (uxH != 123 || uyH != 456 || dirH != TileEdgeKey.Dir.Horizontal)
            {
                return false;
            }

            uint keyV = TileEdgeKey.Pack(777, 11, TileEdgeKey.Dir.Vertical);
            TileEdgeKey.Unpack(keyV, out uint uxV, out uint uyV, out TileEdgeKey.Dir dirV);
            if (uxV != 777 || uyV != 11 || dirV != TileEdgeKey.Dir.Vertical)
            {
                return false;
            }

            if (!TileEdgeKey.TryFromUnitSegment(10, 20, 11, 20, out uint fwdH)
                || !TileEdgeKey.TryFromUnitSegment(11, 20, 10, 20, out uint revH)
                || fwdH != revH)
            {
                return false;
            }

            if (!TileEdgeKey.TryFromUnitSegment(30, 40, 30, 41, out uint fwdV)
                || !TileEdgeKey.TryFromUnitSegment(30, 41, 30, 40, out uint revV)
                || fwdV != revV)
            {
                return false;
            }

            if (TileEdgeKey.TryFromUnitSegment(1, 1, 3, 1, out _))
            {
                return false;
            }

            if (TileEdgeKey.TryFromUnitSegment(5, 5, 6, 6, out _))
            {
                return false;
            }

            return true;
        }
    }
}
