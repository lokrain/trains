#nullable enable
using System;

namespace OpenTTD.Core.World
{
    /// <summary>
    /// Per-chunk dirty flags used to schedule recompute, snapshot, and render work.
    /// </summary>
    [Flags]
    public enum ChunkDirtyFlags : byte
    {
        None = 0,
        Height = 1 << 0,
        Derived = 1 << 1,
        Snapshot = 1 << 2,
        Render = 1 << 3
    }
}
