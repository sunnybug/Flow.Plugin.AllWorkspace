# 功能说明：发布 Release 到 .dist 目录（带版本号）
# 编码：UTF-8 BOM，行尾：CRLF

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Src = Join-Path $Root "src"
$Dist = Join-Path $Root ".dist"
$TempBin = Join-Path $Root ".temp\bin\Release"

$PluginJson = Join-Path $Src "plugin.json"
$Version = (Get-Content $PluginJson -Raw | ConvertFrom-Json).Version
$OutName = "Flow.Launcher.Plugin.AllWorkspace-$Version"
$OutDir = Join-Path $Dist $OutName

& (Join-Path $Root "script\build.ps1") -Release
if ($LASTEXITCODE -ne 0) { exit 1 }

if (-not (Test-Path $TempBin)) {
    Write-Host "错误: 未找到输出目录 $TempBin" -ForegroundColor Red
    exit 1
}

if (Test-Path $OutDir) { Remove-Item $OutDir -Recurse -Force }
New-Item -ItemType Directory -Path $OutDir -Force | Out-Null
Copy-Item -Path (Join-Path $TempBin "*") -Destination $OutDir -Recurse -Force
Write-Host "已发布到: $OutDir" -ForegroundColor Green
