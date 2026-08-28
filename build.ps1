# Builds QuietAdmin.dll and drops it in the game's Mods folder.
#
#   powershell -ExecutionPolicy Bypass -File .\build.ps1
#
# Drives csc directly rather than 'dotnet build' so no net35 targeting pack is needed - the references below
# are the game's own assemblies, which is what the mod runs against anyway.

param(
    # Skip copying into the game's Mods folder. release.ps1 uses this so a running game cannot block a build.
    [switch]$NoDeploy
)

$ErrorActionPreference = 'Stop'

$GameDir    = 'C:\Program Files (x86)\Steam\steamapps\common\Holdfast Nations At War'
$ManagedDir = Join-Path $GameDir 'Holdfast NaW_Data\Managed'
$MelonDir   = Join-Path $GameDir 'MelonLoader\net35'
$ModsDir    = Join-Path $GameDir 'Mods'
$OutFile    = Join-Path $PSScriptRoot 'QuietAdmin.dll'

foreach ($d in @($ManagedDir, $MelonDir, $ModsDir)) {
    if (-not (Test-Path $d)) { throw "Missing directory: $d" }
}

$csc = Get-ChildItem -Path 'C:\Program Files\dotnet\sdk' -Recurse -Filter 'csc.dll' -ErrorAction SilentlyContinue |
       Sort-Object FullName -Descending | Select-Object -First 1
if ($null -eq $csc) { throw 'Could not find the Roslyn compiler under the dotnet SDK.' }

$refs = @(
    (Join-Path $ManagedDir 'mscorlib.dll'),
    (Join-Path $ManagedDir 'System.dll'),
    (Join-Path $ManagedDir 'System.Core.dll'),
    (Join-Path $ManagedDir 'netstandard.dll'),
    (Join-Path $ManagedDir 'UnityEngine.dll'),
    (Join-Path $ManagedDir 'UnityEngine.CoreModule.dll'),
    (Join-Path $ManagedDir 'Assembly-CSharp.dll'),
    (Join-Path $MelonDir   'MelonLoader.dll'),
    (Join-Path $MelonDir   '0Harmony.dll')
)

$cscArgs = @('-nologo', '-noconfig', '-nostdlib+', '-target:library', "-out:$OutFile")
foreach ($r in $refs) {
    if (-not (Test-Path $r)) { throw "Missing reference assembly: $r" }
    $cscArgs += "-r:$r"
}

$sources = Get-ChildItem -Path $PSScriptRoot -Filter '*.cs' -File
if ($sources.Count -eq 0) { throw "No .cs files found in $PSScriptRoot" }
foreach ($s in $sources) { $cscArgs += $s.FullName }

Write-Host "Compiling $($sources.Count) file(s) -> $OutFile"
& dotnet exec $csc.FullName @cscArgs
if ($LASTEXITCODE -ne 0) { throw "Compile failed (exit $LASTEXITCODE)" }

if ($NoDeploy) {
    Write-Host "Built $OutFile (not deployed)."
    return
}

try {
    Copy-Item $OutFile (Join-Path $ModsDir 'QuietAdmin.dll') -Force
    Write-Host "Copied to $ModsDir. Restart the game to load it."
}
catch [System.IO.IOException] {
    throw "Built fine, but could not copy into $ModsDir - the game is probably running and has the DLL loaded. Close Holdfast and run this again."
}
