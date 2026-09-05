param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -Path (Join-Path $ProjectRoot 'Common/Backgrounds/WastesParallaxContract.cs')
$checks = 0
function Assert-Sweep([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw "FAIL: $Message" }
    $script:checks++
}
foreach ($width in 1920,2560) {
    $travel = 8400 * 16 - $width - 3200
    foreach ($layer in 0,1,2) {
        $cycles = [apogean.Common.Backgrounds.WastesParallaxContract]::Repeats($travel,$layer)
        Assert-Sweep ($cycles -ge 2.5) "large-world sweep covers layer $layer at $width"
        Assert-Sweep ($cycles -eq [apogean.Common.Backgrounds.WastesParallaxContract]::Repeats(-$travel,$layer)) 'both directions have equal coverage'
    }
}
Assert-Sweep ([apogean.Common.Backgrounds.WastesParallaxContract]::Repeats(10240,0) -lt 2.5) 'negative control: the old pair of fixed viewpoints is insufficient'
Assert-Sweep ([apogean.Common.Backgrounds.WastesParallaxContract]::Repeats(4200*16-2560-3200,0) -lt 2.5) 'small world cannot silently pass the same full-repeat requirement'
Write-Host "PASS: $checks parallax range checks. Actual rendered range and art still require live evidence."
