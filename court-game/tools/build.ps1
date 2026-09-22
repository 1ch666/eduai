param(
    [Parameter(Mandatory = $true)] [string] $Editor,
    [string] $Python
)
$ErrorActionPreference = 'Stop'
$project = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (-not $Python) { $Python = Join-Path $project '../.court-tools/Scripts/python.exe' }
if (-not (Test-Path -LiteralPath $Python)) {
    throw '請依 tools/fonts/README.md 建立 fonttools 環境，或傳入 -Python 路徑。'
}
& $Python (Join-Path $PSScriptRoot 'subset-font.py') --check
if ($LASTEXITCODE -ne 0) { throw '字型檢查失敗，請重建子集後再建置。' }
$editorPath = (Resolve-Path -LiteralPath $Editor).Path
if ((Split-Path $editorPath -Leaf) -ne 'Unity.exe') {
    throw '請指定已安裝 Unity Editor 的 Unity.exe，不是 Unity Hub 或 unity CLI。'
}
$logDirectory = Join-Path $project 'Logs'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
function Invoke-CourtEditor([string] $Method, [string] $LogName) {
    $logPath = Join-Path $logDirectory $LogName
    $editorArguments = @('-batchmode', '-nographics', '-projectPath', ('"' + $project + '"'),
        '-buildTarget', 'WebGL', '-executeMethod', $Method, '-logFile', ('"' + $logPath + '"'))
    if ($Method -ne 'EduAI.Court.Editor.CourtPlayTests.Run') { $editorArguments += '-quit' }
    $process = Start-Process -FilePath $editorPath -ArgumentList $editorArguments -WindowStyle Hidden -PassThru
    # 只等待 Editor；Start-Process -Wait 在 Windows 也會等待持續運行的授權子程序。
    $process.WaitForExit()
    if ($process.ExitCode -ne 0) { throw "Unity 結束碼 $($process.ExitCode)。請查 $logPath" }
}
# 不覆寫已存在的場景；錯誤會立即停止，不發布半成品。
if (-not (Test-Path -LiteralPath (Join-Path $project 'Assets/Scenes/Courtroom.unity'))) {
    Invoke-CourtEditor 'EduAI.Court.Editor.CourtProjectBuilder.CreateScene' 'create-scene.log'
}
Invoke-CourtEditor 'EduAI.Court.Editor.CourtSmokeTests.Run' 'smoke-tests.log'
$smokeLog = Get-Content -LiteralPath (Join-Path $logDirectory 'smoke-tests.log') -Raw
if (-not $smokeLog.Contains('COURT_SMOKE_TESTS_PASSED')) { throw '未找到 Unity 流程測試通過標記。' }
Invoke-CourtEditor 'EduAI.Court.Editor.CourtPlayTests.Run' 'play-tests.log'
$playLog = Get-Content -LiteralPath (Join-Path $logDirectory 'play-tests.log') -Raw
if (-not $playLog.Contains('COURT_PLAY_TESTS_PASSED')) { throw '未找到 Unity Play 測試通過標記。' }
Invoke-CourtEditor 'EduAI.Court.Editor.CourtProjectBuilder.BuildWebGL' 'webgl-build.log'
if (-not (Test-Path -LiteralPath (Join-Path $project 'Builds/WebGL/index.html'))) { throw '缺少 WebGL index.html。' }
Write-Output 'Unity 場景與流程測試、WebGL 建置完成。仍需以 HTTP 實測遊玩後才可發布。'
