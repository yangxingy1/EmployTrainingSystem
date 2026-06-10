$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$ServiceDir = Join-Path $Root "gesture-service"
$CondaEnv = if ($env:HUIDONGSHOU_CONDA_ENV) {
    $env:HUIDONGSHOU_CONDA_ENV
} else {
    "huidongshou"
}

if (-not (Get-Command conda -ErrorAction SilentlyContinue)) {
    throw "conda was not found. Open an Anaconda Prompt or add Conda to PATH."
}

Set-Location $ServiceDir
& conda run -n $CondaEnv python main.py --mode replay --replay samples\replay_sample.jsonl
