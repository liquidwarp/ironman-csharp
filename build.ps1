#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds the Ironman mod and produces Client/release/acidphantasm-ironman.zip

.DESCRIPTION
    Builds the server project first, then the client. The client's PostBuild
    target packages the server output into the release ZIP, so the order matters
    and the server must be built in Release: the packaging glob points at
    Server/bin/Release/ regardless of how the client itself is configured.

    Nothing is written to the SPT install. TarkovDir is still required, because
    the client compiles against the game's assemblies (BepInEx, SPT plugins,
    Assembly-CSharp).

.PARAMETER TarkovDir
    Root of the SPT install used to resolve client references.

.EXAMPLE
    ./build.ps1

.EXAMPLE
    ./build.ps1 -TarkovDir 'D:\SPT 4.1 BE'
#>
[CmdletBinding()]
param(
    [string]$TarkovDir = 'E:\Games\SPT 4.1'
)

$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot

try {
    # MSBuild needs a trailing separator: the csproj concatenates this with
    # subpaths. Forward slashes avoid the trailing-backslash escaping problem
    # in -p: arguments.
    $tarkov = $TarkovDir.Replace('\', '/').TrimEnd('/') + '/'

    $required = @(
        'BepInEx/core/BepInEx.dll'
        'BepInEx/plugins/spt/spt-reflection.dll'
        'EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll'
    )
    foreach ($rel in $required) {
        if (-not (Test-Path (Join-Path $tarkov $rel))) {
            throw "Not an SPT install (missing $rel): $TarkovDir`nPass a different one with -TarkovDir '<path>'."
        }
    }

    Write-Host "==> Server (Release)" -ForegroundColor Cyan
    dotnet build Server/acidphantasm-ironman-server.csproj -c Release --nologo
    if ($LASTEXITCODE -ne 0) { throw "Server build failed ($LASTEXITCODE)." }

    Write-Host "`n==> Client (Release)" -ForegroundColor Cyan
    dotnet build Client/acidphantasm-ironman-client.csproj -c Release --nologo `
        -p:TarkovDir="$tarkov" `
        -p:DeployToTarkov=false
    if ($LASTEXITCODE -ne 0) { throw "Client build failed ($LASTEXITCODE)." }

    $zip = Join-Path $PSScriptRoot 'Client/release/acidphantasm-ironman.zip'
    if (-not (Test-Path $zip)) { throw "Build reported success but no archive at $zip." }

    $item = Get-Item $zip
    Write-Host "`n==> Archive" -ForegroundColor Green
    Write-Host ("    {0}" -f $item.FullName)
    Write-Host ("    {0:N0} bytes" -f $item.Length)
}
finally {
    Pop-Location
}
