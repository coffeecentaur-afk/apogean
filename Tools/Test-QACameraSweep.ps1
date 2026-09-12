Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/QACameraSweep.cs')
$harness=@'
namespace apogean.Content.Diagnostics {
 public static class CameraSweepChecks {
  public static string Run() {
   int checks=0; void Check(bool v){if(!v)throw new System.Exception("CAMERA_SWEEP_ASSERT");checks++;}
   var previous=QACameraSweep.Offset(0,true);
   for(int i=0;i<=3200;i++) {
    double t=i/100.0; var p=QACameraSweep.Offset(t,true); var still=QACameraSweep.Offset(t,false);
    Check(float.IsFinite(p.X)&&float.IsFinite(p.Y)); Check(System.Math.Abs(p.X)<=160&&System.Math.Abs(p.Y)<=320);
    Check(still.X==0&&still.Y==0); Check(System.Math.Abs(p.X-previous.X)<1.01&&System.Math.Abs(p.Y-previous.Y)<2.02);
    previous=p;
   }
   Check(QACameraSweep.Offset(2.5,true)==(160f,320f)); Check(QACameraSweep.Offset(7.5,true)==(-160f,-320f));
   Check(QACameraSweep.Offset(100,true)==QACameraSweep.Offset(32,true));
   foreach(double t in new[]{-1,double.NaN,double.PositiveInfinity,double.NegativeInfinity}) {
    bool caught=false;try{QACameraSweep.Offset(t,true);}catch(System.ArgumentOutOfRangeException){caught=true;}Check(caught);
   }
   return $"PASS {checks} camera path assertions (sampled bounds/continuity, static control, finite expiry). Synthetic camera only, not native movement evidence.";
  }
 }
}
'@
Add-Type -TypeDefinition ($source+"`n"+$harness)
[apogean.Content.Diagnostics.CameraSweepChecks]::Run()
