Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanEvidenceTest-'+[guid]::NewGuid().ToString('N'))
$null=New-Item -ItemType Directory -Path $scratch
$inputPath=Join-Path $scratch 'synthetic.log'
$outputPath=Join-Path $scratch 'evidence.json'
$fixture=@'
[15:00:00.000] [Main Thread/INFO] [apogean]: MAW SHALLOW COMPLETE: rib1
[15:00:00.040] [Main Thread/WARN] [tML]: Silently Caught Exception:
System.DivideByZeroException: Attempted to divide by zero.
   at Terraria.ModLoader.SurfaceBackgroundStylesLoader.DrawCloseBackground(Int32 style)
   at Terraria.Main.DrawBG()

[15:00:00.050] [Main Thread/DEBUG] [tML]: unrelated private telemetry must not be exported
[15:00:00.060] [Main Thread/WARN] [tML]: Silently Caught Exception:
System.InvalidOperationException: unrelated exception must not be exported
   at Other.Namespace()
[15:00:00.070] [Main Thread/WARN] [tML]: Silently Caught Exception:
System.IndexOutOfRangeException: Index was outside bounds.
   at Terraria.Main.DrawLiquid(Boolean bg)
[15:00:00.080] [Main Thread/WARN] [tML]: Silently Caught Exception:
System.InvalidOperationException: Synthetic cave draw failure.
   at apogean.Content.Diagnostics.MawCaveBackdropProbe.ProbeOverlay.Draw(SpriteBatch batch)
[15:00:00.090] [Main Thread/WARN] [tML]: Silently Caught Exception:
System.ArgumentException: Synthetic overlay manager failure.
   at Terraria.Graphics.Effects.OverlayManager.Draw(SpriteBatch batch)
[15:00:00.100] [Main Thread/INFO] [apogean]: MAW CAVE PROBE COMPLETE: report
'@
[IO.File]::WriteAllText($inputPath,$fixture)
& (Join-Path $PSScriptRoot 'Export-MawNativeEvidence.ps1') -LogPath $inputPath -OutputPath $outputPath
$e=Get-Content -LiteralPath $outputPath -Raw | ConvertFrom-Json
if($e.records.Count -ne 2 -or $e.caughtRenderFailures.Count -ne 4){throw 'Lost complete record or relevant engine draw failure.'}
$raw=Get-Content -LiteralPath $outputPath -Raw
if($raw -match 'unrelated'){throw 'Exported unrelated telemetry.'}
if($e.caughtRenderFailures[0] -notmatch 'DivideByZeroException' -or $e.caughtRenderFailures[1] -notmatch 'IndexOutOfRangeException'){throw 'Failure type not retained.'}
if($e.caughtRenderFailures[2] -notmatch 'MawCaveBackdropProbe' -or $e.caughtRenderFailures[3] -notmatch 'OverlayManager'){throw 'Cave/overlay draw failure not retained.'}
$refused=$false
try { & (Join-Path $PSScriptRoot 'Export-MawNativeEvidence.ps1') -LogPath $inputPath -OutputPath $outputPath } catch { $refused=$_.Exception.Message -match 'already exists' }
if(-not $refused){throw 'Existing evidence was not protected.'}
Write-Output 'PASS: two request completions plus four independent draw failures retained; unrelated data excluded; overwrite refused. Synthetic exporter test, not native render approval.'
