param(
    [Parameter(Mandatory)]
    [ValidateSet('forest-restoration-wastes', 'forest-restoration-mixed', 'forest-restoration-green')]
    [string]$Fixture,
    [Parameter(Mandatory)]
    [ValidateSet('Wastes', 'Green')]
    [string]$ExpectedState,
    [ValidatePattern('^[A-Za-z0-9-]+$')]
    [string]$EvidenceName,
    [string]$TModLoaderRoot = (Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader'),
    [string]$ClientLog = 'E:/SteamLibrary/steamapps/common/tModLoader/tModLoader-Logs/client.log',
    [int]$TimeoutSeconds = 25
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$greenPercent = switch ($Fixture) { 'forest-restoration-wastes' {0} 'forest-restoration-mixed' {50} 'forest-restoration-green' {100} }
$captureName = "Apogean Forest Restoration $greenPercent Percent Capture Probe"
$capturePath = Join-Path $TModLoaderRoot "Captures/$captureName.png"
$startedAt = Get-Date
$offset = (Get-Item -LiteralPath $ClientLog).Length
$stream = [IO.FileStream]::new($ClientLog, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
$reader = [IO.StreamReader]::new($stream)
$clock = [Diagnostics.Stopwatch]::StartNew()
$appended = ''
try {
    [void]$stream.Seek($offset, [IO.SeekOrigin]::Begin)
    & (Join-Path $PSScriptRoot 'Request-LiveValidation.ps1') -Fixture $Fixture -TModLoaderRoot $TModLoaderRoot
    do {
        $appended += $reader.ReadToEnd()
        if ($appended -match 'LIVE VALIDATION REQUEST FAILED') { throw "Runtime rejected the fixture: $appended" }
        $metric = $appended -split '\r?\n' | Where-Object { $_ -like "*FOREST RESTORATION:*output=$captureName" } | Select-Object -Last 1
        $routing = $appended -split '\r?\n' | Where-Object { $_ -like '*TILE LAB CAPTURE PROBE:*' } | Select-Object -Last 1
        $viewport = $appended -split '\r?\n' | Where-Object { $_ -like "*TILE LAB VIEWPORT:*output=$captureName" } | Select-Object -Last 1
        if ($metric -and $routing -and $viewport -and (Test-Path -LiteralPath $capturePath)) {
            $captureFile = Get-Item -LiteralPath $capturePath
            if ($captureFile.LastWriteTime -ge $startedAt -and $captureFile.Length -gt 32) {
                # Do not accept a PNG while the capture camera is still writing it.
                $png = [IO.File]::ReadAllBytes($capturePath)
                $signature = [BitConverter]::ToString($png, 0, 8)
                $ending = [BitConverter]::ToString($png, $png.Length - 12, 12)
                if ($signature -eq '89-50-4E-47-0D-0A-1A-0A' -and $ending -eq '00-00-00-00-49-45-4E-44-AE-42-60-82') { break }
            }
        }
        Start-Sleep -Milliseconds 250
    } while ($clock.Elapsed.TotalSeconds -lt $TimeoutSeconds)
    if (-not $metric -or -not $routing -or -not $viewport -or $clock.Elapsed.TotalSeconds -ge $TimeoutSeconds) {
        throw "No fresh completed fixture within $TimeoutSeconds seconds. Leave the disposable single-player world running and unpaused."
    }
    Write-Host $metric
    Write-Host $routing
    Write-Host $viewport
    if ($metric -notmatch 'fraction=([0-9.]+);.*living selected=(True|False)') { throw 'Missing restoration metrics' }
    $fraction = [double]::Parse($Matches[1], [Globalization.CultureInfo]::InvariantCulture)
    $selectedGreen = $Matches[2] -eq 'True'
    if ($routing -notmatch 'detected biome=Forest; render lab=off;') { throw 'Not real, unforced Forest routing' }
    if ([math]::Abs($fraction - $greenPercent/100.0) -gt 0.06) { throw "Fixture contamination: planted $greenPercent%, measured $($fraction * 100)%" }
    if ($selectedGreen -ne ($ExpectedState -eq 'Green')) { throw "Expected $ExpectedState; green selection is $selectedGreen" }
    # These assertions prove policy/routing and that a fresh PNG exists, not artistic quality.
    if ($EvidenceName) {
        $evidenceDirectory = Join-Path $projectRoot ('Art/Validation/ForestRestoration/' + (Get-Date).ToString('yyyy-MM-dd'))
        New-Item -ItemType Directory -Path $evidenceDirectory -Force | Out-Null
        $destination = Join-Path $evidenceDirectory "$EvidenceName.png"
        if (Test-Path -LiteralPath $destination) { throw "Evidence already exists: $destination" }
        Copy-Item -LiteralPath $capturePath -Destination $destination
        [pscustomobject]@{Fixture=$Fixture; ExpectedState=$ExpectedState; MeasuredFraction=$fraction; Metrics=$metric; Routing=$routing; Viewport=$viewport; CapturedAt=(Get-Date).ToString('o')} |
            ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidenceDirectory "$EvidenceName.json") -Encoding utf8
    }
    Write-Host "PASS: live $Fixture -> $ExpectedState; inspect the captured PNG separately."
} finally {
    $reader.Dispose()
}
