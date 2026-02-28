Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
${Root} = "Assets/Scripts"
${ShowGraph} = $false
${AsMermaid} = $false
${MermaidPath} = "asmdef-deps.mmd"
${AsJson} = $false
${JsonPath} = "asmdef-deps.json"

for ($i = 0; $i -lt $args.Count; $i++) {
    switch ($args[$i]) {
        "-Root" {
            if ($i + 1 -lt $args.Count) {
                $Root = [string]$args[$i + 1]
                $i++
            }
        }
        "-ShowGraph" { $ShowGraph = $true }
        "-AsMermaid" { $AsMermaid = $true }
        "-MermaidPath" {
            if ($i + 1 -lt $args.Count) {
                $MermaidPath = [string]$args[$i + 1]
                $i++
            }
        }
        "-AsJson" { $AsJson = $true }
        "-JsonPath" {
            if ($i + 1 -lt $args.Count) {
                $JsonPath = [string]$args[$i + 1]
                $i++
            }
        }
        default {
            Write-Error "Unknown argument '$($args[$i])'. Supported: -Root <path> -ShowGraph -AsMermaid -MermaidPath <path> -AsJson -JsonPath <path>"
        }
    }
}

if (-not (Test-Path -Path $Root)) {
    Write-Error "Root path not found: $Root"
}

$asmdefs = Get-ChildItem -Path $Root -Recurse -Filter *.asmdef
if ($asmdefs.Count -eq 0) {
    Write-Host "No asmdef files found under $Root."
    exit 0
}

$nodes = @{}
$parseErrors = New-Object System.Collections.Generic.List[string]
$duplicateNames = New-Object System.Collections.Generic.List[string]

foreach ($file in ($asmdefs | Sort-Object FullName)) {
    $raw = Get-Content $file.FullName -Raw
    $trim = $raw.TrimStart()

    if ($trim.StartsWith("{{")) {
        $parseErrors.Add("$($file.FullName): starts with '{{' (duplicated opening brace).")
        continue
    }

    try {
        $json = $raw | ConvertFrom-Json
    }
    catch {
        $parseErrors.Add("$($file.FullName): invalid JSON - $($_.Exception.Message)")
        continue
    }

    $name = [string]$json.name
    if ([string]::IsNullOrWhiteSpace($name)) {
        $parseErrors.Add("$($file.FullName): missing 'name'.")
        continue
    }

    if ($nodes.ContainsKey($name)) {
        $duplicateNames.Add("$name declared in both '$($nodes[$name].Path)' and '$($file.FullName)'.")
        continue
    }

    $refs = @()
    if ($null -ne $json.references) {
        $refs = @($json.references | ForEach-Object { [string]$_ })
    }

if ($duplicateNames.Count -gt 0) {
    $hasErrors = $true
    Write-Host "`nDuplicate asmdef names:" -ForegroundColor Red
    $duplicateNames | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
}

    $nodes[$name] = [PSCustomObject]@{
        Name = $name
        Refs = $refs
        Path = $file.FullName
    }
}

$missingRefs = New-Object System.Collections.Generic.List[string]
foreach ($node in $nodes.Values) {
    foreach ($r in $node.Refs) {
        if (-not $nodes.ContainsKey($r)) {
            $missingRefs.Add("$($node.Name) -> $r")
        }
    }
}

$state = @{}
$stack = New-Object System.Collections.Generic.List[string]
$cycles = New-Object System.Collections.Generic.List[string]

function Visit([string]$name) {
    if ($state.ContainsKey($name)) {
        if ($state[$name] -eq 1) {
            $idx = $stack.IndexOf($name)
            if ($idx -ge 0) {
                $cyclePath = @($stack[$idx..($stack.Count - 1)]) + $name
                $cycles.Add(($cyclePath -join " -> "))
            }
        }
        return
    }

    $state[$name] = 1
    $stack.Add($name) | Out-Null

    foreach ($r in $nodes[$name].Refs) {
        if ($nodes.ContainsKey($r)) {
            Visit $r
        }
    }

    [void]$stack.RemoveAt($stack.Count - 1)
    $state[$name] = 2
}

foreach ($name in $nodes.Keys) {
    if (-not $state.ContainsKey($name)) {
        Visit $name
    }
}

if ($ShowGraph) {
    Write-Host "Asmdef dependency graph:"
    foreach ($node in ($nodes.Values | Sort-Object Name)) {
        $deps = if ($node.Refs.Count -eq 0) { "(none)" } else { $node.Refs -join ", " }
        Write-Host " - $($node.Name) -> $deps"
    }
}

if ($AsMermaid) {
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add("graph TD") | Out-Null
    foreach ($node in ($nodes.Values | Sort-Object Name)) {
        if ($node.Refs.Count -eq 0) {
            $lines.Add("    $($node.Name)") | Out-Null
            continue
        }

        foreach ($r in $node.Refs) {
            $lines.Add("    $($node.Name) --> $r") | Out-Null
        }
    }
    Set-Content -Path $MermaidPath -Value ($lines -join "`n")
    Write-Host "Wrote Mermaid graph to $MermaidPath"
}

if ($AsJson) {
    $reportNodes = @($nodes.Values | Sort-Object Name | ForEach-Object {
            [PSCustomObject]@{
                name = $_.Name
                references = @($_.Refs)
                path = $_.Path
            }
        })

    $report = [PSCustomObject]@{
        root = $Root
        assemblyCount = $nodes.Count
        nodes = $reportNodes
        parseErrors = @($parseErrors)
        missingReferences = @($missingRefs | Sort-Object -Unique)
        cycles = @($cycles | Sort-Object -Unique)
        duplicateNames = @($duplicateNames | Sort-Object -Unique)
    }

    $report | ConvertTo-Json -Depth 8 | Set-Content -Path $JsonPath
    Write-Host "Wrote JSON report to $JsonPath"
}

$hasErrors = $false

if ($parseErrors.Count -gt 0) {
    $hasErrors = $true
    Write-Host "`nInvalid asmdef files:" -ForegroundColor Red
    $parseErrors | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
}

if ($missingRefs.Count -gt 0) {
    $hasErrors = $true
    Write-Host "`nMissing referenced asmdefs:" -ForegroundColor Red
    $missingRefs | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
}

if ($cycles.Count -gt 0) {
    $hasErrors = $true
    Write-Host "`nAsmdef cycles detected:" -ForegroundColor Red
    $cycles | Sort-Object -Unique | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
}

if ($hasErrors) {
    exit 1
}

Write-Host "Asmdef dependency validation passed for $($nodes.Count) assemblies."
