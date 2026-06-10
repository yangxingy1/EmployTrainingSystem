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
& conda run -n $CondaEnv python -c @"
import cv2

backends = [
    ("dshow", cv2.CAP_DSHOW),
    ("msmf", cv2.CAP_MSMF),
    ("auto", 0),
]

for index in range(5):
    for name, backend in backends:
        cap = cv2.VideoCapture(index, backend) if backend else cv2.VideoCapture(index)
        ok = cap.isOpened()
        ret, frame = cap.read() if ok else (False, None)
        mean = float(frame.mean()) if ret and frame is not None else -1.0
        width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH)) if ok else 0
        height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT)) if ok else 0
        cap.release()
        print(f"camera={index} backend={name:<5} opened={ok:<5} frame={ret:<5} size={width}x{height} brightness={mean:.1f}")
"@
