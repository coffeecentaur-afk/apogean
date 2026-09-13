Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$root=Split-Path -Parent $PSScriptRoot
# tML's default BuildMod after-target writes Mods/apogean.tmod. Code-only checks
# explicitly disable it; installing candidates still uses the pinned wrapper.
& dotnet build $root --no-restore -p:BuildMod=false -p:TargetFramework=net8.0 -p:LangVersion=12.0 -p:PlatformTarget=AnyCPU
if($LASTEXITCODE -ne 0){exit $LASTEXITCODE}
Write-Output 'C# compile only. No tmod packaging/install or art/runtime acceptance.'
