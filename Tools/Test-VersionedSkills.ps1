Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$installedBase = Join-Path ([Environment]::GetFolderPath('UserProfile')) '.codex/skills'
$versionedBase = Join-Path $root 'AgentSkills'
$skillNames = @(
    'tmodloader-atlas-authoring',
    'tmodloader-entity-authoring',
    'tmodloader-tree-authoring',
    'tmodloader-background-authoring',
    'tmodloader-structure-authoring',
    'tmodloader-boss-authoring',
    'tmodloader-quest-dialogue-authoring',
    'apogean-content-direction'
)
$failures = [Collections.Generic.List[string]]::new()

function Normalize-Text([string]$path) {
    [IO.File]::ReadAllText($path).Replace("`r`n", "`n").Replace("`r", "`n").TrimEnd("`n")
}

foreach ($skillName in $skillNames) {
    $installedRoot = Join-Path $installedBase $skillName
    $versionedRoot = Join-Path $versionedBase $skillName
    if (-not (Test-Path -LiteralPath $installedRoot)) { $failures.Add("missing installed skill $skillName"); continue }
    if (-not (Test-Path -LiteralPath $versionedRoot)) { $failures.Add("missing versioned skill $skillName"); continue }

    foreach ($installedFile in Get-ChildItem -LiteralPath $installedRoot -Recurse -File) {
        $relative = [IO.Path]::GetRelativePath($installedRoot, $installedFile.FullName)
        $versionedFile = Join-Path $versionedRoot $relative
        if (-not (Test-Path -LiteralPath $versionedFile)) { $failures.Add("$skillName/$relative is not versioned"); continue }
        if ((Normalize-Text $installedFile.FullName) -cne (Normalize-Text $versionedFile)) { $failures.Add("$skillName/$relative differs from installed copy") }
    }
    foreach ($versionedFile in Get-ChildItem -LiteralPath $versionedRoot -Recurse -File) {
        $relative = [IO.Path]::GetRelativePath($versionedRoot, $versionedFile.FullName)
        if (-not (Test-Path -LiteralPath (Join-Path $installedRoot $relative))) { $failures.Add("$skillName/$relative is not installed") }
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "FAIL: $_" -ForegroundColor Red }
    exit 1
}
Write-Host "PASS: $($skillNames.Count) installed authoring skills match their Git-versioned snapshots." -ForegroundColor Green
