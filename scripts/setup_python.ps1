$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$ServiceDir = Join-Path $Root "gesture-service"
$RootDrive = (Get-Item -LiteralPath $Root).PSDrive.Root
$VenvDir = if ($env:HUIDONGSHOU_VENV) {
    $env:HUIDONGSHOU_VENV
} else {
    Join-Path $RootDrive "HuiDongShouPython\.venv"
}

Set-Location $ServiceDir

if (-not (Test-Path $VenvDir)) {
    New-Item -ItemType Directory -Path (Split-Path -Parent $VenvDir) -Force | Out-Null
    python -m venv $VenvDir
}

& (Join-Path $VenvDir "Scripts\python.exe") -m pip install --upgrade pip
& (Join-Path $VenvDir "Scripts\python.exe") -m pip install -r requirements.txt

Write-Host "Python environment is ready:" $VenvDir
