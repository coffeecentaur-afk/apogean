#requires -Version 7.2
<#
.SYNOPSIS
Checks the real Maw seed generation scope and rejects removed/bypassed guards.
.DESCRIPTION
Focused C# source contracts, not a replacement lifecycle model or native proof.
Roslyn parses the actual members; comments and string contents cannot satisfy
guard checks. Mutations exist only in memory. No game, build, install or writes.
An intentional scope refactor may require review of these explicit contracts.
#>
[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# PowerShell's own compiler supplies Roslyn; no additional dependency/install.
if (-not ('Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree' -as [type])) {
    Add-Type -TypeDefinition 'internal sealed class MawSeedScopeSyntaxBootstrap {}'
}
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$paths = [ordered]@{
    World = 'Common/WorldGeneration/MawSeedWorld.cs'
    Plan = 'Common/WorldGeneration/ApogeanWorldPlan.cs'
    Generator = 'Common/WorldGeneration/MawRuptureGenerator.cs'
    Passes = 'Common/WorldGeneration/ApogeanWorldGenerationSystem.cs'
    Engraft = 'Content/World/EngraftSystem.cs'
}
$sources = @{}
$hashes = @{}
foreach ($key in $paths.Keys) {
    $path = Join-Path $root $paths[$key]
    $bytes = [IO.File]::ReadAllBytes($path)
    $sources[$key] = [Text.Encoding]::UTF8.GetString($bytes).TrimStart([char]0xFEFF)
    $hashes[$key] = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))
}
function Parse-Source([string]$Text) {
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($Text)
    $errors = @($tree.GetDiagnostics() | Where-Object Severity -eq Error)
    if ($errors.Count) { throw "Source/mutation must parse before its guards can be tested: $($errors -join '; ')" }
    return $tree.GetRoot()
}
function Find-Member($Tree, [string]$Name) {
    $found = @($Tree.DescendantNodes() | Where-Object {
        ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax] -or
         $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax]) -and $_.Identifier.ValueText -eq $Name
    })
    if ($found.Count -ne 1) { throw "Expected one actual member '$Name'; found $($found.Count). Review the scope seam." }
    return $found[0]
}
function Code($Node) {
    if ($null -eq $Node) { return '' }
    return -join @($Node.DescendantTokens() | ForEach-Object {
        if (([Microsoft.CodeAnalysis.CSharp.SyntaxKind]$_.RawKind).ToString() -match 'String') { '""' } else { $_.Text }
    })
}
function First-Code($Member) {
    if ($null -ne $Member.Body -and $Member.Body.Statements.Count) { return Code $Member.Body.Statements[0] }
    return Code $Member.ExpressionBody.Expression
}
function Get-Contracts([hashtable]$Text) {
    $world = Parse-Source $Text.World
    $m = @{}
    foreach ($name in @('WorldName','Requested','IsTestWorldName','TryNameSeed','PreWorldGen','BeginPlanning','TryPlan',
        'ApplyPlanned','WriteCell','PostWorldGen','DisarmGeneration','LoadWorldData','SaveWorldData','ClearWorld','OnWorldLoad','OnWorldUnload','NativePassed')) {
        $m[$name] = Find-Member $world $name
    }
    $pre = Code $m.PreWorldGen.Body
    $begin = Code $m.BeginPlanning.Body
    $post = $m.PostWorldGen
    $tries = @($post.Body.Statements | Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax] })
    $postTry = if ($tries.Count -eq 1) { $tries[0] } else { $null }
    $checks = [ordered]@{}
    $prefix = @($world.DescendantNodes() | Where-Object {
        $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax] -and $_.Identifier.ValueText -eq 'WorldPrefix'
    })
    $nameCode = Code $m.TryNameSeed.Body
    $checks['explicit-name'] = $prefix.Count -eq 1 -and $prefix[0].Initializer.Value.Token.ValueText -ceq 'Apogean Maw Seed QA ' -and
        (Code $m.IsTestWorldName.ExpressionBody.Expression) -eq 'TryNameSeed(name,out_)' -and
        $nameCode.Contains('name.StartsWith(WorldPrefix,StringComparison.Ordinal)') -and
        $nameCode.Contains("suffix.Length>0&&suffix.All(c=>c>='0'&&c<='9')") -and
        $nameCode.Contains('int.TryParse(suffix,NumberStyles.None,CultureInfo.InvariantCulture,outseed)')
    $checks['identity-latch'] = (Code $m.Requested.ExpressionBody.Expression) -eq
        'generationFile!=null&&ReferenceEquals(generationFile,Main.ActiveWorldFileData)&&generationName==WorldName&&IsTestWorldName(generationName)' -and
        (Code $m.WorldName.ExpressionBody.Expression) -eq 'Main.ActiveWorldFileData?.Name??Main.worldName'
    # No legacy engine flag is authoritative at this pass: native dedicated
    # autocreate omits generatingWorld and Final Cleanup has already cleared gen.
    $gateCode = (@('WorldName','Requested','PreWorldGen','BeginPlanning','TryPlan','ApplyPlanned','PostWorldGen','WriteCell') |
        ForEach-Object { Code $m[$_] }) -join ''
    $checks['native-arm-survives-planning'] = $gateCode -notmatch 'WorldGen\.(gen|generatingWorld)\b' -and
        (First-Code $m.PreWorldGen) -eq 'ClearWorld();' -and
        $pre.Contains('if(!IsTestWorldName(WorldName))return;generationFile=Main.ActiveWorldFileData;generationName=WorldName;Require(Requested,') -and
        (First-Code $m.BeginPlanning) -eq 'if(!Requested)return;' -and $begin.Contains('RequireBindings();') -and
        $begin -notmatch 'ClearWorld\(|DisarmGeneration\(|generation(File|Name)='
    $apply = Code $m.ApplyPlanned.Body
    $checks['guarded-candidate-writes'] = (First-Code $m.TryPlan) -eq 'if(!Requested)returntrue;' -and
        (First-Code $m.ApplyPlanned) -eq 'if(!Requested)return;' -and
        (First-Code $m.WriteCell).StartsWith('Require(Requested,') -and
        $apply.Contains('Require(p!=null&&ReferenceEquals(p.Rupture,rupture)&&record==null,') -and
        $apply.IndexOf('Require(p!=null') -lt $apply.IndexOf('WriteCell(') -and
        $apply.Contains('generatedThisSession=true;')
    $planning = Code $m.TryPlan.Body
    $retainedAt = $planning.IndexOf('pending=placementwith{ApprovedImpact=impact};')
    $subset = @( $m.ApplyPlanned.Body.Statements | Where-Object {
        $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax] -and
        (Code $_).StartsWith('for(inti=0;i<impact.Length;i++)Require((impact[i]&~p.ApprovedImpact[i])==0,')
    })
    $limitGuard = 'Require(p.ApprovedImpact!=null&&p.ApprovedImpact.Length==impact.Length,'
    $subsetAt = if ($subset.Count -eq 1) { $apply.IndexOf((Code $subset[0])) } else { -1 }
    $beforeWrites = $subsetAt -ge 0
    foreach ($write in @('record=MakeRecord(', 'WriteCell(', 'FrameOwned(', 'WorldGen.PlaceObject(',
        'WorldGen.PlaceChest(', 'WorldGen.SquareTileFrame(', 'tile.HasTile=', 'Main.chest[')) {
        $at = $apply.IndexOf($write)
        if ($at -ge 0 -and $at -lt $subsetAt) { $beforeWrites = $false }
    }
    $checks['approved-impact-before-writes'] =
        $planning.Contains('byte[]impact=ImpactMask(scope,MakeMask(placement));') -and
        $planning.Contains('foreach(RectanglespaninSpans(scope,impact))Require(WorldAtlasPlanner.CanReserve(span,0),') -and
        $planning.Contains('if(Owned(scope,impact,x,y))Require(!ProtectedCell(x,y),') -and
        $retainedAt -gt $planning.IndexOf('WorldAtlasPlanner.CanReserve(span,0)') -and
        $retainedAt -gt $planning.IndexOf('Require(!ProtectedCell(x,y),') -and
        $apply.Contains('byte[]mask=MakeMask(p);byte[]impact=ImpactMask(p.Scope,mask);' + $limitGuard) -and
        $subsetAt -gt $apply.IndexOf($limitGuard) -and $beforeWrites
    $applyTries = @($m.ApplyPlanned.Body.Statements | Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax] })
    $ownedRollback = $false
    if ($applyTries.Count -eq 1 -and $applyTries[0].Catches.Count -eq 1) {
        $rollback = $applyTries[0].Catches[0].Block
        $restores = @($rollback.DescendantNodes() | Where-Object {
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -and (Code $_.Expression).EndsWith('.Restore')
        })
        $ownedRollback = $restores.Count -eq 1 -and (Code $rollback).Contains(
            'if(Owned(p.Scope,mask,x,y))original[x-p.Scope.X,y-p.Scope.Y].Restore(x,y);') -and
            (Code $rollback).Contains('foreach(intidincreatedChests)Main.chest[id]=null;') -and
            (Code $rollback.Statements[$rollback.Statements.Count - 1]) -eq 'throw;'
    }
    $checks['owned-catch-rollback'] = $ownedRollback
    $checks['post-requires-candidate'] = (First-Code $post) -eq 'if(generationFile==null)return;' -and
        $null -ne $postTry -and $postTry.Block.Statements.Count -gt 0 -and
        (Code $postTry.Block.Statements[0]).StartsWith('Require(Requested&&generatedThisSession&&NativePassed,') -and
        (Code $m.NativePassed.ExpressionBody.Expression).StartsWith('HasLayout&&') -and
        (Code $postTry.Block).Contains('VerifyOriginal()') -and (Code $postTry.Block).Contains('Require(route.Passed,')
    $checks['disarm-all-exits'] = (Code $m.DisarmGeneration.Body) -eq '{generationFile=null;generationName=null;}' -and
        (First-Code $m.LoadWorldData) -eq 'DisarmGeneration();' -and (First-Code $m.ClearWorld) -eq 'DisarmGeneration();' -and
        (First-Code $m.OnWorldLoad) -eq 'DisarmGeneration()' -and (First-Code $m.OnWorldUnload) -eq 'ClearWorld()' -and
        $null -ne $postTry -and $null -ne $postTry.Finally -and (Code $postTry.Finally.Block) -eq '{DisarmGeneration();}'
    $passive = $true
    $allowed = @{
        LoadWorldData = @('DisarmGeneration','tag.ContainsKey','tag.GetCompound')
        ClearWorld = @('DisarmGeneration')
        OnWorldLoad = @('DisarmGeneration')
        OnWorldUnload = @('ClearWorld')
        SaveWorldData = @('Fingerprint','Mod.Logger.Warn')
    }
    foreach ($name in $allowed.Keys) {
        foreach ($call in @($m[$name].DescendantNodes() | Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] })) {
            if ((Code $call.Expression) -notin $allowed[$name]) { $passive = $false }
        }
        if ((Code $m[$name]) -match '\b(generationFile|generationName)\b') { $passive = $false }
    }
    # The transient identity must never be serialized or armed by another method.
    foreach ($assignment in @($world.DescendantNodes() | Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax] })) {
        if ((Code $assignment.Left) -notin @('generationFile','generationName') -or (Code $assignment.Right) -eq 'null') { continue }
        $owner = $assignment.Ancestors() | Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax] } | Select-Object -First 1
        if ($null -eq $owner -or $owner.Identifier.ValueText -ne 'PreWorldGen') { $passive = $false }
    }
    $checks['load-save-passive'] = $passive -and (Code $m.LoadWorldData).Contains('generatedThisSession=false;')
    $plan = Parse-Source $Text.Plan
    $generator = Parse-Source $Text.Generator
    $passes = Parse-Source $Text.Passes
    $engraft = Parse-Source $Text.Engraft
    $dispatch = Code (Find-Member $generator 'Generate').Body
    $checks['native-dispatch-wired'] = (Code (Find-Member $passes 'ModifyWorldGenTasks')).Contains('EngraftSystem.Instance.GenerateWorld') -and
        (Code (Find-Member $engraft 'GenerateWorld')).Contains('ApogeanWorldPlanSystem.Instance.CreateWorldGenPlan(') -and
        (Code (Find-Member $engraft 'GenerateWorld')).Contains('MawRuptureGenerator.Generate(') -and
        (First-Code (Find-Member $plan 'CreateWorldGenPlanFromSeed')) -eq 'MawSeedWorld.Instance.BeginPlanning();' -and
        (Code (Find-Member $plan 'FindRuptureSite')).Contains('if(major&&!MawSeedWorld.Instance.TryPlan(') -and
        $dispatch.Contains('if(rupture.IsMajor){MawSeedWorld.Instance.BeforeLegacy(rupture);GenerateFeedingWound(rupture,random,seed,intent);MawSeedWorld.Instance.ApplyPlanned(rupture);}')
    return $checks
}
function Replace-Node([string]$Text, $Node, [string]$Replacement) {
    return $Text.Substring(0, $Node.SpanStart) + $Replacement + $Text.Substring($Node.Span.End)
}
$baseline = Get-Contracts $sources
$failures = [Collections.Generic.List[string]]::new()
foreach ($name in $baseline.Keys) {
    if (-not $baseline[$name]) { $failures.Add("contract/$name") }
    Write-Output "$(if ($baseline[$name]) { 'PASS' } else { 'FAIL' }) $name"
}
# Every control changes actual parsed source, and must fail its specific check.
# A baseline failure is reported as an unavailable control, never a fake rejection.
$worldTree = Parse-Source $sources.World
$generatorTree = Parse-Source $sources.Generator
$controls = @(
    @{ Name='accept-arbitrary-world-names'; Check='explicit-name'; Member='IsTestWorldName'; Replace='internal static bool IsTestWorldName(string name) => name != null;' }
    @{ Name='drop-file-identity'; Check='identity-latch'; Member='Requested'; Replace='private bool Requested => generationFile != null && generationName == WorldName && IsTestWorldName(generationName);' }
    @{ Name='omit-native-arm'; Check='native-arm-survives-planning'; Member='PreWorldGen'; Replace='public override void PreWorldGen() { ClearWorld(); }' }
    @{ Name='legacy-generatingWorld-return'; Check='native-arm-survives-planning'; Member='BeginPlanning'; Replace='internal void BeginPlanning() { if (!WorldGen.generatingWorld) return; if (!Requested) return; RequireBindings(); }' }
    @{ Name='reset-latch-during-planning'; Check='native-arm-survives-planning'; Member='BeginPlanning'; Replace='internal void BeginPlanning() { ClearWorld(); if (!Requested) return; RequireBindings(); }' }
    @{ Name='unguarded-writer'; Check='guarded-candidate-writes'; Member='WriteCell'; First='' }
    @{ Name='unguarded-apply'; Check='guarded-candidate-writes'; Member='ApplyPlanned'; First='' }
    @{ Name='legacy-success-flag-skip'; Check='post-requires-candidate'; Member='PostWorldGen'; First='if (!generatedThisSession) return;' }
    @{ Name='missing-required-candidate'; Check='post-requires-candidate'; Member='PostWorldGen'; Required=';' }
    @{ Name='missing-finally-disarm'; Check='disarm-all-exits'; Member='PostWorldGen'; Finally='{}' }
    @{ Name='missing-load-disarm'; Check='disarm-all-exits'; Member='LoadWorldData'; First='' }
    @{ Name='load-starts-generation'; Check='load-save-passive'; Member='OnWorldLoad'; Replace='public override void OnWorldLoad() { DisarmGeneration(); PreWorldGen(); }' }
    @{ Name='missing-candidate-dispatch'; Check='native-dispatch-wired'; File='Generator'; Member='Generate'; Dispatch=$true }
    @{ Name='missing-BeforeLegacy'; Check='native-dispatch-wired'; File='Generator'; Member='Generate'; BeforeLegacy='missing' }
    @{ Name='BeforeLegacy-after-legacy'; Check='native-dispatch-wired'; File='Generator'; Member='Generate'; BeforeLegacy='late' }
    @{ Name='approved-impact-not-retained'; Check='approved-impact-before-writes'; Member='TryPlan'; Impact='retain' }
    @{ Name='missing-impact-subset-guard'; Check='approved-impact-before-writes'; Member='ApplyPlanned'; Impact='missing' }
    @{ Name='impact-subset-after-write'; Check='approved-impact-before-writes'; Member='ApplyPlanned'; Impact='late' }
    @{ Name='unowned-catch-rollback'; Check='owned-catch-rollback'; Member='ApplyPlanned'; Impact='rollback' }
)
$rejected = 0
foreach ($control in $controls) {
    if (-not $baseline[$control.Check]) {
        Write-Output "UNAVAILABLE control/$($control.Name): its baseline contract already fails."
        continue
    }
    $key = if ($control.ContainsKey('File')) { $control.File } else { 'World' }
    $tree = if ($key -eq 'World') { $worldTree } else { $generatorTree }
    $member = Find-Member $tree $control.Member
    $target = $member
    if ($control.ContainsKey('Replace')) { $replacement = $control.Replace }
    elseif ($control.ContainsKey('First')) { $target = $member.Body.Statements[0]; $replacement = $control.First }
    elseif ($control.ContainsKey('Required')) { $target = $member.Body.Statements[1].Block.Statements[0]; $replacement = $control.Required }
    elseif ($control.ContainsKey('Finally')) { $target = $member.Body.Statements[1].Finally.Block; $replacement = $control.Finally }
    elseif ($control.ContainsKey('Impact')) {
        if ($control.Impact -eq 'retain') {
            $found = @($member.DescendantNodes() | Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax] -and
                (Code $_) -eq 'pending=placementwith{ApprovedImpact=impact};'
            })
            $replacement = 'pending = placement;'
        }
        elseif ($control.Impact -eq 'rollback') {
            $catch = @($member.Body.Statements | Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax] })[0].Catches[0]
            $found = @($catch.DescendantNodes() | Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax] -and (Code $_.Condition) -eq 'Owned(p.Scope,mask,x,y)'
            })
            $replacement = 'original[x - p.Scope.X, y - p.Scope.Y].Restore(x, y);'
        }
        else {
            $found = @($member.Body.Statements | Where-Object {
                $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ForStatementSyntax] -and
                (Code $_).StartsWith('for(inti=0;i<impact.Length;i++)Require((impact[i]&~p.ApprovedImpact[i])==0,')
            })
            $replacement = ';'
        }
        if ($found.Count -ne 1) { throw "Expected one actual impact/rollback guard for $($control.Name)." }
        $target = $found[0]
        if ($control.Impact -eq 'late') {
            $writeLoop = @($member.Body.Statements | Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax] })[0].Block.Statements[0]
            $body = $member.Body
            $replacement = $sources[$key].Substring($body.SpanStart, $body.Span.Length)
            $replacement = $replacement.Insert($writeLoop.Span.End - $body.SpanStart, $target.ToString())
            $replacement = $replacement.Remove($target.SpanStart - $body.SpanStart, $target.Span.Length)
            $target = $body
        }
    }
    elseif ($control.ContainsKey('BeforeLegacy')) {
        $before = @($member.DescendantNodes() | Where-Object {
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax] -and
            (Code $_) -eq 'MawSeedWorld.Instance.BeforeLegacy(rupture);'
        })
        if ($before.Count -ne 1) { throw 'Expected one actual BeforeLegacy call to mutate.' }
        $target = $before[0]
        $replacement = ''
        if ($control.BeforeLegacy -eq 'late') {
            $target = $target.Parent
            $statements = $target.Statements
            if ($statements.Count -ne 3) { throw 'Expected the three guarded major-generation calls.' }
            $replacement = '{' + $statements[1].ToString() + $statements[0].ToString() + $statements[2].ToString() + '}'
        }
    }
    else {
        $target = @($member.DescendantNodes() | Where-Object {
            $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax] -and
            (Code $_) -eq 'MawSeedWorld.Instance.ApplyPlanned(rupture);'
        })[0]
        $replacement = ''
    }
    $mutant = $sources.Clone()
    $mutant[$key] = Replace-Node $sources[$key] $target $replacement
    if ($mutant[$key] -eq $sources[$key]) { throw "Control did not mutate source: $($control.Name)" }
    $result = Get-Contracts $mutant
    if ($result[$control.Check]) { $failures.Add("control/$($control.Name) survived") }
    else { $rejected++; Write-Output "REJECTED control/$($control.Name)" }
}
foreach ($key in $paths.Keys) {
    if ((Get-FileHash -LiteralPath (Join-Path $root $paths[$key])).Hash -ne $hashes[$key]) {
        $failures.Add("source-changed/$key; rerun against the completed edit")
    }
}
Write-Output "$($baseline.Count) source contracts; $rejected/$($controls.Count) negative controls rejected. Source-contract evidence only; native generation/load behavior is separate."
Write-Output "MawSeedWorld SHA256: $($hashes.World)"
if ($failures.Count) { throw "MAW_SEED_SCOPE: $($failures -join '; ')" }
