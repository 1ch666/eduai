param(
    [Parameter(Mandatory = $true)] [string] $Editor
)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$editorPath = (Resolve-Path -LiteralPath $Editor).Path
if ((Split-Path $editorPath -Leaf) -ne 'Unity.exe') {
    throw '請指定已安裝 Unity Editor 的 Unity.exe，不是 Unity Hub 或 unity CLI。'
}
$logDirectory = Join-Path $project 'Logs'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
function Invoke-CourtEditor([string] $Method, [string] $LogName) {
    $logPath = Join-Path $logDirectory $LogName
    $editorArguments = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $project + '"'),
        '-buildTarget', 'WebGL', '-executeMethod', $Method, '-logFile', ('"' + $logPath + '"'))
    $process = Start-Process -FilePath $editorPath -ArgumentList $editorArguments -WindowStyle Hidden -PassThru -Wait
    if ($process.ExitCode -ne 0) { throw "Unity 結束碼 $($process.ExitCode)。請查 $logPath" }
}
# 不覆寫已存在的場景；錯誤會立即停止，不發布半成品。
if (-not (Test-Path -LiteralPath (Join-Path $project 'Assets/Scenes/Courtroom.unity'))) {
    Invoke-CourtEditor 'EduAI.Court.Editor.CourtProjectBuilder.CreateScene' 'create-scene.log'
}
Invoke-CourtEditor 'EduAI.Court.Editor.CourtSmokeTests.Run' 'smoke-tests.log'
$smokeLog = Get-Content -LiteralPath (Join-Path $logDirectory 'smoke-tests.log') -Raw
if (-not $smokeLog.Contains('COURT_SMOKE_TESTS_PASSED')) { throw '未找到 Unity 流程測試通過標記。' }
Invoke-CourtEditor 'EduAI.Court.Editor.CourtProjectBuilder.BuildWebGL' 'webgl-build.log'
if (-not (Test-Path -LiteralPath (Join-Path $project 'Builds/WebGL/index.html'))) { throw '缺少 WebGL index.html。' }
Write-Output 'Unity 場景與流程測試、WebGL 建置完成。仍需以 HTTP 實測遊玩後才可發布。'
