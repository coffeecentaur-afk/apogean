Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/MawShallowReturnInputs.cs')
$test=@'
namespace apogean.Content.Diagnostics {
 public static class ReturnInputTests {
  public static int Run(){int n=0;void Check(bool p,string why){if(!p)throw new System.Exception("RETURN_INPUT: "+why);n++;}
   for(int tick=0;tick<360;tick++){
    var at=MawShallowReturnInputs.Decide(1110,1062,0,tick);
    Check(!at.Left&&!at.Right,"center stays horizontally quiet");Check(at.Jump==(tick%60<24),"explicit24/36 jump cycle");
    var approach=MawShallowReturnInputs.Decide(944,1062,0,tick);Check(approach.Right&&!approach.Left&&!approach.Jump,"lower passage approach");
    var coast=MawShallowReturnInputs.Decide(1100,1062,2,tick);Check(!coast.Right&&!coast.Left,"native-momentum release");
   }
   foreach(int tick in new[]{-1,360,999})Check(MawShallowReturnInputs.Decide(1110,1062,0,tick)==default,"bounded tick window");
   foreach(float invalid in new[]{float.NaN,float.PositiveInfinity,float.NegativeInfinity}){
    Check(MawShallowReturnInputs.Decide(invalid,1062,0,0)==default,"invalidX");
    Check(MawShallowReturnInputs.Decide(1110,invalid,0,0)==default,"invalidY");
    Check(MawShallowReturnInputs.Decide(1110,1062,invalid,0)==default,"invalid velocity");
   }
   Check(MawShallowReturnInputs.Decide(863,1062,0,0)==default,"left envelope");
   Check(MawShallowReturnInputs.Decide(1441,1062,0,0)==default,"right envelope");
   Check(MawShallowReturnInputs.Decide(1110,575,0,0)==default,"top envelope");
   Check(MawShallowReturnInputs.Decide(1110,1153,0,0)==default,"bottom envelope");
   Check(MawShallowReturnInputs.Decide(1110,54*16-42,0,0).Right,"upper bend target");
   Check(MawShallowReturnInputs.Decide(1270,43*16-42,0,0).Right,"last pocket target");
   return n;
  }
 }
}
'@
Add-Type -TypeDefinition ($source+"`n"+$test)
$count=[apogean.Content.Diagnostics.ReturnInputTests]::Run()
# The independent pulse count/control expectations reject a held-jump bug.
$bad=$source.Replace('MawShallowReturnInputs','BadReturnInputs').Replace('tick % 60 < 24','true')
$badTest=$test.Replace('MawShallowReturnInputs','BadReturnInputs').Replace('ReturnInputTests','BadReturnTests')
Add-Type -TypeDefinition ($bad+"`n"+$badTest)
$rejected=$false
try{[apogean.Content.Diagnostics.BadReturnTests]::Run()|Out-Null}catch{if($_.Exception.ToString() -match 'RETURN_INPUT:'){$rejected=$true}else{throw}}
if(-not $rejected){throw 'Held-jump mutation survived.'}
Write-Output "PASS $count pure input decisions plus held-jump negative control. Not native jumping, route viability or gameplay approval."
