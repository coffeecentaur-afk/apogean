param([string]$CandidateDirectory=(Join-Path (Split-Path $PSScriptRoot) 'Art/Candidates/MawTerrain-Harsh-v1/Packed-v2'))
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$recipe=Get-Content -Raw -LiteralPath (Join-Path $CandidateDirectory 'recipe.json') | ConvertFrom-Json
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanPackedControls-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition (Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'PackedMaterialCompiler.cs.txt'))
$count=0
foreach($case in @('positive','alpha','white','guard','wrongPhase','dimensions','badIndex','wallOverlap')) {
    $wall=$case -eq 'wallOverlap'
    $entry=@($recipe.entries | Where-Object {$_.name -eq 'stone' -and $_.wall -eq $wall})[0]
    $folder=Join-Path $scratch $case
    New-Item -ItemType Directory -Path $folder | Out-Null
    foreach($name in @('atlas.png','map.bin')) {Copy-Item -LiteralPath (Join-Path $CandidateDirectory "$($entry.folder)/$name") -Destination $folder}
    if($case -eq 'badIndex') {
        $stream=[IO.File]::OpenWrite((Join-Path $folder 'map.bin'))
        try {$stream.Position=32;$bytes=[BitConverter]::GetBytes([int]$entry.masks);$stream.Write($bytes,0,4)} finally {$stream.Dispose()}
    } elseif($case -ne 'positive') {
        $path=Join-Path $folder 'atlas.png'
        $original=[Drawing.Bitmap]::new($path)
        $changed=if($case -eq 'dimensions'){[Drawing.Bitmap]::new(16,16)}else{[Drawing.Bitmap]::new($original)}
        try {
            if($case -eq 'guard') {$changed.SetPixel(16,0,[Drawing.Color]::White)}
            elseif($case -ne 'dimensions') {
                $found=$false
                for($y=0;$y -lt $original.Height -and -not $found;$y++) {
                    for($x=0;$x -lt $original.Width -and -not $found;$x++) {
                        if($original.GetPixel($x,$y).A -ne 255){continue}
                        if($case -eq 'wallOverlap' -and $x%34 -lt 16){continue}
                        switch($case) {
                            alpha {$changed.SetPixel($x,$y,[Drawing.Color]::Transparent)}
                            white {$changed.SetPixel($x,$y,[Drawing.Color]::White)}
                            wallOverlap {$changed.SetPixel($x,$y,[Drawing.Color]::White)}
                            wrongPhase {
                                $other=$original.GetPixel(($x+18)%$original.Width,$y)
                                if($other.A -ne 255 -or $other.ToArgb() -eq $original.GetPixel($x,$y).ToArgb()){continue}
                                $changed.SetPixel($x,$y,$other)
                            }
                        }
                        $found=$true
                    }
                }
                if(-not $found){throw "NO_MUTATION_TARGET $case"}
            }
            $original.Dispose();$changed.Save($path,[Drawing.Imaging.ImageFormat]::Png)
        } finally {$original.Dispose();$changed.Dispose()}
    }
    $failure=$null
    try {[void][PackedMaterialCompiler]::Verify($entry.materialPath,$entry.referencePath,$folder,$entry.grass,$entry.wall,$entry.overlayPath)}
    catch {$failure=$_.Exception.ToString()}
    $expected=switch($case){positive{''}dimensions{'DIMENSIONS'}badIndex{'INDEX'}default{'PIXEL_CONTRACT'}}
    if($case -eq 'positive') {if($failure){throw "POSITIVE_FAILED $failure"}}
    elseif(-not $failure -or $failure -notmatch $expected){throw "MUTATION_SURVIVED $case $failure"}
    Write-Output "PASS packed compiler control: $case";$count++
}
Write-Output "PASS $count compiler controls (not CLI provenance controls). Retained in $scratch"
