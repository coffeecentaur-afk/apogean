Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
# Read-only inventory of the ACTUAL pure layout source. Not a frame/render gate.
# This inventories the natural panel, not the wall bank or lower control rows.
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/MawNaturalLayout.cs')
$harness=@'
public static class MawFixtureInterfaceInventory {
 public static string[] Run() {
  var a=apogean.Content.Diagnostics.MawNaturalLayout.Create();
  var counts=new System.Collections.Generic.SortedDictionary<string,int>();
  var examples=new System.Collections.Generic.Dictionary<string,string>();
  for(int x=1;x<127;x++)for(int y=1;y<49;y++) {
   foreach(var d in new[]{(1,0),(0,1)}) {
    string p=a[x,y].Tile,q=a[x+d.Item1,y+d.Item2].Tile;
    if(p==null||q==null||p==q)continue;
    string key=string.CompareOrdinal(p,q)<0?p+"/"+q:q+"/"+p;
    if(!counts.ContainsKey(key)){counts[key]=0;examples[key]=$"{x},{y}->{x+d.Item1},{y+d.Item2}";}
    counts[key]++;
   }
  }
  var lines=new System.Collections.Generic.List<string>();
  foreach(var row in counts)lines.Add($"{row.Key}: {row.Value} shared tile edges; first local pair {examples[row.Key]}");
  return lines.ToArray();
 }
}
'@
Add-Type -TypeDefinition ($source+$harness)
[MawFixtureInterfaceInventory]::Run()
Write-Output 'Inventory only: no native pixels, world writes or visual acceptance. Coordinates relative to fixture origin.'
