Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$requiredPaths = @(
    "Assets/Scripts/Core/Rails/RailIds.cs",
    "Assets/Scripts/Core/Rails/RailIdsSelfTest.cs",
    "Assets/Scripts/Core/Rails/SegmentIdAllocator.cs",
    "Assets/Scripts/Core/Rails/SegmentStoreSoA.cs",
    "Assets/Scripts/Core/Rails/NodeTable.cs",
    "Assets/Scripts/Core/Rails/AdjacencyPool.cs",
    "Assets/Scripts/Core/Rails/RailSpatialIndex.cs",
    "Assets/Scripts/Core/Rails/RailSpatialIndexSelfTest.cs",
    "Assets/Scripts/Core/Rails/TileEdgeKeySelfTest.cs",
    "Assets/Scripts/Core/Rails/TileCenterRailGraph.cs",
    "Assets/Scripts/Core/Rails/RailMutationEvents.cs",
    "Assets/Scripts/Core/Rails/RailSegmentDeltaEncoder.cs",
    "Assets/Scripts/Core/Rails/SegmentSpec16.cs",
    "Assets/Scripts/Core/Rails/SegmentSpec16SelfTest.cs",
    "Assets/Scripts/Core/Rails/RailGraphCoreSelfTest.cs",
    "Assets/Scripts/Core/Rails/RailTraversalSelfTest.cs",
    "Assets/Scripts/Core/Rails/RailMutationEventsSelfTest.cs",
    "Assets/Scripts/Core/Rails/RailSegmentDeltaEncoderSelfTest.cs",
    "Assets/Scripts/Core/Rails/RailMutationStressSelfTest.cs",
    "Assets/Scripts/Core/Rails/RailWireContractSelfTest.cs",
    "Assets/Scripts/Core/Rails/RailIndexDriftSelfTest.cs",
    "Assets/Scripts/Core/Rails/RailScale100kSelfTest.cs",
    "docs/adr/0009-rail-graph-storage-architecture.md",
    "docs/architecture/RAIL-GRAPH-INVARIANTS.md"
)

$missing = New-Object System.Collections.Generic.List[string]
foreach ($path in $requiredPaths)
{
    if (-not (Test-Path -Path $path))
    {
        $missing.Add($path)
    }
}

if ($missing.Count -gt 0)
{
    Write-Host "Missing Sprint 6 evidence files:" -ForegroundColor Red
    foreach ($m in $missing)
    {
        Write-Host " - $m" -ForegroundColor Red
    }

    exit 1
}

Write-Host "Sprint 6 evidence validation passed."