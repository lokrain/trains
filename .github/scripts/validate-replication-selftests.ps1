Set-StrictMode -Version Latest
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$requiredSelfTests = @(
    "Assets/Scripts/Core/Net/Protocol/Crc64SelfTest.cs",
    "Assets/Scripts/Core/Net/Protocol/ReplicationProtocolSelfTest.cs",
    "Assets/Scripts/Core/Net/Protocol/SnapshotReassemblySelfTest.cs",
    "Assets/Scripts/Core/Net/Protocol/ReassemblyChurnSelfTest.cs",
    "Assets/Scripts/Core/Net/Protocol/ReassemblyMemoryTrendSelfTest.cs",
    "Assets/Scripts/Core/Client/Net/WorldPatchReceiverSelfTest.cs",
    "Assets/Scripts/Core/Client/Net/SnapshotPatchEquivalenceSelfTest.cs",
    "Assets/Scripts/Core/Client/Net/PatchChainPropertySelfTest.cs",
    "Assets/Scripts/Core/Server/Net/ChunkStreamSchedulerSelfTest.cs"
)

foreach ($path in $requiredSelfTests)
{
    if (-not (Test-Path $path))
    {
        throw "Missing replication self-test file: $path"
    }

    $content = Get-Content -Path $path -Raw
    if ($content -notmatch "public\s+static\s+bool\s+Run\s*\(")
    {
        throw "Self-test file does not expose 'public static bool Run()': $path"
    }
}

$suitePath = "Assets/Scripts/Core/Net/Protocol/ReplicationSelfTestSuite.cs"
if (-not (Test-Path $suitePath))
{
    throw "Missing self-test suite file: $suitePath"
}

$suiteContent = Get-Content -Path $suitePath -Raw
$expectedCalls = @(
    "Crc64SelfTest.Run()",
    "ReplicationProtocolSelfTest.Run()",
    "SnapshotReassemblySelfTest.Run()",
    "ReassemblyChurnSelfTest.Run()",
    "ReassemblyMemoryTrendSelfTest.Run()",
    "WorldPatchReceiverSelfTest.Run()",
    "SnapshotPatchEquivalenceSelfTest.Run()",
    "PatchChainPropertySelfTest.Run()",
    "ChunkStreamSchedulerSelfTest.Run()"
)

foreach ($call in $expectedCalls)
{
    if ($suiteContent -notmatch [regex]::Escape($call))
    {
        throw "Self-test suite is missing required call: $call"
    }
}

Write-Host "Replication self-test suite validation passed."
