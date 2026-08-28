# Builds QuietAdmin and stages the DLL in Release\ ready to attach to a GitHub release.
#
#   powershell -ExecutionPolicy Bypass -File .\release.ps1
#
# Does not tag, commit or upload - it only produces the artifact and prints the steps.

$ErrorActionPreference = 'Stop'

$ReleaseDir = Join-Path $PSScriptRoot 'Release'
$Dll        = Join-Path $PSScriptRoot 'QuietAdmin.dll'

# -NoDeploy so a running game holding the installed copy cannot block a release build.
& (Join-Path $PSScriptRoot 'build.ps1') -NoDeploy

if (-not (Test-Path $Dll)) { throw "Build produced no QuietAdmin.dll" }

if (Test-Path $ReleaseDir) { Remove-Item $ReleaseDir -Recurse -Force }
New-Item -ItemType Directory -Path $ReleaseDir | Out-Null
Copy-Item $Dll $ReleaseDir -Force

# Read the version back off the built DLL rather than tracking it in a second place.
$version = (Get-Item $Dll).VersionInfo.FileVersion
if ([string]::IsNullOrWhiteSpace($version)) { $version = 'unknown' }

Write-Host "`nStaged QuietAdmin.dll v$version in $ReleaseDir" -ForegroundColor Green

Write-Host @'

To publish:

  1. Commit and push.
  2. On GitHub, Releases -> Draft a new release.
  3. Tag it v1.0.0, matching AssemblyVersion in QuietAdmin.cs.
  4. Attach Release\QuietAdmin.dll.
  5. Publish. The README badges and download link point at /releases/latest and update themselves.
'@
