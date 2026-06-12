$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$ServiceDir = Join-Path $Root "gesture-service"
$RootDrive = (Get-Item -LiteralPath $Root).PSDrive.Root
$VenvDir = if ($env:HUIDONGSHOU_VENV) {
    $env:HUIDONGSHOU_VENV
} else {
    Join-Path $RootDrive "HuiDongShouPython\.venv"
}
$Python = Join-Path $VenvDir "Scripts\python.exe"

if (-not (Test-Path $Python)) {
    Write-Host "Virtual environment not found. Running setup first..."
    & (Join-Path $PSScriptRoot "setup_python.ps1")
}

Set-Location $ServiceDir
& $Python -m src.main --mode replay --replay samples\replay_sample.jsonl
