# Read the declared parameter contract, not comments or executable script output.
function Assert-LiteralValidateSet {
 param([Parameter(Mandatory)][string]$Source,[Parameter(Mandatory)][string]$ParameterName,
       [Parameter(Mandatory)][string[]]$Required)
 $parseTokens=$null;$parseErrors=$null
 $ast=[Management.Automation.Language.Parser]::ParseInput($Source,[ref]$parseTokens,[ref]$parseErrors)
 if($parseErrors.Count){throw 'VALIDATE_SET_CONTRACT: script does not parse'}
 $parameters=@($ast.ParamBlock.Parameters | Where-Object {$_.Name.VariablePath.UserPath -ieq $ParameterName})
 if($parameters.Count -ne 1){throw 'VALIDATE_SET_CONTRACT: expected one top-level parameter'}
 $attributes=@($parameters[0].Attributes | Where-Object {
  $_ -is [Management.Automation.Language.AttributeAst] -and
  $_.TypeName.FullName -in @('ValidateSet','ValidateSetAttribute','System.Management.Automation.ValidateSetAttribute')
 })
 if($attributes.Count -ne 1){throw 'VALIDATE_SET_CONTRACT: expected one literal ValidateSet'}
 if($attributes[0].NamedArguments.Count){throw 'VALIDATE_SET_CONTRACT: nondefault validation options require an explicit contract'}
 $seen=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
 foreach($argument in $attributes[0].PositionalArguments){
  if($argument -isnot [Management.Automation.Language.StringConstantExpressionAst] -or
     [string]::IsNullOrWhiteSpace($argument.Value) -or -not $seen.Add($argument.Value)){
   throw 'VALIDATE_SET_CONTRACT: nonliteral, empty or duplicate value'
  }
 }
 foreach($value in $Required){if(-not $seen.Contains($value)){throw "VALIDATE_SET_CONTRACT: missing $value"}}
 if($seen.Count -eq 0){throw 'VALIDATE_SET_CONTRACT: no accepted values'}
}
