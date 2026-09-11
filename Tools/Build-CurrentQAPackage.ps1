param([switch]$CompileOnly,[switch]$KeepWorkspace)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
# Named immutable preset: accepted September 9 pixels with current source code.
# Do not silently repoint this snapshot when a continuity check fails.
$options=@{
    ProjectRoot=$root
    WastesCutoutCandidateDirectory='Art/Candidates/WastesFarCity-v1/QA-Package-v1'
    WastesCityCandidateDirectory='Art/Candidates/WastesFarCity-v1/Runtime-v1'
    WastesScaleCandidateDirectory='Art/Candidates/WastesMidRuins-v2/ScaleStudy-v3'
    WastesDepotAssemblyDirectory='Art/Candidates/WastesMidRuins-v2/ComponentAssembly-v1'
    WastesStationBridgeDirectory='Art/Candidates/WastesStationBridge-v1/Study'
    WastesMidDepthDirectory='Art/Candidates/WastesMidDepth-v2/EdgeReview'
    ArrivalPodCandidateDirectory='Art/Candidates/ArrivalPod-v1/Native-v3'
    MawBoneCandidateDirectory='Art/Candidates/MawBone-v1/MaskedNative-v2'
    MawFangCandidateDirectory='Art/Candidates/MawTooth-v1/Native-v1'
    MawToothArtCandidateDirectory='Art/Candidates/MawToothFamily-v1/Native-v1'
    MawToothClusterCandidateDirectory='Art/Candidates/MawToothCluster-v1/Native-v4'
    AssetSnapshotPath='Tools/Manifests/QAAssets-2026-09-09.json'
    CompileOnly=$CompileOnly
    KeepWorkspace=$KeepWorkspace
}
foreach($key in @($options.Keys)) {
    if($key -like '*Directory' -or $key -eq 'AssetSnapshotPath'){$options[$key]=Join-Path $root $options[$key]}
}
& (Join-Path $PSScriptRoot 'Build-ApogeanIsolated.ps1') @options
