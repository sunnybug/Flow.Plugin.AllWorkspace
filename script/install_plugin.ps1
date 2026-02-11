# 功能说明：编译并安装插件到 Flow Launcher（从 src 构建，输出到 .temp）
# 编码：UTF-8 BOM，行尾：CRLF

param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Src = Join-Path $Root "src"
$PluginJsonPath = Join-Path $Src "plugin.json"
$OutputPath = Join-Path $Root ".temp\bin\$Configuration"

$PluginConfig = Get-Content $PluginJsonPath -Raw | ConvertFrom-Json
$PluginID = $PluginConfig.ID
$PluginName = $PluginConfig.Name
$PluginVersion = $PluginConfig.Version

$FlowLauncherPluginsPath = Join-Path $env:APPDATA "FlowLauncher\Plugins"
$PluginFolderName = "$PluginName-$PluginVersion"
$TargetPath = Join-Path $FlowLauncherPluginsPath $PluginFolderName

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Flow Launcher 插件编译安装脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "插件: $PluginName $PluginVersion | 配置: $Configuration" -ForegroundColor White
Write-Host ""

& (Join-Path $Root "script\build.ps1") $(if ($Configuration -eq "Release") { "-Release" })
if ($LASTEXITCODE -ne 0) { exit 1 }

if (-not (Test-Path $OutputPath)) {
    Write-Host "错误: 输出目录不存在: $OutputPath" -ForegroundColor Red
    exit 1
}

# 关闭 Flow Launcher
$FlowLauncherProcesses = Get-Process -Name "Flow.Launcher" -ErrorAction SilentlyContinue
if ($FlowLauncherProcesses) {
    foreach ($Process in $FlowLauncherProcesses) {
        try {
            $Process.CloseMainWindow() | Out-Null
            Start-Sleep -Milliseconds 500
            if (-not $Process.HasExited) { $Process.Kill() }
            $Process.WaitForExit(5000)
        } catch { Write-Host "终止进程失败: $_" -ForegroundColor Red }
    }
}
Start-Sleep -Seconds 3

# 清理旧插件目录
$OldPluginPaths = @(
    (Join-Path $FlowLauncherPluginsPath $PluginID),
    (Join-Path $FlowLauncherPluginsPath "AllWorkspace"),
    (Join-Path $FlowLauncherPluginsPath "VS Code Workspaces-$PluginVersion")
)
foreach ($OldPath in $OldPluginPaths) {
    if (Test-Path $OldPath) { Remove-Item -Path $OldPath -Recurse -Force }
}

if (Test-Path $TargetPath) {
    Get-ChildItem -Path $TargetPath -File | Remove-Item -Force
    Get-ChildItem -Path $TargetPath -Directory | Remove-Item -Recurse -Force
} else {
    New-Item -ItemType Directory -Path $TargetPath -Force | Out-Null
}

Get-ChildItem -Path $OutputPath | ForEach-Object {
    Copy-Item -Path $_.FullName -Destination $TargetPath -Recurse -Force
}
Write-Host "已安装到: $TargetPath" -ForegroundColor Green

$FlowLauncherExe = Join-Path $env:LOCALAPPDATA "FlowLauncher\Flow.Launcher.exe"
if (Test-Path $FlowLauncherExe) { Start-Process -FilePath $FlowLauncherExe }
Write-Host "========================================" -ForegroundColor Cyan
