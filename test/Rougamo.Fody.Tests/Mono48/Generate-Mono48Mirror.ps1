param(
    [string]$ProjectRoot = $(Split-Path -Parent $PSScriptRoot),
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$items = @(
    @{ Namespace = "Rougamo.Fody.Tests.Mono48"; Class = "MonoTests"; TargetType = "Rougamo.Fody.Tests.MonoTests"; SourceFile = "MonoTests.cs" },
    @{ Namespace = "Rougamo.Fody.Tests.Mono48"; Class = "BasicTest"; TargetType = "Rougamo.Fody.Tests.BasicTest"; SourceFile = "BasicTest.cs" },
    @{ Namespace = "Rougamo.Fody.Tests.Mono48"; Class = "IssueTest"; TargetType = "Rougamo.Fody.Tests.IssueTest"; SourceFile = "IssueTest.cs" },
    @{ Namespace = "Rougamo.Fody.Tests.Mono48"; Class = "ConfiguredMoTests"; TargetType = "Rougamo.Fody.Tests.ConfiguredMoTests"; SourceFile = "ConfiguredMoTests.cs" },
    @{ Namespace = "Rougamo.Fody.Tests.Mono48"; Class = "PatternTests"; TargetType = "Rougamo.Fody.Tests.PatternTests"; SourceFile = "PatternTests.cs" },

    @{ Namespace = "Rougamo.Fody.Tests.Mono48.Signatures"; Class = "SignatureBasicTests"; TargetType = "Rougamo.Fody.Tests.Signatures.SignatureBasicTests"; SourceFile = "Signatures/SignatureBasicTests.cs" },
    @{ Namespace = "Rougamo.Fody.Tests.Mono48.Signatures"; Class = "WildcardMatchTests"; TargetType = "Rougamo.Fody.Tests.Signatures.WildcardMatchTests"; SourceFile = "Signatures/WildcardMatchTests.cs" },
    @{ Namespace = "Rougamo.Fody.Tests.Mono48.Signatures"; Class = "PropertyMatchTests"; TargetType = "Rougamo.Fody.Tests.Signatures.PropertyMatchTests"; SourceFile = "Signatures/PropertyMatchTests.cs" },
    @{ Namespace = "Rougamo.Fody.Tests.Mono48.Signatures"; Class = "GetterMatchTests"; TargetType = "Rougamo.Fody.Tests.Signatures.GetterMatchTests"; SourceFile = "Signatures/PropertyMatchTests.cs" },
    @{ Namespace = "Rougamo.Fody.Tests.Mono48.Signatures"; Class = "SetterMatchTests"; TargetType = "Rougamo.Fody.Tests.Signatures.SetterMatchTests"; SourceFile = "Signatures/PropertyMatchTests.cs" },
    @{ Namespace = "Rougamo.Fody.Tests.Mono48.Signatures"; Class = "PatternParseTests"; TargetType = "Rougamo.Fody.Tests.Signatures.PatternParseTests"; SourceFile = "Signatures/PatternParseTests.cs" },
    @{ Namespace = "Rougamo.Fody.Tests.Mono48.Signatures"; Class = "ExecutionMatchTests"; TargetType = "Rougamo.Fody.Tests.Signatures.ExecutionMatchTests"; SourceFile = "Signatures/ExecutionMatchTests.cs" },
    @{ Namespace = "Rougamo.Fody.Tests.Mono48.Signatures"; Class = "MethodMatchTests"; TargetType = "Rougamo.Fody.Tests.Signatures.MethodMatchTests"; SourceFile = "Signatures/ExecutionMatchTests.cs" }
)

$excludedFacts = @(
    "Rougamo.Fody.Tests.BasicTest.GenericMoTest",
    "Rougamo.Fody.Tests.IssueTest.Issue63Test"
)

function Test-PreprocessorCondition {
    param(
        [string]$Expression,
        [bool]$IsDebug
    )

    $exp = ($Expression -replace '\s+', '')
    switch ($exp) {
        "DEBUG" { return $IsDebug }
        "!DEBUG" { return -not $IsDebug }
        default { return $true }
    }
}

function Get-ActiveContent {
    param(
        [string]$Content,
        [bool]$IsDebug
    )

    $lines = $Content -split "`r?`n"
    $states = New-Object System.Collections.Generic.List[bool]
    $states.Add($true)
    $sb = New-Object System.Text.StringBuilder

    foreach ($line in $lines) {
        if ($line -match '^\s*#if\s+(.+)$') {
            $parent = $states[$states.Count - 1]
            $condition = Test-PreprocessorCondition -Expression $matches[1] -IsDebug $IsDebug
            $states.Add($parent -and $condition)
            continue
        }

        if ($line -match '^\s*#else\b') {
            if ($states.Count -gt 1) {
                $parent = $states[$states.Count - 2]
                $current = $states[$states.Count - 1]
                $states[$states.Count - 1] = $parent -and (-not $current)
            }
            continue
        }

        if ($line -match '^\s*#endif\b') {
            if ($states.Count -gt 1) {
                $states.RemoveAt($states.Count - 1)
            }
            continue
        }

        if ($states[$states.Count - 1]) {
            [void]$sb.AppendLine($line)
        }
    }

    return $sb.ToString()
}

$isDebug = $Configuration -imatch 'debug'

$factPattern = '(?ms)\[Fact(?:\([^\)]*\))?\]\s*public\s+(?:async\s+)?(?:Task|void)\s+([A-Za-z_][A-Za-z0-9_]*)\s*\('
$grouped = [ordered]@{}

foreach ($item in $items) {
    $sourcePath = Join-Path $ProjectRoot $item.SourceFile
    $content = Get-Content -Path $sourcePath -Raw -Encoding UTF8
    $content = Get-ActiveContent -Content $content -IsDebug $isDebug
    $names = [regex]::Matches($content, $factPattern) | ForEach-Object { $_.Groups[1].Value } | Select-Object -Unique

    if (-not $grouped.Contains($item.Namespace)) {
        $grouped[$item.Namespace] = @()
    }
    $grouped[$item.Namespace] += [pscustomobject]@{
        Class = $item.Class
        TargetType = $item.TargetType
        Methods = $names
    }
}

$sb = New-Object System.Text.StringBuilder
[void]$sb.AppendLine("#if NET48")
[void]$sb.AppendLine("using Xunit;")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("// <auto-generated>")
[void]$sb.AppendLine("// Generated by Mono48/Generate-Mono48Mirror.ps1")
[void]$sb.AppendLine("// </auto-generated>")
[void]$sb.AppendLine("")

foreach ($ns in $grouped.Keys) {
    [void]$sb.AppendLine("namespace $ns")
    [void]$sb.AppendLine("{")
    foreach ($entry in $grouped[$ns]) {
        [void]$sb.AppendLine("    [Collection(""Mono48"")]")
        [void]$sb.AppendLine("    public class $($entry.Class)")
        [void]$sb.AppendLine("    {")
        foreach ($method in $entry.Methods) {
            $factFullName = "$($entry.TargetType).$method"
            if ($excludedFacts -contains $factFullName) {
                continue
            }
            [void]$sb.AppendLine("        [Fact] public void $method() => MonoRunnerBridge.RunMonoFact(""$factFullName"");")
        }
        [void]$sb.AppendLine("    }")
        [void]$sb.AppendLine("")
    }
    [void]$sb.AppendLine("}")
    [void]$sb.AppendLine("")
}

[void]$sb.AppendLine("#endif")

$outputPath = Join-Path $PSScriptRoot "Mono48MirrorTests.g.cs"
[System.IO.File]::WriteAllText($outputPath, $sb.ToString(), (New-Object System.Text.UTF8Encoding($false)))
Write-Host "Generated $outputPath"
