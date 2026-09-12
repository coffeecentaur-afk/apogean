param(
 [Parameter(Mandatory)][ValidatePattern('^[a-z0-9-]{1,128}$')][string]$Request,
 [string]$CaptureDirectory=(Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'My Games/Terraria/tModLoader/Captures')
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$directory=(Resolve-Path -LiteralPath $CaptureDirectory).Path
$target=Join-Path $directory 'ApogeanLiveValidation.request'
if(Test-Path -LiteralPath $target){throw 'A native request is pending; it was not overwritten.'}
# Same-directory rename publishes a CLOSED complete file. The old CreateNew /
# FileShare.Read writer exposed its live handle to File.ReadAllText, which opens
# with a sharing mode incompatible with an existing writer on Windows.
$staging=Join-Path $directory ('ApogeanLiveValidation.'+[Guid]::NewGuid().ToString('N')+'.staging')
if([IO.Path]::GetDirectoryName([IO.Path]::GetFullPath($staging)) -ne [IO.Path]::GetFullPath($directory)) {throw 'Invalid staging boundary.'}
try {
 $stream=[IO.File]::Open($staging,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
 try {$bytes=[Text.Encoding]::UTF8.GetBytes($Request);$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}
 [IO.File]::Move($staging,$target) # overwrite=false, including a competing sender
} finally {
 # Only this uniquely owned temporary staging file; never delete target/pending requests.
 if([IO.File]::Exists($staging)){[IO.File]::Delete($staging)}
}
Write-Output "Published complete request $Request. Await a fresh native result."
