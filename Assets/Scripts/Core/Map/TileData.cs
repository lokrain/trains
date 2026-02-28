namespace OpenTTD.Core.Map
{
    public struct TileData
    {
        public byte Height; // Using a byte is memory-efficient for a 0-255 height range.
        public byte Type;   // e.g., 0=Grass, 1=Water, 2=Rock.
    }
}