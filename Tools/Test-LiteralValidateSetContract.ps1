Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'LiteralValidateSetContract.ps1')
$required=@('conversion','vegetation','wastes-terrain','wastes-properties','material','grass','entity-scale','forest-background','forest-background-aerial','forest-background-night','forest-background-eclipse','desert-background','jungle-background','jungle-routing','snow-background','corruption-background','crimson-background','hallow-background','ocean-background','mushroom-background','underworld-background','kessler-construction','helix-construction','kessler-campus','kessler-world','forest-restoration-wastes','forest-restoration-mixed','forest-restoration-green')
$real=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'Request-LiveValidation.ps1')
Assert-LiteralValidateSet $real Fixture $required
$declaration="[ValidateSet('"+($required -join "', '")+"')]"
function Source([string]$attribute,[string]$parameter='Fixture'){'param('+ $attribute+'[string]$'+$parameter+')'}
$original=Source $declaration
Assert-LiteralValidateSet $original Fixture $required
$reverse=@($required);[array]::Reverse($reverse)
Assert-LiteralValidateSet (Source ("[ValidateSet('"+($reverse -join "',`n '")+"', 'future-mode')]")) Fixture $required
$bad=[Collections.Generic.List[string]]::new()
foreach($value in $required){$bad.Add($original.Replace("'$value'","'misspelled-$value'"))}
$bad.Add((Source $declaration Other))
$bad.Add(('# '+$declaration+"`n"+'param([string]$Fixture)'))
$bad.Add($original.Replace("'conversion'","'conversion','CONVERSION'"))
$bad.Add((Source '[ValidateSet($values)]'))
$bad.Add((Source '[ValidateSet("conversion", IgnoreCase=$false)]'))
$bad.Add('param([ValidateSet(')
foreach($source in $bad){
 $caught=$false
 try{Assert-LiteralValidateSet $source Fixture $required}catch{$caught=$true}
 if(-not $caught){throw 'Malformed declaration accepted.'}
}
# Isolate the named-option guard: conversion alone otherwise satisfies this set.
$refusedOption=$false
try{Assert-LiteralValidateSet (Source '[ValidateSet("conversion", IgnoreCase=$false)]') Fixture @('conversion')}catch{$refusedOption=$true}
if(-not $refusedOption){throw 'Nondefault validation options were accepted without an explicit contract.'}
# The original brittle gate must not silently survive as the actual caller.
$gate=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'Test-WorldVisualIntegrity.ps1')
if($gate -notmatch 'Assert-LiteralValidateSet' -or $gate.Contains($declaration)){
 throw 'VALIDATE_SET_CONTRACT: world-integrity gate still uses the obsolete exact declaration'
}
Write-Output "PASS current/old/reordered+extended declarations and $($bad.Count) malformed controls; actual gate uses the semantic checker. No QA requests sent."
