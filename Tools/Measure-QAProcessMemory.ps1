param(
 [Parameter(Mandatory)][int]$ProcessId,
 [Parameter(Mandatory)][string]$OutputPath,
 [Parameter(Mandatory)][ValidateSet('startup','menu','world-idle','scene','after-probes','after-return')][string]$Phase,
 [ValidateRange(3,120)][int]$Samples=15,
 [ValidateRange(250,5000)][int]$IntervalMilliseconds=1000
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if(Test-Path -LiteralPath $OutputPath){throw 'Refusing to overwrite existing evidence.'}
$qaProcess=Get-Process -Id $ProcessId
$qaExecutable=$qaProcess.Path
if($qaExecutable -ne 'E:\SteamLibrary\steamapps\common\tModLoader\dotnet\dotnet.exe') {throw 'Target is not the installed tModLoader runtime.'}
$qaStarted=$qaProcess.StartTime.ToUniversalTime().ToString('o')
$records=[Collections.Generic.List[object]]::new()
$gpuError=$null
for($sample=0;$sample -lt $Samples;$sample++) {
 $qaProcess=Get-Process -Id $ProcessId
 if($qaProcess.StartTime.ToUniversalTime().ToString('o') -ne $qaStarted){throw 'PID was reused; no combined measurement.'}
 $gpu=@()
 try {
  $gpu=@(Get-CimInstance -ClassName Win32_PerfRawData_GPUPerformanceCounters_GPUProcessMemory -Filter "Name LIKE 'pid[_]$($ProcessId)[_]%'" | Where-Object { $_.Name -match "^pid_$($ProcessId)_" } | ForEach-Object {
   [ordered]@{instance=$_.Name;dedicatedBytes=[long]$_.DedicatedUsage;sharedBytes=[long]$_.SharedUsage;committedBytes=[long]$_.TotalCommitted}
  })
 } catch {$gpuError=$_.Exception.GetType().FullName}
 $records.Add([ordered]@{utc=[DateTime]::UtcNow.ToString('o');privateBytes=$qaProcess.PrivateMemorySize64;workingSetBytes=$qaProcess.WorkingSet64;peakWorkingSetBytes=$qaProcess.PeakWorkingSet64;cpuSeconds=$qaProcess.TotalProcessorTime.TotalSeconds;gpu=$gpu})
 if($sample+1 -lt $Samples){Start-Sleep -Milliseconds $IntervalMilliseconds}
}
$result=[ordered]@{schemaVersion=1;phase=$Phase;processId=$ProcessId;processStartedUtc=$qaStarted;sampleCount=$records.Count;intervalMilliseconds=$IntervalMilliseconds;gpuProvider='Win32_PerfRawData_GPUPerformanceCounters_GPUProcessMemory';gpuError=$gpuError;scope='Read-only process-wide counters. Not exact mod ownership or driver residency guarantees; empty GPU rows mean unavailable, not zero.';records=$records}
$bytes=[Text.Encoding]::UTF8.GetBytes(($result|ConvertTo-Json -Depth 8))
$stream=[IO.File]::Open([IO.Path]::GetFullPath($OutputPath),[IO.FileMode]::CreateNew,[IO.FileAccess]::Write)
try{$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()}
$peakPrivate=($records|ForEach-Object {$_.privateBytes}|Measure-Object -Maximum).Maximum
Write-Output "Recorded $($records.Count) $Phase process samples; peak observed private bytes=$peakPrivate. No limit/pass conclusion."
