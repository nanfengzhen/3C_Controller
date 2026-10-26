# 在用户有项目写权限的 PowerShell 中运行。本脚本尚未执行。
# 只复制指定新增文件；任何同名目标存在即整体停止，不覆写任何文件。
$ErrorActionPreference = 'Stop'
$ikTarget = 'D:\unity\Event_Stream\Assets\Scripts\TestIK\Plan'
$ikNames = @('PlanASphereFootIK.cs', 'UpwardSphereClearance.cs', 'PlanBToeFootIK.cs', 'README_Plans.md')
if (-not (Test-Path -LiteralPath 'D:\unity\Event_Stream\Assets\Scripts\TestIK\PredictedFootPath.cs')) {
    throw '缺少现有 PredictedFootPath.cs；请确认目标项目。'
}
foreach ($ikName in $ikNames) {
    if (-not (Test-Path -LiteralPath (Join-Path $PSScriptRoot $ikName))) { throw "缺少源文件：$ikName" }
    if (Test-Path -LiteralPath (Join-Path $ikTarget $ikName)) { throw "停止：目标同名文件已存在，不覆盖：$ikName" }
}
New-Item -ItemType Directory -Force -Path $ikTarget | Out-Null
foreach ($ikName in $ikNames) {
    # File.Copy 的 overwrite=false 同时防止检查后的同名竞争。
    [System.IO.File]::Copy((Join-Path $PSScriptRoot $ikName), (Join-Path $ikTarget $ikName), $false)
    Write-Output "已新增：$ikName"
}
Write-Output '复制完成。请回到 Unity 等待导入编译，再按 README 配置一个方案。'
