param(
    [Parameter(Mandatory)][string]$ReferenceAtlas,
    [string]$SourcePath = (Join-Path (Split-Path -Parent $PSScriptRoot) 'Art/Candidates/MawBone-v1/MaskedSource-v1/material-source.png'),
    [ValidatePattern('^[A-Fa-f0-9]{64}$')][string]$SourceSHA256 = '09DABAA17B0D4ABBFED3338B7BAB22361D9C6C36A330BDB73D69E0598B748221'
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$repo=Split-Path -Parent $PSScriptRoot
$source=(Resolve-Path -LiteralPath $SourcePath).Path
$reference=(Resolve-Path -LiteralPath $ReferenceAtlas).Path
$sourceHash=$SourceSHA256
$referenceHash='48907D0C61D9B68997C33FD25B0BAADB0A2D8276B6E759761C14E6B153C917EC'
$production=Join-Path $repo 'Content/Tiles/OssuaryBone.png'
$productionHash=(Get-FileHash -LiteralPath $production).Hash
$runner=(Get-Process -Id $PID).Path
$entry=Join-Path $PSScriptRoot 'New-MaskedTerrainCandidate.ps1'
$scratch=Join-Path ([IO.Path]::GetTempPath()) ('ApogeanTerrainMask-'+[guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $scratch | Out-Null
$results=[Collections.Generic.List[string]]::new()
function Invoke-Candidate([string]$destination,[string]$sourcePin,[string]$referencePin,[string]$failure='') {
    $log=& $runner -NoProfile -File $entry -SourcePath $source -ReferenceAtlas $reference -OutputDirectory $destination -SourceSHA256 $sourcePin -ReferenceSHA256 $referencePin 2>&1
    if($failure){
        if($LASTEXITCODE -eq 0 -or ($log | Out-String) -notmatch $failure){throw "EXPECTED_$($failure): $log"}
        $results.Add("PASS CLI rejects $failure")
    } elseif($LASTEXITCODE -ne 0){throw "CANDIDATE_FAILED: $log"}
}
$first=Join-Path $scratch 'first'
$second=Join-Path $scratch 'repeat'
Invoke-Candidate $first $sourceHash $referenceHash
Invoke-Candidate $second $sourceHash $referenceHash
foreach($file in @('material-native.png','color-master.png','frame-mask.png','OssuaryBone-candidate.png','preview.png')){
    if((Get-FileHash -LiteralPath (Join-Path $first $file)).Hash -ne (Get-FileHash -LiteralPath (Join-Path $second $file)).Hash){throw "NONDETERMINISTIC_$file"}
}
$results.Add('PASS two real CLI runs reproduce five PNGs exactly')
$report=Get-Content -LiteralPath (Join-Path $first 'recipe.json') -Raw | ConvertFrom-Json
if(-not $report.exactMaskExport -or $report.nativeRendered -or $report.artApproved -or $report.opaquePixels -ne 44104 -or $report.pairwiseColorSocketChecks -ne 1843200 -or $report.sourceSHA256 -ine $sourceHash -or $report.referenceSHA256 -ine $referenceHash){throw 'INCORRECT_EVIDENCE_REPORT'}
$results.Add('PASS report distinguishes static evidence from pending art/native approval')
Invoke-Candidate (Join-Path $scratch 'bad-source') ('0'*64) $referenceHash 'SOURCE_HASH_MISMATCH'
Invoke-Candidate (Join-Path $scratch 'bad-reference') $sourceHash ('0'*64) 'REFERENCE_HASH_MISMATCH'
Invoke-Candidate $first $sourceHash $referenceHash 'OUTPUT_EXISTS'
$outside=Join-Path $repo ('Content/Tiles/ForbiddenMaskTest-'+[guid]::NewGuid().ToString('N'))
Invoke-Candidate $outside $sourceHash $referenceHash 'OUTPUT_OUTSIDE_CANDIDATES'
foreach($path in @((Join-Path $scratch 'bad-source'),(Join-Path $scratch 'bad-reference'),$outside)){
    if(Test-Path -LiteralPath $path){throw "REJECTED_PREFLIGHT_CREATED_OUTPUT: $path"}
}
Add-Type -AssemblyName System.Drawing
$references=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime')
$references+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $references -TypeDefinition (Get-Content -LiteralPath (Join-Path $PSScriptRoot 'TerrainMaskMaterialCompiler.cs.txt') -Raw)
$masterPath=Join-Path $first 'color-master.png'
$maskPath=Join-Path $first 'frame-mask.png'
$atlasPath=Join-Path $first 'OssuaryBone-candidate.png'
foreach($case in @('mask','pixels','socket','canvas')){
    $master=[Drawing.Bitmap]::new($masterPath)
    $mask=[Drawing.Bitmap]::new($maskPath)
    $atlas=if($case -eq 'canvas'){[Drawing.Bitmap]::new(18,18)}else{[Drawing.Bitmap]::new($atlasPath)}
    try {
        $expected=switch($case){
            mask { $mask.SetPixel(0,0,[Drawing.Color]::Gray); 'NATIVE_MASK_CHANGED' }
            pixels { $atlas.SetPixel(0,0,[Drawing.Color]::Red); 'MASKED_SOURCE_CHANGED' }
            socket {
                $master.SetPixel(15,7,[Drawing.Color]::Red)
                if($atlas.GetPixel(15,7).A -eq 255){$atlas.SetPixel(15,7,[Drawing.Color]::Red)}
                'COLOR_SOCKET_MISMATCH'
            }
            canvas { 'CANVAS_CHANGED' }
        }
        $mutMaster=Join-Path $scratch "$case-master.png"
        $mutMask=Join-Path $scratch "$case-mask.png"
        $mutAtlas=Join-Path $scratch "$case-atlas.png"
        $master.Save($mutMaster,[Drawing.Imaging.ImageFormat]::Png)
        $mask.Save($mutMask,[Drawing.Imaging.ImageFormat]::Png)
        $atlas.Save($mutAtlas,[Drawing.Imaging.ImageFormat]::Png)
        $caught=$false
        try {[void][TerrainMaskMaterialCompiler]::Verify($mutMaster,$mutMask,$mutAtlas,$reference)}
        catch {if($_.ToString() -notmatch $expected){throw}; $caught=$true}
        if(-not $caught){throw "MUTATION_SURVIVED_$case"}
        $results.Add("PASS verifier rejects $expected")
    } finally {$master.Dispose(); $mask.Dispose(); $atlas.Dispose()}
}
if((Get-FileHash -LiteralPath $production).Hash -ne $productionHash -or
   (Get-FileHash -LiteralPath $source).Hash -ne $sourceHash -or
   (Get-FileHash -LiteralPath $reference).Hash -ne $referenceHash){throw 'INPUT_OR_PRODUCTION_CHANGED'}
$results.Add('PASS source, native reference and production atlas unchanged')
# Retain unique bounded scratch evidence; no recursive deletion or repo writes.
[ordered]@{schemaVersion=2; sourceSHA256=$sourceHash; referenceSHA256=$referenceHash; checks=@($results); count=$results.Count; nativeRendered=$false; artApproved=$false} |
    ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $scratch 'checks.json') -Encoding utf8
$results
Write-Output "Evidence: $scratch"
