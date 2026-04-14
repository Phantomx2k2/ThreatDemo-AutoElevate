# build.ps1 - compiles both apps with csc.exe (.NET Framework 4.x, no SDK needed)
$ErrorActionPreference = 'Stop'

$csc = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$src = $PSScriptRoot
$bin = Join-Path $src 'bin'

if (-not (Test-Path $bin)) { New-Item -ItemType Directory -Path $bin | Out-Null }

function Compile($name) {
    $srcFile = Join-Path $src "$name.cs"
    $outFile = Join-Path $bin "$name.exe"
    Write-Host "  Compiling $name..." -ForegroundColor Cyan
    & $csc /nologo /target:winexe /out:$outFile /r:System.Windows.Forms.dll /r:System.Drawing.dll $srcFile
    if ($LASTEXITCODE -ne 0) { throw "Compile failed: $name" }
    Write-Host "  -> bin\$name.exe" -ForegroundColor Green
}

Compile 'YourRMM'
Compile 'ThreatDemo'
Write-Host 'Done.' -ForegroundColor Yellow
