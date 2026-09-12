Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanInventory-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path (Join-Path $scratch 'Content') | Out-Null
Add-Type -AssemblyName System.Drawing
foreach($size in @(@(2,2),@(3,4))) {
 $bitmap=[Drawing.Bitmap]::new($size[0],$size[1])
 try{$bitmap.Save((Join-Path $scratch "Content/$($size[0]).png"),[Drawing.Imaging.ImageFormat]::Png)}finally{$bitmap.Dispose()}
}
Copy-Item -LiteralPath (Join-Path $scratch 'Content/2.png') -Destination (Join-Path $scratch 'Content/duplicate.png')
$tool=Join-Path $PSScriptRoot 'Measure-QATextureInventory.ps1';$output=Join-Path $scratch 'positive.json'
& pwsh -NoProfile -File $tool -BuildRoot $scratch -OutputPath $output
if($LASTEXITCODE -ne 0){throw 'INVENTORY_POSITIVE_FAILED'}
$r=Get-Content -Raw -LiteralPath $output | ConvertFrom-Json
if($r.pngCount -ne 3 -or $r.uniqueFileHashCount -ne 2 -or $r.perPathRgba8Bytes -ne 80 -or $r.uniqueFileHashRgba8Bytes -ne 64 -or $r.potentialIdenticalFileRgba8Bytes -ne 16){throw 'INVENTORY_ARITHMETIC'}
$negative=& pwsh -NoProfile -File $tool -BuildRoot $scratch -OutputPath $output 2>&1
if($LASTEXITCODE -eq 0 -or ($negative -join "`n") -notmatch 'INVENTORY_OUTPUT_EXISTS'){throw 'OVERWRITE_CONTROL_SURVIVED'}
[IO.File]::WriteAllBytes((Join-Path $scratch 'Content/invalid.png'),[byte[]](0,1,2))
$negative=& pwsh -NoProfile -File $tool -BuildRoot $scratch -OutputPath (Join-Path $scratch 'bad.json') 2>&1
if($LASTEXITCODE -eq 0 -or ($negative -join "`n") -notmatch 'PNG_HEADER_TRUNCATED' -or (Test-Path -LiteralPath (Join-Path $scratch 'bad.json'))){throw 'TRUNCATED_CONTROL_SURVIVED'}
Write-Output 'PASS actual inventory CLI: exact small-image arithmetic, duplicate hash opportunity, output protection and truncated-header rejection. Synthetic fixtures retained only in temp.'
