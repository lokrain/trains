Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$outputPath = "render-throughput-bench.md"

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# Render Throughput Benchmark (Scaffold)") | Out-Null
$lines.Add("") | Out-Null
$lines.Add("This scaffold captures benchmark scenario metadata and baseline placeholders for Sprint 5 F2.") | Out-Null
$lines.Add("") | Out-Null
$lines.Add("| Scenario | p50 Frame (ms) | p95 Frame (ms) | p50 Rebuild (ms) | p95 Rebuild (ms) | Notes |") | Out-Null
$lines.Add("|---|---:|---:|---:|---:|---|") | Out-Null
$lines.Add("| Cold join full AOI | TBD | TBD | TBD | TBD | Capture after profiler run |") | Out-Null
$lines.Add("| Steady-state no updates | TBD | TBD | TBD | TBD | Capture after profiler run |") | Out-Null
$lines.Add("| High-frequency patch updates | TBD | TBD | TBD | TBD | Capture after profiler run |") | Out-Null
$lines.Add("") | Out-Null
$lines.Add("Generated UTC: $([DateTime]::UtcNow.ToString('o'))") | Out-Null

Set-Content -Path $outputPath -Value ($lines -join "`n")
Write-Host "Wrote $outputPath"