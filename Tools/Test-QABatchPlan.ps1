Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/QABatchPlan.cs')
$test=@'
namespace apogean.Content.Diagnostics {
 public static class BatchChecks {
  public static string Run() {
   int checks=0;void Check(bool ok){if(!ok)throw new System.Exception("QA_BATCH_PLAN");checks++;}
   foreach(int total in new[]{0,1,31,32,33,256,768,1023,1024})foreach(int budget in new[]{1,8,17,32}) {
    int cursor=0;
    foreach(var b in QABatchPlan.Create(total,budget)) {Check(b.Start==cursor&&b.Count>0&&b.Count<=budget);cursor+=b.Count;Check(cursor<=total);}
    Check(cursor==total);
   }
   foreach(int total in new[]{-1,1025,int.MaxValue}) {bool caught=false;try{QABatchPlan.Create(total,32);}catch(System.ArgumentOutOfRangeException){caught=true;}Check(caught);}
   foreach(int budget in new[]{-1,0,33,int.MaxValue}) {bool caught=false;try{QABatchPlan.Create(1,budget);}catch(System.ArgumentOutOfRangeException){caught=true;}Check(caught);}
   return $"PASS {checks} finite batch partition assertions. Native timings and update scheduling are separate.";
  }
 }
}
'@
Add-Type -TypeDefinition ($source+"`n"+$test)
[apogean.Content.Diagnostics.BatchChecks]::Run()
