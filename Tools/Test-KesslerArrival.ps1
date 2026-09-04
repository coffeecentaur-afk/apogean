Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$failures = [System.Collections.Generic.List[string]]::new()

function Require-Text([string]$relativePath, [string]$pattern, [string]$message) {
	$path = Join-Path $projectRoot $relativePath
	if (-not (Test-Path -LiteralPath $path)) {
		$failures.Add("Missing $relativePath")
		return
	}
	if ((Get-Content -Raw -LiteralPath $path) -notmatch $pattern) {
		$failures.Add($message)
	}
}

Require-Text 'Content/Factions/KesslerArrivalState.cs' 'ImpactSignaled[\s\S]*AwaitingDawn[\s\S]*AssessmentActive' 'Arrival state order is incomplete.'
Require-Text 'Content/Factions/FactionProgression.cs' 'kesslerArrivalStage' 'Arrival stage is not persisted.'
Require-Text 'Content/Factions/FactionProgression.cs' 'kesslerArrivalSawNight' 'Night observation is not persisted.'
Require-Text 'Content/Factions/FactionProgression.cs' 'NetSend[\s\S]*kesslerArrival\.Stage[\s\S]*kesslerArrival\.SawNight' 'Arrival state is not networked.'
Require-Text 'Content/Factions/FactionProgression.cs' 'RegisterInvasionKill[\s\S]*Main\.netMode == NetmodeID\.MultiplayerClient' 'Clients can still mutate the shared invasion quota.'
Require-Text 'Content/Factions/FactionProgression.cs' 'SetContactable[\s\S]*GetRelation\(faction\) != FactionRelation\.Hostile[\s\S]*AwardKesslerCompletionScrip' 'Completion rewards are not protected against duplicate transitions.'
Require-Text 'Content/Invasions/InvasionSpawnRates.cs' 'ZoneOverworldHeight' 'Assessment spawn ownership is not surface-bounded.'
Require-Text 'Content/NPCs/Kessler/KesslerSurveyDrone.cs' 'NPC\.damage = 0' 'Survey Drone regained contact damage.'
Require-Text 'Content/NPCs/Kessler/KesslerReclaimer.cs' 'NPC\.damage = 0' 'Reclaimer regained contact damage.'
Require-Text 'Content/NPCs/Kessler/KesslerQuartermaster.cs' 'SupportsVanillaChat => true' 'Quartermaster no longer supports the stable vanilla shop path.'
Require-Text 'Content/Structures/CompoundGen.cs' 'TryGetPublicPost' 'Quartermaster is not anchored to the saved Campus.'
Require-Text 'Content/Structures/CompoundGen.cs' 'NetSend\(BinaryWriter writer\)[\s\S]*compoundBounds[\s\S]*doorBounds' 'Campus bounds and door state are not sent with world data.'
Require-Text 'Content/Structures/CompoundGen.cs' 'NetReceive\(BinaryReader reader\)[\s\S]*compoundBounds\.Clear\(\)[\s\S]*doorBounds\.Clear\(\)' 'Joining clients do not rebuild Campus location state.'
Require-Text 'Content/Structures/CompoundGen.cs' 'SetCompoundSealed[\s\S]*Main\.netMode == NetmodeID\.MultiplayerClient' 'Clients can still mutate progression-controlled compound tiles.'
Require-Text 'Content/NPCs/Kessler/KesslerSurveyDrone.cs' 'IsKesslerAssessmentActive\)[\s\S]*Main\.netMode != NetmodeID\.MultiplayerClient[\s\S]*NPC\.active = false' 'Survey Drone can deactivate itself on a client.'
Require-Text 'Content/NPCs/Kessler/KesslerReclaimer.cs' 'IsKesslerAssessmentActive\)[\s\S]*Main\.netMode != NetmodeID\.MultiplayerClient[\s\S]*NPC\.active = false' 'Reclaimer can deactivate itself on a client.'
Require-Text 'Content/NPCs/Kessler/KesslerQuartermaster.cs' 'AI\(\)[\s\S]*Main\.netMode == NetmodeID\.MultiplayerClient[\s\S]*NPC\.netUpdate = true' 'Quartermaster anchoring is not server-authoritative.'

if ($failures.Count -gt 0) {
	Write-Host "KESSLER ARRIVAL CONTRACT: FAIL ($($failures.Count) problems)" -ForegroundColor Red
	$failures | ForEach-Object { Write-Host " - $_" }
	exit 1
}

Write-Host 'KESSLER ARRIVAL CONTRACT: PASS' -ForegroundColor Green
