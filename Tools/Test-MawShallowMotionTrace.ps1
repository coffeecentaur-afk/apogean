param([string]$Path,[switch]$SelfTest)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'

function Test-Trace($record) {
    if($record.schemaVersion -notin @(1,2) -or $record.world -ne 'Apogee Native Visual V3' -or $record.player -notin @('gg','Maw QA Plain')) { throw 'MOTION_TRACE: context' }
    $plain=$record.player -eq 'Maw QA Plain'
    if($plain) {
        if($record.schemaVersion -ne 2 -or $record.baselineStart -isnot [bool] -or $record.baselineEnd -isnot [bool] -or
            -not $record.baselineStart -or -not $record.baselineEnd -or
            $record.equipment -notmatch '^(0:0,){20}$' -or $record.startLife -gt 100 -or $record.endLife -gt 100) {
            throw 'MOTION_TRACE: missing or invalid plain-character evidence'
        }
    }
    $rows=@($record.samples)
    if($rows.Count -lt 30 -or $rows.Count -gt 360 -or $record.reason -ne 'landed' -or -not $record.pass) { throw 'MOTION_TRACE: incomplete' }
    if($record.controlled -lt $rows.Count-1 -or $record.controlled -gt $rows.Count+1) { throw 'MOTION_TRACE: missing real controls' }
    if($record.scene.Width -ne 112 -or $record.scene.Height -ne 104) { throw 'MOTION_TRACE: scene' }
    $right=0; $falling=0; $contacts=0
    for($i=0;$i -lt $rows.Count;$i++) {
        $r=$rows[$i]
        if($r.Tick -ne $i) { throw 'MOTION_TRACE: skipped ticks' }
        if($r.Life -isnot [long] -and $r.Life -isnot [int]) { throw 'MOTION_TRACE: missing or noninteger sample health' }
        if($r.Life -le 0 -or ($plain -and $r.Life -gt 100)) { throw 'MOTION_TRACE: invalid sample health' }
        foreach($field in @('X','Y','Vx','Vy')) { if(-not [double]::IsFinite([double]$r.$field)) { throw 'MOTION_TRACE: nonfinite' } }
        if($r.X -lt 64 -or $r.X -gt 1728 -or $r.Y -lt 0 -or $r.Y -gt 768) { throw 'MOTION_TRACE: out of bounds' }
        if($r.Right) { $right++ }; if($r.Vy -gt .25) { $falling++ }; if($r.Teeth) { $contacts++ }
        if($i -gt 0 -and ([Math]::Abs($r.X-$rows[$i-1].X) -gt 32 -or [Math]::Abs($r.Y-$rows[$i-1].Y) -gt 32)) { throw 'MOTION_TRACE: discontinuity' }
    }
    $first=$rows[0]; $last=$rows[-1]
    if($last.Life -ne $record.endLife -or $record.startLife -le 0) { throw 'MOTION_TRACE: inconsistent health summary' }
    if([Math]::Abs($first.X-320) -gt 16 -or [Math]::Abs($first.Y-150) -gt 3) { throw 'MOTION_TRACE: entry' }
    if($right -lt 10 -or $falling -le 10 -or $last.X-$first.X -lt 130 -or $last.Y-$first.Y -le 200) { throw 'MOTION_TRACE: movement not exercised' }
    # First rib only: final body overlaps its x28..36 shaft and rests above y31..35.
    if($last.X -lt 28*16-20 -or $last.X -gt 37*16 -or $last.Y+42 -lt 31*16-2 -or $last.Y+42 -gt 36*16) { throw 'MOTION_TRACE: wrong landing' }
    foreach($r in $rows[($rows.Count-12)..($rows.Count-1)]) {
        if(-not $r.Grounded -or [Math]::Abs($r.Vy) -ge .001 -or [Math]::Abs($r.Y-$last.Y) -gt 1) { throw 'MOTION_TRACE: unstable landing' }
    }
    [ordered]@{pass=$true; plainBaseline=$plain; ticks=$rows.Count; airborneTicks=$falling; toothContactTicks=$contacts; lifeBefore=$record.startLife; lifeAfter=$record.endLife;
        scope='Trace validation of one actual entry route with existing equipment. Not full descent, independent engine simulation, manual feel or difficulty acceptance.'}
}

if($SelfTest -or -not $Path) {
    # Synthetic validator controls only; never report these as native movement.
    $rows=@(for($i=0;$i -lt 80;$i++) {
        $x=320+[Math]::Min(200,$i*5); $y=150+[Math]::Min(320,[Math]::Max(0,($i-35)*10))
        [pscustomobject]@{Tick=$i;X=$x;Y=$y;Vx=0;Vy=$(if($i -gt 35 -and $i -lt 68){10}else{0});Right=($i -lt 40);Grounded=($i -ge 68);Teeth=$false;Life=500}
    })
    $base=[pscustomobject]@{schemaVersion=1;world='Apogee Native Visual V3';player='gg';reason='landed';pass=$true;controlled=80;scene=@{Width=112;Height=104};startLife=500;endLife=500;samples=$rows}
    $null=Test-Trace $base
    $mutations=@(
        {param($r) $r.world='aga'},
        {param($r) $r.samples[20].Tick=10},
        {param($r) $r.samples[20].Y=700},
        {param($r) $r.samples[0].X=500},
        {param($r) $r.controlled=0},
        {param($r) foreach($s in $r.samples){$s.Vy=0}},
        {param($r) $r.samples[-1].Grounded=$false},
        {param($r) $r.reason='tick-budget'},
        {param($r) $r.samples=$r.samples[0..20]},
        {param($r) $r.samples[-1].Life=1},
        {param($r) $r.samples[20].Life=0},
        {param($r) $r.samples[20].Life=$null}
    )
    foreach($mutation in $mutations) {
        $copy=$base|ConvertTo-Json -Depth 8|ConvertFrom-Json
        & $mutation $copy
        $rejected=$false
        try {$null=Test-Trace $copy} catch {if($_.Exception.Message -notlike 'MOTION_TRACE:*'){throw};$rejected=$true}
        if(-not $rejected){throw 'MOTION_TRACE: defective control was accepted'}
    }
    $plain=$base|ConvertTo-Json -Depth 8|ConvertFrom-Json
    $plain.schemaVersion=2; $plain.player='Maw QA Plain'; $plain.startLife=100; $plain.endLife=100
    foreach($s in $plain.samples){$s.Life=100}
    $plain|Add-Member baselineStart $true
    $plain|Add-Member baselineEnd $true
    $plain|Add-Member equipment ('0:0,'*20)
    $null=Test-Trace $plain
    foreach($mutate in @(
        {param($r) $r.baselineStart=$false}, {param($r) $r.baselineEnd=$false},
        {param($r) $r.equipment='1:0,'+('0:0,'*19)}, {param($r) $r.endLife=500},
        {param($r) $r.player='Maw QA Plain2'}, {param($r) $r.schemaVersion=1},
        {param($r) $r.samples[20].Life=500}, {param($r) $r.baselineStart='true'},
        {param($r) $r.baselineEnd='false'}
    )) {
        $copy=$plain|ConvertTo-Json -Depth 8|ConvertFrom-Json
        & $mutate $copy; $rejected=$false
        try{$null=Test-Trace $copy}catch{if($_.Exception.Message -notlike 'MOTION_TRACE:*'){throw};$rejected=$true}
        if(-not $rejected){throw 'MOTION_TRACE: defective plain control accepted'}
    }
    Write-Output 'PASS two synthetic trace-validator baselines and twenty-one rejected defects. No native game evidence.'
} else {
    if(-not $Path){throw 'Provide a native motion JSON path or -SelfTest.'}
    Test-Trace (Get-Content -Raw -LiteralPath $Path|ConvertFrom-Json)|ConvertTo-Json
}
