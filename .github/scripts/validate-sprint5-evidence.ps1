Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$requiredPaths = @(
    "Assets/Scripts/Core/Client/Render/ChunkMeshBuilder.cs",
    "Assets/Scripts/Core/Client/Render/MapChunkMeshBuildSystem.cs",
    "Assets/Scripts/Core/Client/Render/ChunkMeshDeterminismSelfTest.cs",
    "Assets/Scripts/Core/Client/Render/ChunkMeshPatchEquivalenceSelfTest.cs",
    "Assets/Scripts/Core/Client/Render/ChunkMeshSeamSelfTest.cs",
    "Assets/Scripts/Presentation/WorldRendering/ChunkPresenterRenderBindingSystem.cs",
    "Assets/Scripts/Presentation/WorldRendering/ChunkPresenterLifecycleSystem.cs",
    "Assets/Scripts/Presentation/WorldRendering/ChunkRenderInvalidationSystem.cs",
    "Assets/Scripts/Presentation/WorldRendering/ChunkMeshRebuildBudget.cs",
    "docs/adr/0008-entities-graphics-chunk-rendering-architecture.md",
    "docs/architecture/RENDERING-RUNBOOK.md"
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
    Write-Host "Missing Sprint 5 evidence files:" -ForegroundColor Red
    foreach ($m in $missing)
    {
        Write-Host " - $m" -ForegroundColor Red
    }

    exit 1
}

Write-Host "Sprint 5 evidence validation passed."