param([string]$ProjectRoot=(Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference='Stop'
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanArrivalMutations-'+[guid]::NewGuid().ToString('N'))
$source=Get-Content -LiteralPath (Join-Path $ProjectRoot 'Common/WorldGeneration/ArrivalSitePlanner.cs') -Raw
$controls=@(
 @('missing-compact','mode == 0 ? 13 : 7','13','NO_TREE_PRESERVING_COMPACT_SITE'),
 @('ignore-obstacle','read(x, y) == ArrivalCell.Blocked','read(x, y) == (ArrivalCell)99','IGNORED_OBSTACLE'),
 @('ignore-support','read(x, y) != ArrivalCell.Soil) { reason = "cavity-or-changed-support"','read(x, y) == (ArrivalCell)99) { reason = "cavity-or-changed-support"','CAVITY_OPENED'),
 @('ignore-reservation','site != null && !available(site.Envelope)','site != null && site.CenterX == int.MinValue','RESERVATION_BYPASS_OR_RETRY_LOOP'),
 @('ignore-spawn','site != null && site.Envelope.Intersects(landing)','site != null && site.CenterX == int.MinValue','SPAWN_ENVELOPE_CHANGED'),
 @('too-deep','0, 0, 1, 1, 2, 2, 2, 2, 2, 1, 1, 0, 0','0, 0, 1, 1, 3, 3, 3, 3, 3, 1, 1, 0, 0','FLAT_DIVOT'),
 @('partial-cover','affected == 0 || affected == cover.Width * cover.Height','affected >= 0','PARTIAL_COVER_CUT')
)
foreach($control in $controls) {
 $variant=Join-Path $scratch $control[0]
 $directory=Join-Path $variant 'Common/WorldGeneration'
 New-Item -ItemType Directory -Path $directory -Force | Out-Null
 if(-not $source.Contains($control[1])){throw "Missing mutation seam: $($control[0])"}
 [IO.File]::WriteAllText((Join-Path $directory 'ArrivalSitePlanner.cs'),$source.Replace($control[1],$control[2]))
 $output=& pwsh -NoProfile -File (Join-Path $ProjectRoot 'Tools/Test-ArrivalSitePlanner.ps1') -ProjectRoot $variant 2>&1 | Out-String
 if($LASTEXITCODE -eq 0 -or $output -notmatch $control[3]){throw "Mutation not rejected for intended reason: $($control[0]) $output"}
 Write-Host "REJECTED: $($control[0]) => $($control[3])"
}
Write-Host "PASS: $($controls.Count) actual-source mutations rejected by the real planner suite. Scratch retained: $scratch"
