[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$source=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot '../Content/Diagnostics/MawShallowQaScope.cs')
$test=@'
namespace apogean.Content.Diagnostics {
    public static class ShallowScopeChecks {
        public static int Run() {
            int checks=0;
            void Check(bool b,string why) { if(!b)throw new System.Exception("SHALLOW_SCOPE: "+why); checks++; }
            foreach(string world in new[]{MawShallowQaScope.World,"aga","Apogee Native Visual V2",null})
            foreach(string player in new[]{"gg",MawShallowQaScope.Plain,"Maw QA Plain2",null})
            foreach(bool single in new[]{true,false}) foreach(bool menu in new[]{true,false})
                Check(MawShallowQaScope.Context(world,player,single,menu)==(world==MawShallowQaScope.World && (player=="gg"||player==MawShallowQaScope.Plain)&&single&&!menu),"world/player/mode guard");
            foreach(string request in new[]{"maw-shallow-build","maw-shallow-unknown","vegetation-view-build","qa-perf-start","kessler-campus",null})
                Check(!MawShallowQaScope.Request(MawShallowQaScope.Plain,request),"plain cannot mutate other scenes");
            foreach(string request in new[]{"qa-save-and-quit","maw-shallow-pristine","maw-shallow-motion-entry","maw-shallow-motion-connector-out","maw-shallow-light-awake","maw-shallow-light-dormant","maw-shallow-light-awake-bright","maw-shallow-light-dormant-bright","maw-shallow-release"}) {
                Check(MawShallowQaScope.Request(MawShallowQaScope.Plain,request),"plain allowed request");
                Check(!MawShallowQaScope.Request("ordinary",request),"unknown player denied");
            }
            foreach(bool context in new[]{true,false}) foreach(bool held in new[]{true,false}) foreach(bool visiting in new[]{true,false})
            for(int x=9;x<=14;x++) for(int y=19;y<=24;y++)
            {
                bool expected=context&&held&&visiting&&x>=10&&x<=13&&y>=20&&y<=23;
                bool applies=MawShallowQaScope.PreviewApplies(context,held,visiting,x,y,10,20,4,4);
                Check(applies==expected,"exclusive preview bounds");
                foreach(bool bright in new[]{true,false})
                    Check(MawShallowQaScope.LightScale(applies,bright)==(expected&&bright?1.6f:1f),"bounded light scale");
            }
            Check(!MawShallowQaScope.PreviewApplies(true,true,true,10,20,10,20,0,4),"empty preview");
            return checks;
        }
    }
}
'@
Add-Type -TypeDefinition ($source+"`n"+$test)
$count=[apogean.Content.Diagnostics.ShallowScopeChecks]::Run()
Write-Output "PASS $count pure QA context/request/preview boundary checks. Native behavior is separate."
