#requires -Version 7.2
# Execute the actual extracted production parser. No substitute lifecycle model.
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -TypeDefinition 'internal sealed class MawNameRoslynBootstrap {}'
$path = Join-Path $PSScriptRoot '../Common/WorldGeneration/MawSeedWorld.cs'
$before = (Get-FileHash -LiteralPath $path).Hash
$tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText((Get-Content -LiteralPath $path -Raw)).GetRoot()
$members = @($tree.DescendantNodes() | Where-Object {
    ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax] -and $_.Declaration.Variables[0].Identifier.ValueText -in @('WorldPrefix','WorldV2Prefix','WorldV3Prefix')) -or
    ($_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax] -and $_.Identifier.ValueText -eq 'TryNameVersion')
})
if ($members.Count -ne 4) { throw 'Expected exactly three prefixes and the actual parser.' }
$body = ($members | ForEach-Object ToString) -join "`n"
Add-Type -TypeDefinition ("using System; using System.Linq; using System.Globalization; public static class MawNamesNativeSource {" + $body + '
public static string Check(string name) { bool valid=TryNameVersion(name,out int seed,out int version); return valid?$"{version}:{seed}":"no"; }
}')
$cases = @(
    @('Apogean Maw Seed QA 202','1:202'), @('Apogean Maw Seed V2 QA 202','2:202'),
    @('Apogean Maw Seed QA 0','1:0'), @('Apogean Maw Seed V2 QA 2147483647','2:2147483647'),
    @('Apogean Maw Seed QA 00042','1:42'), @('aga','no'), @('Apogee Native Visual V3','no'),
    @('Apogean Maw Seed QA ','no'), @('Apogean Maw Seed V2 QA ','no'), @('Apogean Maw Seed V3 QA 202','3:202'),
    @('Apogean Maw Seed V4 QA 202','no'), @('Apogean Maw Seed V3 QA 2147483648','no'), @('Apogean Maw Seed V3 QA ','no'),
    @('Apogean Maw Seed V2 QA -1','no'), @('Apogean Maw Seed QA +1','no'), @('apogean Maw Seed QA 202','no'),
    @('Apogean Maw Seed QA 202 backup','no'), @('Apogean Maw Seed V2 QA 2147483648','no'),
    @('Apogean Maw Seed QA １２','no'), @("Apogean Maw Seed QA 202`n",'no'), @($null,'no')
)
foreach ($case in $cases) {
    $actual = [MawNamesNativeSource]::Check($case[0])
    if ($actual -cne $case[1]) { throw "Name contract failed: '$($case[0])': $actual" }
}
if ((Get-FileHash -LiteralPath $path).Hash -ne $before) { throw 'Parser changed during checks; rerun.' }
Write-Output "PASS $($cases.Count) actual parser cases: v1/v2/v3 separation, overflow, malformed and regular-world rejection. No generation performed."
