param([Parameter(Mandatory)][ValidateSet('Masks','Native')][string]$Stage)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
$family=Join-Path $root 'Art/Candidates/MawToothFamily-v1'
$pins=@{short='DE970048243C0B61D42E3692837AEB4760CC8BCD98C8AEB5B065E2EAB1A40D59';wide='B873663803A3B79CF77FF314A09C1F05BF3042162FD5F5730BB569B57879B60E'}
foreach($name in $pins.Keys){if((Get-FileHash (Join-Path $family "$name-source.png")).Hash -ne $pins[$name]){throw 'SOURCE_CHANGED'}}
Add-Type -AssemblyName System.Drawing
$refs=@([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections')
$refs+=@(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition (Get-Content (Join-Path $PSScriptRoot 'MawToothFamilyStudy.cs.txt') -Raw)
Add-Type -ReferencedAssemblies $refs -TypeDefinition (Get-Content (Join-Path $PSScriptRoot 'MawSlimToothStudy.cs.txt') -Raw)
if($Stage -eq 'Masks') {
    foreach($name in @('short','wide')) {
        $mask=Join-Path $family "$name-source-mask.png"
        if(Test-Path -LiteralPath $mask){throw 'MASK_ALREADY_EXISTS'}
        [MawToothFamilyStudy]::ProposeMask((Join-Path $family "$name-source.png"),$mask)
        & pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Export-MaskedBackground.ps1') -SourcePath (Join-Path $family "$name-source.png") -MaskPath $mask -OutputPath (Join-Path $family "$name-cutout.png")
        if($LASTEXITCODE -ne 0){throw 'SOURCE_CUTOUT_FAILED'}
    }
    exit
}
# Review source cutouts BEFORE this explicit second stage.
$maskPins=@{short='243CFD1EE1B7686C6C86F84B15B520F65E1B8E0EA8B24A867B6266E9EF002B3C';wide='9E4034D65883BE68FD76A402A7D33EAC7A51B63D0A449DC46390E4048C894C77'}
foreach($name in $maskPins.Keys){if((Get-FileHash (Join-Path $family "$name-source-mask.png")).Hash -ne $maskPins[$name]){throw 'REVIEWED_MASK_CHANGED'}}
$native=Join-Path $family 'Native-v1'
if(Test-Path -LiteralPath $native){throw 'CANDIDATE_EXISTS'}
$null=New-Item -ItemType Directory -Path $native
$records=@()
foreach($entry in @(@('short',16,32),@('long',16,48),@('wide',32,64))) {
    $name=$entry[0];$width=[int]$entry[1];$height=[int]$entry[2]
    $folder=Join-Path $native $name;$null=New-Item -ItemType Directory -Path $folder
    if($name -eq 'long'){$source=Join-Path $root 'Art/Candidates/MawTooth-v2/source.png';$sourceMask=$null}
    else{$source=Join-Path $family "$name-source.png";$sourceMask=Join-Path $family "$name-source-mask.png"}
    $phase=if($name -eq 'wide'){0.7}else{0.5}
    $fitting=[MawSlimToothStudy]::Prepare($source,$folder,$width,$height,$sourceMask,$phase)
    Write-Host "$name $fitting"
    & pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Export-MaskedBackground.ps1') -SourcePath (Join-Path $folder 'color-master.png') -MaskPath (Join-Path $folder 'mask.png') -OutputPath (Join-Path $folder 'tooth.png')
    if($LASTEXITCODE -ne 0){throw 'NATIVE_EXPORT_FAILED'}
    [MawToothFamilyStudy]::Atlas($folder)
    & pwsh -NoProfile -File (Join-Path $PSScriptRoot 'Test-MawSlimTooth.ps1') -CandidateDirectory $folder -Width $width -Height $height
    if($LASTEXITCODE -ne 0){throw 'FAMILY_STATIC_FAILED'}
    $records+=@{name=$name;canvas=@($width,$height);fitting=$fitting;sourceSHA256=(Get-FileHash $source).Hash;sourceMaskSHA256=$(if($sourceMask){(Get-FileHash $sourceMask).Hash}else{'native-alpha'});spriteSHA256=(Get-FileHash (Join-Path $folder 'tooth.png')).Hash}
}
if((Get-FileHash (Join-Path $native 'long/tooth.png')).Hash -ne 'E2C5E9D6EEB2D6D3EA81DE2B3E26C5532D2BD1449847730838E4B24BFC3DA903'){throw 'LONG_TOOTH_CHANGED'}
$bone=Join-Path $root 'Art/Candidates/MawBone-v1/MaskedNative-v2/OssuaryBone-candidate.png'
$dirt=Join-Path $root 'Content/Tiles/MawDirt.png'
[MawToothFamilyStudy]::Board($native,$bone,$dirt)
@{artOnly=$true;records=$records;rootOcclusionPixels=4;longPreserved=$true;boneSHA256=(Get-FileHash $bone).Hash;dirtSHA256=(Get-FileHash $dirt).Hash} | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $native 'recipe.json') -Encoding utf8
