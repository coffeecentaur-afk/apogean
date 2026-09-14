#requires -Version 7.2
<#
.SYNOPSIS
Checks Maw seed semantic state and queue-preserving restoration using real source.
.DESCRIPTION
Compiles the extracted CellState (including Read/Restore), actual assertion and
restore receiver against installed tModLoader value types. Executes only value
operations: no Main initialization, tilemap, liquid queue, game, build or install.
The receiver selected for Restore is exercised; native world writes are not.
Actual framing dispatcher source runs against recording native boundaries and
also compiles against the installed APIs; recursive native effects are not run.
Run in a fresh pwsh process. Negative controls mutate source only in memory.
#>
[CmdletBinding()]
param([string]$TModLoaderDirectory = 'E:/SteamLibrary/steamapps/common/tModLoader')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$sourcePath = Join-Path $PSScriptRoot '../Common/WorldGeneration/MawSeedWorld.cs'
$source = Get-Content -Raw -LiteralPath $sourcePath
$sourceHash = (Get-FileHash -LiteralPath $sourcePath).Hash
$nativePath = Join-Path $TModLoaderDirectory 'tModLoader.dll'
$fnaPath = Join-Path $TModLoaderDirectory 'Libraries/FNA/1.0.0/FNA.dll'
$nativeHash = (Get-FileHash -LiteralPath $nativePath).Hash
Add-Type -TypeDefinition 'internal sealed class MawSeedCellStateSyntaxBootstrap {}'
$tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($source)
if (@($tree.GetDiagnostics() | Where-Object Severity -eq Error).Count) { throw 'World source does not parse.' }
$nodes = @($tree.GetRoot().DescendantNodes())
function Member([string]$Name) {
    $found = @($nodes | Where-Object {
        $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax] -and $_.Identifier.ValueText -eq $Name
    })
    if ($found.Count -ne 1) { throw "Expected one method: $Name" }
    return $found[0]
}
function Code($Node) { return -join @($Node.DescendantTokens() | ForEach-Object Text) }
$record = @($nodes | Where-Object {
    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.RecordDeclarationSyntax] -and $_.Identifier.ValueText -eq 'CellState'
})
if ($record.Count -ne 1) { throw 'Expected one actual CellState.' }
$recordSource = $record[0].ToString()
$apply = Member 'ApplyPlanned'
$restore = @($apply.DescendantNodes() | Where-Object {
    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -and
    $_.Expression -is [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
    $_.Parent.Parent -is [Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax] -and
    $_.Expression.Name.Identifier.ValueText -eq 'Restore' -and $_.ArgumentList.ToString() -eq '(x, y)'
})
if ($restore.Count -ne 1) { throw 'Expected one unowned Restore(x, y) call.' }
$restoreStatement = $restore[0].Parent
$siblings = $restoreStatement.Parent.Statements
$restoreIndex = $siblings.IndexOf($restoreStatement)
if ($restoreIndex -lt 2) { throw 'Missing pre-restore assertions.' }
$guard = $siblings[$restoreIndex - 2]
$halo = $siblings[$restoreIndex - 1]
if ($guard -isnot [Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax] -or
    $guard.Expression -isnot [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -or
    $guard.Expression.Expression.ToString() -ne 'Require' -or
    (Code $guard.Expression.ArgumentList.Arguments[0].Expression) -ne 'after.SameNonFrameState(before)') {
    throw 'Semantic equality must be asserted before restoration.'
}
if ((Code $halo) -notlike 'if(!Owned(p.Scope,impact,x,y))Require(after.Equals(before),*') {
    throw 'Exact outside-impact protection changed.'
}
foreach ($name in @('VerifyProtected', 'VerifyProtectedCells')) {
    if (!(Code (Member $name)).Contains('CellState.Read(sample.Position.X,sample.Position.Y).Equals(sample.State)')) {
        throw "Exact protection comparison changed: $name"
    }
}
$receiver = $restore[0].Expression.Expression.ToString()
$requireSource = (Member 'Require').ToString()
$frameSource = (Member 'FrameOwned').ToString()
$spanSource = (Member 'Spans').ToString()
$ownedSource = (Member 'Owned').ToString()
$checks = @'
private static int semanticCases, flagCases;
private static void Check(bool ok, string message) {
    if (!ok) throw new InvalidOperationException(message);
}
private static T Set<T>(T value, string name, object replacement) where T : struct {
    object boxed = value;
    typeof(T).GetProperty(name).SetValue(boxed, replacement);
    return (T)boxed;
}
private static CellState Baseline() => new(new TileTypeData { Type = 1 },
    new WallTypeData { Type = 2 }, default, new LiquidData { Amount = 255, LiquidType = 0 }, default);
private static void Reject(string name, CellState after) {
    var before = Baseline();
    Check(!after.SameNonFrameState(before) && !before.SameNonFrameState(after), "Accepted semantic change: " + name);
    bool rejected = false;
    try { SelectedForRestore(before, after); }
    catch (InvalidOperationException) { rejected = true; }
    Check(rejected, "Restore accepted semantic change: " + name);
    semanticCases++;
}
public static string Run() {
    CheckFramingEntryPoints();
    // Exact observed 404 base state; vary both native processing bits, not water.
    for (int oldFlags = 0; oldFlags < 4; oldFlags++) for (int newFlags = 0; newFlags < 4; newFlags++) {
        var basis = Baseline();
        var before = basis with { Liquid = new LiquidData { Amount = 255, LiquidType = 0,
            SkipLiquid = (oldFlags & 1) != 0, CheckingLiquid = (oldFlags & 2) != 0 } };
        var state = new TileWallWireStateData { TileFrameX = 18, TileFrameY = 36,
            TileFrameNumber = 1, WallFrameX = 36, WallFrameY = 36, WallFrameNumber = 1 };
        var after = basis with { State = state, Liquid = new LiquidData { Amount = 255, LiquidType = 0,
            SkipLiquid = (newFlags & 1) != 0, CheckingLiquid = (newFlags & 2) != 0 } };
        Check(after.SameNonFrameState(before) && before.SameNonFrameState(after), "Rejected transient liquid/frame change");
        Check(!after.Equals(before), "Exact state equality must still detect frame/queue changes");
        var restored = SelectedForRestore(before, after);
        Check(restored.Liquid.Equals(after.Liquid), "Restoration reset live liquid queue flags");
        Check(restored.State.Equals(before.State), "Restoration failed to select original frame state");
        Check(restored.Type.Equals(before.Type) && restored.Wall.Equals(before.Wall) &&
            restored.Coating.Equals(before.Coating), "Restoration changed non-liquid state");
        flagCases++;
    }
    var b = Baseline();
    Reject("tile", b with { Type = new TileTypeData { Type = 2 } });
    Reject("wall", b with { Wall = new WallTypeData { Type = 3 } });
    Reject("liquid amount", b with { Liquid = new LiquidData { Amount = 254, LiquidType = 0 } });
    foreach (int kind in new[] { 1, 2, 3, 63 })
        Reject("liquid type " + kind, b with { Liquid = new LiquidData { Amount = 255, LiquidType = kind } });
    foreach (string field in new[] { "HasTile", "IsActuated", "HasActuator", "IsHalfBlock",
        "RedWire", "BlueWire", "GreenWire", "YellowWire" })
        Reject(field, b with { State = Set(b.State, field, true) });
    Reject("tile paint", b with { State = Set(b.State, "TileColor", (byte)1) });
    Reject("wall paint", b with { State = Set(b.State, "WallColor", (byte)1) });
    for (int slope = 1; slope <= 4; slope++)
        Reject("slope " + slope, b with { State = Set(b.State, "Slope", (Terraria.ID.SlopeType)slope) });
    // Cover all native coating bits, including reserved bits omitted by decoded flags.
    var coatingField = typeof(TileWallBrightnessInvisibilityData).GetField("bitpack",
        BindingFlags.Instance | BindingFlags.NonPublic);
    Check(coatingField != null, "Review changed native coating representation");
    for (int bit = 0; bit < 8; bit++) {
        object boxed = default(TileWallBrightnessInvisibilityData);
        coatingField.SetValue(boxed, (BitsByte)(byte)(1 << bit));
        Reject("coating bit " + bit, b with { Coating = (TileWallBrightnessInvisibilityData)boxed });
    }
    return $"{flagCases} queue-flag transitions preserve live LiquidData; {semanticCases} semantic changes reject before restoration";
}
'@
$framingCheck = @'
private static void CheckFramingEntryPoints() {
    // Run the actual FrameOwned/Spans/Owned source. Only external native entry
    // points are recorded; this does not emulate native framing or recursion.
    var scope = new Microsoft.Xna.Framework.Rectangle(100, 200, 9, 9);
    var mask = new byte[11];
    foreach (int index in new[] { 30, 31, 50 }) mask[index / 8] |= (byte)(1 << (index % 8));
    FrameOwned(scope, mask);
    string actual = string.Join("|", WorldGen.Calls);
    Check(actual == "tile:103,203|wall:103,203|tile:104,203|wall:104,203|tile:105,205|wall:105,205",
        "Framing started outside the three owned cells: " + actual);
}
'@
$references = @($nativePath, $fnaPath) + @([IO.Directory]::GetFiles((Join-Path $PSHOME 'ref'), '*.dll'))
# Load assemblies only; neither Main nor a graphics/game instance is initialized.
$null = [Reflection.Assembly]::LoadFrom($fnaPath)
$null = [Reflection.Assembly]::LoadFrom($nativePath)
Add-Type -TypeDefinition @"
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
public static class MawNativeFramingCompileOnly {
$frameSource
$spanSource
$ownedSource
}
"@ -ReferencedAssemblies $references -CompilerOptions '/nowarn:1701'
function Compile-Check([string]$Name, [string]$RecordText, [string]$ReceiverText, [string]$FrameText = $frameSource) {
    $class = 'MawCellState_' + $Name
    $code = @"
using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using WorldGen = FrameSink_$class;
using Framing = FrameSink_$class;
internal static class FrameSink_$class {
    internal static readonly List<string> Calls = new();
    internal static void TileFrame(int x, int y, bool resetFrame) {
        if (!resetFrame) throw new InvalidOperationException("Owned tile lost original center reset policy");
        Calls.Add("tile:" + x + "," + y);
    }
    internal static void WallFrame(int x, int y, bool resetFrame) {
        if (!resetFrame) throw new InvalidOperationException("Owned wall lost original center reset policy");
        Calls.Add("wall:" + x + "," + y);
    }
    internal static void SquareTileFrame(int x, int y) => throw new InvalidOperationException("Broad tile framing wrapper invoked");
    internal static void SquareWallFrame(int x, int y) => throw new InvalidOperationException("Broad wall framing wrapper invoked");
}
public static class $class {
$RecordText
$requireSource
$FrameText
$spanSource
$ownedSource
$framingCheck
private static CellState SelectedForRestore(CellState before, CellState after) {
    int x = 0, y = 0;
    $guard
    return $ReceiverText;
}
$checks
}
"@
    Add-Type -TypeDefinition $code -ReferencedAssemblies $references -CompilerOptions '/nowarn:1701'
    return ($class -as [type]).GetMethod('Run').Invoke($null, @())
}
Write-Output ('PASS actual-source/native-values: ' + (Compile-Check 'Actual' $recordSource $receiver))
$directFrameCalls = @($apply.DescendantNodes() | Where-Object {
    $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -and
    $_.Expression.ToString() -in @('WorldGen.TileFrame', 'WorldGen.SquareTileFrame', 'WorldGen.SquareWallFrame')
})
if ($directFrameCalls.Count -ne 1 -or (Code $directFrameCalls[0]) -ne 'WorldGen.TileFrame(x,y,resetFrame:true)') {
    throw 'Fiber placement must use the native center-only framing entry point.'
}
Write-Output 'PASS actual framing source: exactly three owned centers dispatched to tile/wall entry points; fiber entry is center-only.'
$semantic = Member 'SameNonFrameState'
$expression = $semantic.ExpressionBody.Expression.ToString()
function Changed-Record([string]$Old, [string]$New) {
    if (!$recordSource.Contains($Old)) { throw "Control target missing: $Old" }
    return $recordSource.Replace($Old, $New)
}
$controls = @(
    @{ Name = 'strict-queue-flags'; Record = Changed-Record $expression "($expression) && Liquid.Equals(other.Liquid)"; Receiver = $receiver },
    @{ Name = 'reset-queue-flags'; Record = $recordSource; Receiver = 'before' },
    @{ Name = 'missing-liquid-amount'; Record = Changed-Record 'Liquid.Amount == other.Liquid.Amount' 'true'; Receiver = $receiver },
    @{ Name = 'missing-liquid-type'; Record = Changed-Record 'Liquid.LiquidType == other.Liquid.LiquidType' 'true'; Receiver = $receiver }
)
foreach ($control in $controls) {
    $rejected = $false
    try { $null = Compile-Check ($control.Name.Replace('-', '_')) $control.Record $control.Receiver }
    catch [System.Management.Automation.MethodInvocationException] {
        if ($_.Exception.GetBaseException() -isnot [InvalidOperationException]) { throw }
        $rejected = $true
        Write-Output "REJECTED control/$($control.Name): $($_.Exception.GetBaseException().Message)"
    }
    if (!$rejected) { throw "Negative control survived: $($control.Name)" }
}
foreach ($kind in @('Tile', 'Wall')) {
    $old = if ($kind -eq 'Tile') { 'WorldGen.TileFrame(x, span.Y, resetFrame: true);' } else { 'Framing.WallFrame(x, span.Y, resetFrame: true);' }
    $new = 'WorldGen.Square' + $kind + 'Frame(x, span.Y);'
    if (!$frameSource.Contains($old)) { throw "Framing control target missing: $kind" }
    $rejected = $false
    try { $null = Compile-Check ('broad_' + $kind) $recordSource $receiver $frameSource.Replace($old, $new) }
    catch [System.Management.Automation.MethodInvocationException] {
        if ($_.Exception.GetBaseException() -isnot [InvalidOperationException]) { throw }
        $rejected = $true
        Write-Output "REJECTED control/broad-$kind-wrapper: $($_.Exception.GetBaseException().Message)"
    }
    if (!$rejected) { throw "Broad framing control survived: $kind" }
}
if ((Get-FileHash -LiteralPath $sourcePath).Hash -ne $sourceHash) { throw 'World source changed during run; rerun.' }
Write-Output "PASS 6 negative controls; exact protection/impact assertions retained. Source SHA256: $sourceHash"
Write-Output "Native DLL SHA256: $nativeHash"
Write-Output 'Limits: extracted source/value operations only; no native queue execution, world mutation, framing, save/reload, game or install.'
