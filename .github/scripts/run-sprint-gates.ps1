Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$results = New-Object System.Collections.Generic.List[object]
$reportTitle = "Sprint Gates"
$reportBaseName = "sprint-gates"

function Invoke-Gate([string]$name, [scriptblock]$action)
{
    try
    {
        & $action
        $results.Add([PSCustomObject]@{ Gate = $name; Status = "PASS"; Message = "" }) | Out-Null
    }
    catch
    {
        $results.Add([PSCustomObject]@{ Gate = $name; Status = "FAIL"; Message = $_.Exception.Message }) | Out-Null
        throw
    }
}

try
{
    Invoke-Gate "asmdef-deps" { & ./.github/scripts/validate-asmdef-deps.ps1 -ShowGraph -AsJson -JsonPath asmdef-deps.json }
    Invoke-Gate "replication-selftests" { & ./.github/scripts/validate-replication-selftests.ps1 }
    Invoke-Gate "sprint4-evidence" { & ./.github/scripts/validate-sprint4-evidence.ps1 }
    Invoke-Gate "sprint5-evidence" { & ./.github/scripts/validate-sprint5-evidence.ps1 }
    Invoke-Gate "sprint6-evidence" { & ./.github/scripts/validate-sprint6-evidence.ps1 }
}
finally
{
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("# $reportTitle") | Out-Null
    $lines.Add("") | Out-Null
    $lines.Add("| Gate | Status | Message |") | Out-Null
    $lines.Add("|---|---|---|") | Out-Null

    foreach ($r in $results)
    {
        $msg = [string]$r.Message
        if ([string]::IsNullOrWhiteSpace($msg))
        {
            $msg = "-"
        }
        else
        {
            $msg = $msg.Replace("|", "/").Replace("`r", " ").Replace("`n", " ")
        }

        $lines.Add("| $($r.Gate) | $($r.Status) | $msg |") | Out-Null
    }

    Set-Content -Path "$reportBaseName.md" -Value ($lines -join "`n")

    $overall = "PASS"
    foreach ($r in $results)
    {
        if ($r.Status -eq "FAIL")
        {
            $overall = "FAIL"
            break
        }
    }

    $gateJson = @($results | ForEach-Object {
            @{
                gate = [string]$_.Gate
                status = [string]$_.Status
                message = [string]$_.Message
            }
        })

    $jsonMap = @{
        generatedUtc = (Get-Date).ToUniversalTime().ToString("o")
        overall = $overall
        gates = $gateJson
    }

    $jsonMap | ConvertTo-Json -Depth 8 | Set-Content -Path "$reportBaseName.json"
}

Write-Host "$reportTitle run complete."
