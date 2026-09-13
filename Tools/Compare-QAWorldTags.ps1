param(
 [Parameter(Mandatory)][string]$BeforePath,
 [Parameter(Mandatory)][string]$AfterPath,
 [Parameter(Mandatory)][string]$OutputPath,
 [switch]$RequireUnchanged
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$runtime='E:/SteamLibrary/steamapps/common/tModLoader'
$dll=Join-Path $runtime 'tModLoader.dll'
if((Get-FileHash -LiteralPath $dll).Hash -ne 'D530E508B2841E66D880CE279A609624B5AB66CE8093EEDFA04F47C3D12D485C'){throw 'TagIO runtime pin changed.'}
if(Test-Path -LiteralPath $OutputPath){throw 'Preserve existing comparison output.'}
$before=(Resolve-Path -LiteralPath $BeforePath).Path
$after=(Resolve-Path -LiteralPath $AfterPath).Path
$beforeHash=(Get-FileHash -LiteralPath $before).Hash
$afterHash=(Get-FileHash -LiteralPath $after).Hash
# Uses the installed, pinned framework's real TagIO reader, not a guessed NBT parser.
# The isolated process never initializes the game or loads mods; files are read-only.
Add-Type -TypeDefinition @'
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
public static class QaWorldTags {
 static MethodInfo read;
 public static void Initialize(string runtime) {
  AssemblyLoadContext.Default.Resolving += (context,name) => {
   var matches=Directory.GetFiles(Path.Combine(runtime,"Libraries"),name.Name+".dll",SearchOption.AllDirectories);
   if(matches.Length!=1) throw new InvalidOperationException("Ambiguous/missing dependency: "+name.Name+" count="+matches.Length);
   return context.LoadFromAssemblyPath(matches[0]);
  };
  var assembly=AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(runtime,"tModLoader.dll"));
  read=assembly.GetType("Terraria.ModLoader.IO.TagIO",true).GetMethod("FromFile",new[]{typeof(string),typeof(bool)});
 }
 static IDictionary<string,object> Compound(object value) => ((IEnumerable<KeyValuePair<string,object>>)value).ToDictionary(p=>p.Key,p=>p.Value,StringComparer.Ordinal);
 public static IDictionary<string,object> Read(string path) => Compound(read.Invoke(null,new object[]{path,true}));
 // Typed, length-prefixed, ordinal-key canonical digest. Lists preserve order.
 // No float formatting, arbitrary ToString(), flattening or missing-value substitution.
 static void Write(BinaryWriter w,object value) {
  if(value==null){w.Write((byte)0);return;}
  if(value is IEnumerable<KeyValuePair<string,object>>){var dict=Compound(value);w.Write((byte)10);w.Write(dict.Count);foreach(var p in dict.OrderBy(p=>p.Key,StringComparer.Ordinal)){w.Write(p.Key);Write(w,p.Value);}return;}
  if(value is byte b){w.Write((byte)1);w.Write(b);return;}
  if(value is short s){w.Write((byte)2);w.Write(s);return;}
  if(value is int i){w.Write((byte)3);w.Write(i);return;}
  if(value is long l){w.Write((byte)4);w.Write(l);return;}
  if(value is float f){w.Write((byte)5);w.Write(f);return;}
  if(value is double d){w.Write((byte)6);w.Write(d);return;}
  if(value is byte[] bytes){w.Write((byte)7);w.Write(bytes.Length);w.Write(bytes);return;}
  if(value is string text){w.Write((byte)8);w.Write(text);return;}
  if(value is int[] ints){w.Write((byte)11);w.Write(ints.Length);foreach(int n in ints)w.Write(n);return;}
  if(value is IList list){w.Write((byte)9);w.Write(value.GetType().FullName);w.Write(list.Count);foreach(var item in list)Write(w,item);return;}
  throw new InvalidDataException("Unrecognized payload: "+value.GetType());
 }
 public static string Hash(object value){using var ms=new MemoryStream();using var w=new BinaryWriter(ms);Write(w,value);w.Flush();return Convert.ToHexString(SHA256.HashData(ms.ToArray()));}
 public static int TestDigest(){
  int cases=0;
  void Equal(object a,object b,bool expected){if((Hash(a)==Hash(b))!=expected)throw new Exception("Canonical digest oracle failure");cases++;}
  Equal(new Dictionary<string,object>{{"b",2},{"a",1}},new Dictionary<string,object>{{"a",1},{"b",2}},true);
  Equal(new Dictionary<string,object>{{"a",1}},new Dictionary<string,object>{{"b",1}},false);
  Equal(new Dictionary<string,object>{{"a",1}},new Dictionary<string,object>(),false);
  Equal(new int[]{1,2},new int[]{2,1},false);Equal(new int[]{1,2},new int[]{1,2},true);
  Equal(new byte[]{1,2},new int[]{1,2},false);Equal((short)1,1,false);Equal(1,1L,false);
  Equal(1f,1d,false);Equal(0d,-0d,false);Equal("1",1,false);
  Equal(new List<int>(),new List<string>(),false);Equal(new List<int>{1,2},new List<int>{2,1},false);
  Equal(new Dictionary<string,object>{{"nested",new int[]{1}}},new Dictionary<string,object>{{"nested",new int[]{2}}},false);
  return cases;
 }
 public static Dictionary<string,IDictionary<string,object>> Systems(string path){
  var root=Read(path);if(!root.TryGetValue("modData",out object entries))throw new InvalidDataException("No modData.");
  var result=new Dictionary<string,IDictionary<string,object>>(StringComparer.Ordinal);
  foreach(object rawEntry in (IList)entries){
   var entry=Compound(rawEntry);
   string id=(string)entry["mod"]+"/"+(string)entry["name"];
   if(!result.TryAdd(id,Compound(entry["data"])))throw new InvalidDataException("Duplicate system "+id);
  }
  return result;
 }
}
'@
[QaWorldTags]::Initialize($runtime)
$digestChecks=[QaWorldTags]::TestDigest()
$a=[QaWorldTags]::Systems($before)
$b=[QaWorldTags]::Systems($after)
$ids=@(@($a.Keys)+@($b.Keys)|Sort-Object -Unique)
$records=@(foreach($id in $ids){
 $presentBefore=$a.ContainsKey($id);$presentAfter=$b.ContainsKey($id)
 $hashA=if($presentBefore){[QaWorldTags]::Hash($a[$id])}else{$null}
 $hashB=if($presentAfter){[QaWorldTags]::Hash($b[$id])}else{$null}
 [ordered]@{system=$id;beforePresent=$presentBefore;afterPresent=$presentAfter;beforeSha256=$hashA;afterSha256=$hashB;equal=$presentBefore -and $presentAfter -and $hashA -eq $hashB;
  beforeKeys=@(if($presentBefore){$a[$id].Keys|Sort-Object});afterKeys=@(if($presentAfter){$b[$id].Keys|Sort-Object})}
})
if((Get-FileHash -LiteralPath $before).Hash -ne $beforeHash -or (Get-FileHash -LiteralPath $after).Hash -ne $afterHash){throw 'Input changed during read.'}
$report=[ordered]@{schemaVersion=1;utc=[DateTime]::UtcNow.ToString('o');reader='Pinned installed TagIO.FromFile(compressed:true)';digest='typed ordinal compound keys, ordered lists, exact numeric bytes; version1';
 before=[ordered]@{path=$before;sha256=$beforeHash};after=[ordered]@{path=$after;sha256=$afterHash};digestChecks=$digestChecks;systems=$records;
 limits='System metadata only. No tile payload equivalence, world geometry, client rendering, multiplayer gameplay or whole-file equality claim.'}
$full=[IO.Path]::GetFullPath($OutputPath)
$stream=[IO.File]::Open($full,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
try{$writer=[IO.StreamWriter]::new($stream);$writer.Write(($report|ConvertTo-Json -Depth 12));$writer.Flush()}finally{$stream.Dispose()}
$records|Where-Object {-not $_.equal}|ForEach-Object {Write-Output "$($_.system): before=$($_.beforePresent), after=$($_.afterPresent), equal=$($_.equal)"}
Write-Output "Recorded $($records.Count) systems; $(@($records|Where-Object {-not $_.equal}).Count) differ. This script reports differences; acceptance requires an explicit system contract."
if($RequireUnchanged -and @($records|Where-Object {-not $_.equal}).Count){throw 'System metadata changed; comparison retained.'}
