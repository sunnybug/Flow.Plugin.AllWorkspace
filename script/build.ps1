# 功能说明：构建项目，默认 Debug，传参 --release 时构建 Release
# 编码：UTF-8 BOM，行尾：CRLF

param(
    [switch]$Release
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Src = Join-Path $Root "src"
$Config = if ($Release) { "Release" } else { "Debug" }

Set-Location $Src
dotnet restore
if ($LASTEXITCODE -ne 0) { exit 1 }
dotnet build -c $Config --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }
Write-Host "构建完成: $Config" -ForegroundColor Green
