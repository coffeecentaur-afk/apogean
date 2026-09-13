Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/MawShallowTraversalLab.cs')
# Compile the actual reporting expression/helper, not a duplicate implementation.
$m=[regex]::Match($source,'internal static string DescribeHistory\(Rectangle\[\] areas\)\s*\{([\s\S]*?)\n        \}')
if($m.Success){
 $method=$m.Value
 if($source -notmatch 'Mod.Logger.Info\(DescribeHistory\(old\)\)'){throw 'History reporter is not wired into the real command.'}
}else{
 $old=[regex]::Match($source,'Mod.Logger.Info\((\$"MAW SHALLOW PRESERVATION:[^\r\n]+)\);')
 if(-not $old.Success){throw 'No real history reporting path.'}
 $method='internal static string DescribeHistory(Rectangle[] old){return '+$old.Groups[1].Value+';}'
}
$support=@'
using System;
using System.Collections.Generic;
public readonly record struct Rectangle(int X,int Y,int Width,int Height);
public static class Coverage {
'@
$tests=@'
 public static int Run(){
  int n=0;void Check(bool p,string why){if(!p)throw new Exception("HISTORY_COVERAGE: "+why);n++;}
  for(int known=0;known<=17;known++){
   var a=new Rectangle[17];for(int i=0;i<known;i++)a[i]=new Rectangle(50+i*20,50,16,16);
   string text=DescribeHistory(a);
   Check(text.Contains($"{known}/17 nonempty historical bounds"),$"{known} known areas must not be reported as17; actual={text}");
   Check(text.Contains($"{17-known} missing/empty slots"),"explicit unavailable count");
   Check(text.Contains("NOT verified"),"missing coordinates are not proof of preservation");
  }
  Check(DescribeHistory(Array.Empty<Rectangle>()).Contains("0/0 nonempty historical bounds"),"empty input");
  Check(DescribeHistory(new[]{new Rectangle(20,20,0,10),new Rectangle(20,20,10,0)}).Contains("0/2 nonempty historical bounds"),"zero-area locations");
  return n;
 }
}
'@
Add-Type -TypeDefinition ($support+$method+$tests)
$count=[Coverage]::Run()
Write-Output "PASS $count actual history-report checks. Counts known coordinates only; not native tile preservation."
