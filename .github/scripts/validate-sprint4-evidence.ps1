Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$requiredPaths = @(
    "Assets/Scripts/Core/Client/Net/WorldSnapshotReceiver.cs",
    "Assets/Scripts/Core/Client/Net/WorldPatchReceiver.cs",
    "Assets/Scripts/Core/Server/Net/ChunkStreamScheduler.cs",
    "Assets/Scripts/Core/Net/Protocol/ReplicationCounters.cs",
    "Assets/Scripts/Core/Net/Protocol/ReplicationErrorCode.cs",
    "Assets/Scripts/Core/Net/Protocol/ReplicationSelfTestSuite.cs",
    "Assets/Scripts/Core/Net/Protocol/SnapshotReassemblySelfTest.cs",
    "Assets/Scripts/Core/Net/Protocol/ReassemblyChurnSelfTest.cs",
    "Assets/Scripts/Core/Net/Protocol/ReassemblyMemoryTrendSelfTest.cs",
    "Assets/Scripts/Core/Client/Net/WorldPatchReceiverSelfTest.cs",
    "Assets/Scripts/Core/Client/Net/SnapshotPatchEquivalenceSelfTest.cs",
    "Assets/Scripts/Core/Client/Net/PatchChainPropertySelfTest.cs",
    "Assets/Scripts/Core/Server/Net/ChunkStreamSchedulerSelfTest.cs",
    "docs/adr/0006-snapshot-fragmentation-reassembly-safety.md",
    "docs/adr/0007-patch-lineage-mismatch-resync-policy.md",
    "docs/sprints/Sprint4-Closure-Evidence.md"
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
    Write-Host "Missing Sprint 4 evidence files:" -ForegroundColor Red
    foreach ($m in $missing)
    {
        Write-Host " - $m" -ForegroundColor Red
    }

    exit 1
}

Write-Host "Sprint 4 evidence validation passed."