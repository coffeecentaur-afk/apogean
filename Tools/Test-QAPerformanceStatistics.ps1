Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/QASampleStatistics.cs')
$harness=@'
namespace apogean.Content.Diagnostics {
 public static class PerformanceChecks {
  public static string Run() {
   int checks=0; void Require(bool v){if(!v)throw new System.Exception("PERF_STAT_ASSERT");checks++;}
   double[] input=new double[101]; for(int i=0;i<100;i++)input[i]=100-i; input[100]=double.NaN;
   var s=QASampleStatistics.Describe(input,100);
   Require(s.Count==100&&s.MeanMs==50.5&&s.P50Ms==50&&s.P95Ms==95&&s.P99Ms==99&&s.MaxMs==100);
   Require(s.Over33Ms==67&&s.Over50Ms==50); Require(input[0]==100&&input[99]==1);
   Require(QASampleStatistics.Describe(input,0).Count==0);
   Require(QASampleStatistics.Describe(new double[]{16.5},1).P99Ms==16.5);
   foreach(double bad in new[]{double.NaN,double.PositiveInfinity,-1}) {
    bool caught=false;try{QASampleStatistics.Describe(new[]{bad},1);}catch(System.ArgumentException){caught=true;} Require(caught);
   }
   foreach(int n in new[]{-1,102}) {bool caught=false;try{QASampleStatistics.Describe(input,n);}catch(System.ArgumentOutOfRangeException){caught=true;}Require(caught);}
   // Deliberate spike must not disappear into a good average.
   double[] spike=new double[100];System.Array.Fill(spike,16.0);spike[99]=200;
   var p=QASampleStatistics.Describe(spike,100);Require(p.MaxMs==200&&p.Over50Ms==1&&p.P99Ms==16);
   return $"PASS {checks} timing statistics controls; includes outlier, invalid data and unused-buffer guards. No native performance verdict.";
  }
 }
}
'@
Add-Type -TypeDefinition ($source+"`n"+$harness)
[apogean.Content.Diagnostics.PerformanceChecks]::Run()
