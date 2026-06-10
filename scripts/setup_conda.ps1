$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $PSScriptRoot
$ServiceDir = Join-Path $Root "gesture-service"
$CondaEnv = if ($env:HUIDONGSHOU_CONDA_ENV) {
    $env:HUIDONGSHOU_CONDA_ENV
} else {
    "huidongshou"
}
$PythonVersion = if ($env:HUIDONGSHOU_PYTHON_VERSION) {
    $env:HUIDONGSHOU_PYTHON_VERSION
} else {
    "3.11"
}

if (-not (Get-Command conda -ErrorAction SilentlyContinue)) {
    throw "conda was not found. Open an Anaconda Prompt or add Conda to PATH."
}

$envList = (& conda env list --json | ConvertFrom-Json).envs
$exists = $CondaEnv -eq "base"
foreach ($envPath in $envList) {
    if ((Split-Path -Leaf $envPath) -eq $CondaEnv) {
        $exists = $true
        break
    }
}

if (-not $exists) {
    Write-Host "Creating Conda environment '$CondaEnv' with Python $PythonVersion..."
    & conda create -y -n $CondaEnv python=$PythonVersion pip
}

Set-Location $ServiceDir
& conda run -n $CondaEnv python -m pip install --upgrade pip
& conda run -n $CondaEnv python -m pip install -r requirements.txt
& conda run -n $CondaEnv python -c "import sys; print('Python:', sys.executable)"

Write-Host "Conda environment is ready:" $CondaEnv
