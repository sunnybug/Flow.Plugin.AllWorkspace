# 功能说明：1) 编译 build.ps1 默认 Debug、传参 -Release 为 Release 2) 强杀目标程序 3) 清除运行日志 4) 插件则安装到目标程序 5) 启动目标程序
# 编码：UTF-8 BOM，行尾：CRLF

param(
    [switch]$Release
)

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot
$Run = Join-Path $Root ".run"
if (-not (Test-Path $Run)) { New-Item -ItemType Directory -Path $Run -Force | Out-Null }

$Config = if ($Release) { "Release" } else { "Debug" }

# 1. 编译
Write-Host "[1/5] 编译 ($Config)..." -ForegroundColor Yellow
& (Join-Path $Root "script\build.ps1") $(if ($Release) { "-Release" })
if ($LASTEXITCODE -ne 0) {
    Write-Host "编译失败" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 目标程序：Flow Launcher
$TargetProcessName = "Flow.Launcher"
$TargetExe = Join-Path $env:LOCALAPPDATA "FlowLauncher\Flow.Launcher.exe"
$FlowLauncherLogsPath = Join-Path $env:APPDATA "FlowLauncher\Logs"
$FlowLauncherPluginsPath = Join-Path $env:APPDATA "FlowLauncher\Plugins"

# 2. 强杀目标程序
Write-Host "[2/5] 强杀目标程序 ($TargetProcessName)..." -ForegroundColor Yellow
$procs = Get-Process -Name $TargetProcessName -ErrorAction SilentlyContinue
if ($procs) {
    foreach ($p in $procs) {
        try {
            $p.Kill()
            $p.WaitForExit(5000)
        } catch { Write-Host "  终止 PID $($p.Id) 失败: $_" -ForegroundColor Red }
    }
    Start-Sleep -Seconds 2
    Write-Host "已强杀 $($procs.Count) 个进程" -ForegroundColor Green
} else {
    Write-Host "未运行" -ForegroundColor Gray
}
Write-Host ""

# 3. 清除运行日志
Write-Host "[3/5] 清除运行日志..." -ForegroundColor Yellow
if (Test-Path $FlowLauncherLogsPath) {
    $logFiles = Get-ChildItem -Path $FlowLauncherLogsPath -File -Recurse -Include "*.txt","*.log"
    $n = 0
    foreach ($f in $logFiles) {
        try { Remove-Item $f.FullName -Force -ErrorAction Stop; $n++ } catch { }
    }
    Write-Host "已删除 $n 个日志文件" -ForegroundColor Green
} else {
    Write-Host "日志目录不存在" -ForegroundColor Gray
}
Write-Host ""

# 4. 插件则安装到目标程序
Write-Host "[4/5] 安装插件到目标程序..." -ForegroundColor Yellow
$PluginJsonPath = Join-Path $Root "src\plugin.json"
$OutputPath = Join-Path $Root ".temp\bin\$Config"
if (-not (Test-Path $OutputPath)) {
    Write-Host "错误: 输出目录不存在 $OutputPath" -ForegroundColor Red
    exit 1
}
$plugin = Get-Content $PluginJsonPath -Raw | ConvertFrom-Json
$pluginDir = Join-Path $FlowLauncherPluginsPath "$($plugin.Name)-$($plugin.Version)"
if (Test-Path $pluginDir) {
    Get-ChildItem -Path $pluginDir -File | Remove-Item -Force
    Get-ChildItem -Path $pluginDir -Directory | Remove-Item -Recurse -Force
}
New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null
Get-ChildItem -Path $OutputPath | ForEach-Object { Copy-Item -Path $_.FullName -Destination $pluginDir -Recurse -Force }
Write-Host "已安装到: $pluginDir" -ForegroundColor Green
Write-Host ""

# 5. 启动目标程序
Write-Host "[5/5] 启动目标程序..." -ForegroundColor Yellow
if (Test-Path $TargetExe) {
    Start-Process -FilePath $TargetExe
    Write-Host "已启动 $TargetProcessName" -ForegroundColor Green
} else {
    Write-Host "未找到: $TargetExe" -ForegroundColor Red
}
Write-Host ""
Write-Host "完成. 工作目录: $Run" -ForegroundColor Cyan
