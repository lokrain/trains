# Binary Protocol v1 — Byte-Level Layout (Snapshots + Rect Patches)

*Target:* Unity Transport + custom messages  
*Locked:* little-endian, versioned, fragmentation supported  
*Reference:* Unity Jobs docs, Unity Burst docs (for zero-GC parsing patterns), Unity Entities docs.

---

## 0. Goals
- Stable: forward-compatible via protocol version + message version.
- Fast: single-pass parsing and minimal copies.
- Safe: bounds-checked lengths and explicit fragmentation.
- Deterministic: versioning avoids out-of-order ambiguity.

---

## 1. Endianness & Encoding
- All integers: little-endian.
- Signed deltas: `i8`.
- Variable-length integers: not used in v1.
- Payload compression:
  - `codec=1` = LZ4 block (recommended).

---

## 2. Packet Envelope (common)
Every datagram payload begins with:

| Offset | Type | Name | Notes |
|---:|---|---|---|
| 0 | `u16` | `proto_ver` | `=1` |
| 2 | `u16` | `msg_type` | enum in section 3 |
| 4 | `u32` | `msg_len` | bytes including header |
| 8 | `u32` | `msg_id` | per-lane monotonic for diagnostics/idempotency |
| 12 | `u32` | `flags` | reserved (`bit0=fragmented`) |
| 16 | bytes | `body[msg_len-16]` | message-specific |

Constraints:
- `msg_len >= 16`
- drop when `msg_len > datagram_payload_len`

v1 note:
- RO reliability is transport-managed.
- Keep envelope minimal; add custom ack fields only in a future protocol revision.

---

## 3. Message Types (v1)

| `msg_type` | Name | Lane |
|---:|---|---|
| 1 | `ClientHello` | RO |
| 2 | `ServerHello` | RO |
| 10 | `ChunkSnapshotFrag` | RO |
| 11 | `HeightRectPatch` | RO |
| 12 | `ResyncChunkRequest` | RO |
| 20 | `BuildRailCmd` | RO |
| 21 | `RailSegmentsAdded` | RO |
| 30 | `TrainState` | US |

---

## 4. Handshake Messages

### 4.1 `ClientHello` (`msg_type=1`)
Body:

| Offset | Type | Name |
|---:|---|---|
| 0 | `u32` | `client_build` |
| 4 | `u32` | `cap_flags` |

`cap_flags` (v1):
- `bit0`: supports LZ4
- `bit1`: supports RLE patches

### 4.2 `ServerHello` (`msg_type=2`)
Body:

| Offset | Type | Name |
|---:|---|---|
| 0 | `u32` | `world_gen_version` |
| 4 | `u64` | `world_seed` |
| 12 | `u16` | `map_w` (`2048`) |
| 14 | `u16` | `map_h` (`2048`) |
| 16 | `u16` | `chunk_size` (`64`) |
| 18 | `u8` | `sea_level` |
| 19 | `u8` | `reserved0` |
| 20 | `u32` | `server_tick_rate_hz` (`30`) |
| 24 | `u32` | `reserved1` |

---

## 5. Chunk Snapshot Fragmentation

### 5.1 `ChunkSnapshotFrag` (`msg_type=10`)
Body:

| Offset | Type | Name |
|---:|---|---|
| 0 | `i16` | `cx` |
| 2 | `i16` | `cy` |
| 4 | `u32` | `snapshot_id` |
| 8 | `u16` | `frag_index` |
| 10 | `u16` | `frag_count` |
| 12 | `u16` | `frag_len` |
| 14 | `u16` | `codec` (`1=LZ4`) |
| 16 | bytes | `frag_payload[frag_len]` |

Constraints:
- `frag_index < frag_count`
- `frag_len == body_len - 16`
- reassembly key: `(cx, cy, snapshot_id)`

### 5.2 Snapshot payload (after reassembly, before decompression)
Decompressed layout:

| Offset | Type | Name |
|---:|---|---|
| 0 | `u32` | `snapshot_id` |
| 4 | `u8` | `payload_ver` (`=1`) |
| 5 | `u8` | `fields_mask` (`bit0=height`, `bit1=river`, `bit2=biome`) |
| 6 | `u16` | `reserved` |
| 8 | bytes | `height_u8[4096]` (if bit0) |
| ... | bytes | `river_mask_u8[4096]` (if bit1) |
| ... | bytes | `biome_u8[4096]` (if bit2) |

v1 baseline recommendation:
- `fields_mask = 0b0000_0111` for join snapshots.

---

## 6. Height Rect Patch (Terraform)

### 6.1 `HeightRectPatch` (`msg_type=11`)
Body:

| Offset | Type | Name |
|---:|---|---|
| 0 | `i16` | `cx` |
| 2 | `i16` | `cy` |
| 4 | `u32` | `base_snapshot_id` |
| 8 | `u32` | `new_snapshot_id` |
| 12 | `u8` | `rx` |
| 13 | `u8` | `ry` |
| 14 | `u8` | `rw` |
| 15 | `u8` | `rh` |
| 16 | `u8` | `patch_codec` (`0=abs_u8`, `1=delta_i8`, `2=rle_delta`) |
| 17 | `u8` | `reserved0` |
| 18 | `u16` | `payload_len` |
| 20 | bytes | `payload[payload_len]` |

Constraints:
- `rx + rw <= 64`
- `ry + rh <= 64`
- apply only when local `snapshot_id == base_snapshot_id`, else request resync.

### 6.2 Patch payload formats
`codec=0` (`abs_u8`):
- `rw*rh` bytes absolute heights (row-major).

`codec=1` (`delta_i8`):
- `rw*rh` bytes signed deltas (row-major).
- apply `height = clamp_u8(height + delta)`.

`codec=2` (`rle_delta`):
- repeated runs of:
  - `u16 run_len`
  - `i8 delta`
- until `rw*rh` tiles covered.

---

## 7. Resync Request

### 7.1 `ResyncChunkRequest` (`msg_type=12`)
Body:

| Offset | Type | Name |
|---:|---|---|
| 0 | `i16` | `cx` |
| 2 | `i16` | `cy` |
| 4 | `u32` | `client_snapshot_id` |

Server response:
- latest `ChunkSnapshotFrag` sequence for chunk.

---

## 8. Rail Commands / Events (minimal v1)

### 8.1 `SegmentSpec16` (SIMD-friendly fixed record)
Used by `BuildRailCmd` and `RailSegmentsAdded`.

| Offset | Type | Name | Notes |
|---:|---|---|---|
| 0 | `u16` | `ax` | tile coordinate |
| 2 | `u16` | `ay` | tile coordinate |
| 4 | `u16` | `bx` | tile coordinate |
| 6 | `u16` | `by` | tile coordinate |
| 8 | `u8` | `kind` | straight/bridge/tunnel (v1 minimal) |
| 9 | `u8` | `flags` | reserved |
| 10 | `u16` | `cost_class` | speed/cost class |
| 12 | `u32` | `segment_id` | stable authoritative SegmentId |

Size:
- 16 bytes exactly.

Rationale:
- predictable alignment
- memcpy-friendly bulk encode/decode
- vectorization-friendly record size

### 8.2 `BuildRailCmd` (`msg_type=20`)
Body:

| Offset | Type | Name |
|---:|---|---|
| 0 | `u32` | `cmd_id` |
| 4 | `u32` | `tick` |
| 8 | `u16` | `segment_count` |
| 10 | `u16` | `reserved0` |
| 12 | bytes | `segments[segment_count * 16]` |

### 8.3 `RailSegmentsAdded` (`msg_type=21`)
Body:

| Offset | Type | Name |
|---:|---|---|
| 0 | `u16` | `segment_count` |
| 2 | `u16` | `reserved0` |
| 4 | bytes | `segments[segment_count * 16]` |

### 8.4 Rail encode/decode implementation notes
- Encode path: write message header then bulk-copy contiguous `SegmentSpec16` bytes.
- Decode path: bounds-check first, then either:
  - bulk-copy to `NativeArray<SegmentSpec16>` for job processing, or
  - iterate over span directly if no retained storage is needed.
- Server must validate decoded segments (bounds/build rules) before apply.
- Client applies rail add events in-order; idempotent insert behavior is recommended.
- `segment_id` is server-authoritative and deterministic; clients must not synthesize IDs.

### 8.5 Upgrade path
If rail payload bandwidth becomes a bottleneck, introduce `segment_codec`:
- `0` = raw `SegmentSpec16` (v1 default)
- `1` = coordinate-delta compressed variant (future)

---

## 9. TrainState (US lane)

### 9.1 `TrainState` (`msg_type=30`)
Body:

| Offset | Type | Name |
|---:|---|---|
| 0 | `u32` | `server_tick` |
| 4 | `u16` | `train_count` |
| 6 | `u16` | `reserved` |
| 8 | bytes | `states[train_count]` |

Per-train packed entry:

| Field | Type | Notes |
|---|---|---|
| `train_id` | `u32` | stable ID |
| `segment_id` | `u32` | rail segment index |
| `t_q16` | `u16` | `0..65535` along segment |
| `speed_q16` | `u16` | fixed-point speed |

Entry size:
- 12 bytes.

---

## 10. Compression & Prefilters

### 10.1 LZ4 block
- Apply LZ4 to snapshot payload (section 5.2).
- Rect patches are usually tiny; compression optional via heuristic.

### 10.2 Optional prefilters (future)
- Heights: row delta prefilter (`x-delta`) for improved compression.
- River/biome: raw or lightweight RLE depending on content.

---

## 11. Security / Validation (v1 essentials)
- Reject invalid lengths and malformed fragment metadata.
- Deduplicate Build/Terraform `cmd_id` per connection.
- Clamp server-applied terraforming deltas to configured limits.
- Enforce protocol version and capability gate checks during handshake.

---

## 12. Decoder/Encoder API Design (Zero-GC Friendly)

Goal:
- Keep parsing allocation-free in hot paths.

### 12.1 Principles
- Parse from `ReadOnlySpan<byte>` or `NativeArray<byte>` views.
- Avoid per-message allocations.
- Use fixed scratch buffers for fragment reassembly per connection.

### 12.2 Suggested Decoder API Surface
Conceptual C# API:
- `bool TryReadEnvelope(ReadOnlySpan<byte> data, out Envelope env)`
- `bool TryReadClientHello(in Envelope env, out ClientHello msg)`
- `bool TryReadChunkSnapshotFrag(in Envelope env, out ChunkSnapshotFrag msg)`
- `bool TryReadHeightRectPatch(in Envelope env, out HeightRectPatch msg)`
- `bool TryReadTrainState(in Envelope env, out TrainState msg)`

Message struct guidance:
- primitive fields only for headers/metadata
- `ReadOnlySpan<byte>` payload slices where possible
- no implicit heap allocations during decode

### 12.3 Fragment Reassembly Strategy
Per-connection state:
- `Dictionary<ReassemblyKey, ReassemblyBuffer>` (managed acceptable if long-lived)
- `ReassemblyBuffer` owns pooled storage (`ArrayPool<byte>` or persistent `NativeArray<byte>`)

Flow:
1. accumulate fragments by key `(cx, cy, snapshot_id)`
2. validate complete fragment set and lengths
3. hand contiguous compressed span to LZ4 decoder
4. release/reuse pooled reassembly storage

### 12.4 LZ4 Decode Strategy
Pre-allocate maximum decompressed chunk payload for v1 snapshots:
- `snapshot header (8 bytes) + 3 * 4096 bytes = 12,296 bytes`

Recommended:
- one decode scratch buffer per connection
- validate decoded length before field extraction
- copy into chunk SoA arrays via block copy after validation

### 12.5 Deterministic Logging
Server log tuple for diagnostics/replay:
- `(conn_id, msg_type, msg_id, tick, bytes)`

Optional integrity check:
- compute CRC64 over decoded snapshot payload
