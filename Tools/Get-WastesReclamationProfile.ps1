<#
Offline design calculator only. Counts are supplied by the caller; this does
not scan a world, classify blocks, save progress, or change game rendering.
The denominator must remain the original eligible surface measure, not the
number of surviving tiles. Curves are proposed art-stage activation weights,
not approved balance and not a measured percentage of green image pixels.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateRange(0, [long]::MaxValue)]
    [long]$EligibleSurfaceUnits,
    [Parameter(Mandatory = $true)]
    [ValidateRange(0, [long]::MaxValue)]
    [long]$RestoredSurfaceUnits
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($RestoredSurfaceUnits -gt $EligibleSurfaceUnits) {
    throw 'Restored units cannot exceed the fixed eligible surface measure.'
}
if ($EligibleSurfaceUnits -eq 0) {
    return [pscustomobject]@{
        Available = $false; Global = $null; Close = $null; Mid = $null; Far = $null
    }
}

$recovery = [double]([decimal]$RestoredSurfaceUnits / [decimal]$EligibleSurfaceUnits)
# Algebraically equivalent to 1-(1-R)^3 and 1-(1-R)^2, without subtractive
# cancellation at very small positive R. All layers begin growing together.
[pscustomobject]@{
    Available = $true
    Global = $recovery
    Close = $recovery * (3 - 3 * $recovery + $recovery * $recovery)
    Mid = $recovery * (2 - $recovery)
    Far = $recovery
}
