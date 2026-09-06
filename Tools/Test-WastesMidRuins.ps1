Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$candidate = Join-Path $root 'Art/Candidates/WastesMidRuins-v2'
$decision = Get-Content -Raw -LiteralPath (Join-Path $candidate 'Mask-Decision.json') | ConvertFrom-Json
Add-Type -AssemblyName System.Drawing
$refs = @([Drawing.Bitmap].Assembly.Location,[Drawing.Color].Assembly.Location,'System.Runtime','System.Collections','System.Text.Json')
$refs += @(Get-ChildItem -LiteralPath $PSHOME -Filter 'System.Private.Windows*.dll' | Select-Object -ExpandProperty FullName)
Add-Type -ReferencedAssemblies $refs -TypeDefinition @'
using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
public static class MidRuinProof {
    public static void Check(string source,string mask,string prepared,string exported,string changesPath,int fault) {
        using(var original=new Bitmap(source))using(var selection=new Bitmap(mask))
        using(var color=new Bitmap(prepared))using(var result=new Bitmap(exported)) {
            int w=original.Width,h=original.Height;
            if(w!=1024||h!=1536||selection.Width!=w||selection.Height!=h||color.Width!=w||color.Height!=h||result.Width!=w||result.Height!=h)
                throw new InvalidDataException("DIMENSION_CHANGE");
            var changes=JsonSerializer.Deserialize<int[][]>(File.ReadAllText(changesPath));
            var donors=new Dictionary<int,int>();
            foreach(var c in changes) {
                if(c.Length!=4||c[0]<0||c[0]>=w||c[1]<0||c[1]>=h||c[2]<0||c[2]>=w||c[3]<0||c[3]>=h)
                    throw new InvalidDataException("BAD_CHANGE");
                int dx=c[0]-c[2],dy=c[1]-c[3];
                if(dx*dx+dy*dy>36||dx*dx+dy*dy==0)throw new InvalidDataException("DONOR_DISTANCE");
                if(selection.GetPixel(c[0],c[1]).ToArgb()!=Color.White.ToArgb()||selection.GetPixel(c[2],c[3]).ToArgb()!=Color.White.ToArgb())
                    throw new InvalidDataException("REMOVED_DONOR_OR_TARGET");
                bool near=false;
                for(int ay=-2;ay<=2;ay++)for(int ax=-2;ax<=2;ax++) {
                    int x=c[0]+ax,y=c[1]+ay;
                    if(x>=0&&x<w&&y>=0&&y<h&&selection.GetPixel(x,y).ToArgb()==Color.Black.ToArgb())near=true;
                }
                if(!near)throw new InvalidDataException("INTERIOR_RECOLOR");
                donors.Add(c[1]*w+c[0],c[3]*w+c[2]);
            }
            bool injected=false;long removed=0,kept=0;
            for(int y=0;y<h;y++)for(int x=0;x<w;x++) {
                int p=y*w+x,m=selection.GetPixel(x,y).ToArgb();
                if(m!=Color.White.ToArgb()&&m!=Color.Black.ToArgb())throw new InvalidDataException("NONBINARY_MASK");
                int donor=donors.TryGetValue(p,out int d)?d:p;
                int expected=original.GetPixel(donor%w,donor/w).ToArgb();
                int actualColor=color.GetPixel(x,y).ToArgb();
                if(fault==3&&!injected&&donors.ContainsKey(p)){actualColor^=1;injected=true;}
                if(actualColor!=expected)throw new InvalidDataException("COLOR_PROVENANCE");
                int actual=result.GetPixel(x,y).ToArgb();
                if(m==Color.Black.ToArgb()) {
                    removed++;
                    if(fault==1&&!injected){actual=Color.White.ToArgb();injected=true;}
                    if(actual!=0)throw new InvalidDataException("MATTE_LEAK");
                } else {
                    kept++;
                    if(fault==2&&!injected){actual^=1;injected=true;}
                    if(actual!=actualColor)throw new InvalidDataException("KEPT_RGB_CHANGE");
                }
            }
            if(removed==0||kept==0)throw new InvalidDataException("EMPTY_OR_OPAQUE");
            if(fault!=0)throw new InvalidDataException("FAULT_NOT_REJECTED");
        }
    }
}
'@
foreach ($source in $decision.sources) {
    $name = $source.name
    $original = if ($name -eq 'BrokenShell') { Join-Path $root 'Art/Candidates/WastesMidRuin-v1/Concept-original.png' } else { Join-Path $candidate "$name-original.png" }
    if ((Get-FileHash -LiteralPath $original).Hash -ne $source.sourceSHA256) { throw 'ORIGINAL_CHANGED' }
    $mask = Join-Path $candidate "Prepared-v1/$name-Mask.png"
    $prepared = Join-Path $candidate "Prepared-v1/$name-EdgeSource.png"
    $exported = Join-Path $candidate "Transparent-v1/$name.png"
    $changes = Join-Path $candidate "Prepared-v1/$name-edge-changes.json"
    $report = Get-Content -Raw -LiteralPath "$exported.report.json" | ConvertFrom-Json
    foreach ($pair in @(@($mask,'maskSHA256'),@($prepared,'sourceSHA256'),@($exported,'outputSHA256'))) {
        if ((Get-FileHash -LiteralPath $pair[0]).Hash -ne $report.($pair[1])) { throw 'EXPORT_HASH_CHANGED' }
    }
    [MidRuinProof]::Check($original,$mask,$prepared,$exported,$changes,0)
    Write-Output "PASS $name : exact export, hard alpha, zero removed RGB, unchanged interior, edge donors within6px."
    foreach ($fault in @(1,2,3)) {
        $expected = @('','MATTE_LEAK','KEPT_RGB_CHANGE','COLOR_PROVENANCE')[$fault]
        $rejected = $false
        try { [MidRuinProof]::Check($original,$mask,$prepared,$exported,$changes,$fault) }
        catch { if ($_.Exception.ToString().Contains($expected)) { $rejected=$true } else { throw } }
        if (!$rejected) { throw "FAULT_NOT_REJECTED_$fault" }
        Write-Output "PASS in-memory negative control $name / $expected (no files mutated)."
    }
}
Write-Output 'Scope: pixel provenance/export only. No semantic, scale, live-render or production approval.'
