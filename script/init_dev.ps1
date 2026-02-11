# 功能说明：初始化开发环境（还原依赖、确保 .run 目录等）
# 编码：UTF-8 BOM，行尾：CRLF

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Src = Join-Path $Root "src"
$Run = Join-Path $Root ".run"
$RunLog = Join-Path $Run "log"
$RunConfig = Join-Path $Run "config"

foreach ($d in $Run, $RunLog, $RunConfig) {
    if (-not (Test-Path $d)) {
        New-Item -ItemType Directory -Path $d -Force | Out-Null
        Write-Host "创建目录: $d" -ForegroundColor Gray
    }
}

Set-Location $Src
dotnet restore
if ($LASTEXITCODE -ne 0) { exit 1 }
Write-Host "开发环境初始化完成" -ForegroundColor Green
