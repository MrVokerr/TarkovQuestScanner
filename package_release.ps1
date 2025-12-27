$ErrorActionPreference = "Stop"

$sourceDir = "bin\x64\Release"
$destDir = "ReleaseBuild"

Write-Host "Cleaning release folder..."
if (Test-Path $destDir) {
    Remove-Item -Recurse -Force $destDir
}
New-Item -ItemType Directory -Force -Path $destDir | Out-Null

Write-Host "Copying executable..."
Copy-Item "$sourceDir\TarkovQuestScanner.exe" -Destination $destDir
if (Test-Path "$sourceDir\TarkovQuestScanner.exe.config") {
    Copy-Item "$sourceDir\TarkovQuestScanner.exe.config" -Destination $destDir
}

Write-Host "Copying resources..."
if (Test-Path "$sourceDir\Resources") {
    Copy-Item -Recurse "$sourceDir\Resources" -Destination $destDir
}

Write-Host "Copying native dependencies..."
# OpenCvSharpExtern is often in the root
if (Test-Path "$sourceDir\OpenCvSharpExtern.dll") {
    Copy-Item "$sourceDir\OpenCvSharpExtern.dll" -Destination $destDir
}

if (Test-Path "$sourceDir\tpv.ico") {
    Copy-Item "$sourceDir\tpv.ico" -Destination $destDir
}

Write-Host "Copying documentation..."
if (Test-Path "README.md") {
    Copy-Item "README.md" -Destination $destDir
}
if (Test-Path "INSTALL_INSTRUCTIONS.txt") {
    Copy-Item "INSTALL_INSTRUCTIONS.txt" -Destination $destDir
}

# Copy the 'dll' folder which contains Sdcb/PaddleOCR native libs
if (Test-Path "$sourceDir\dll") {
    Copy-Item -Recurse "$sourceDir\dll" -Destination $destDir
}

# Create debug directories
New-Item -ItemType Directory -Force -Path "$destDir\debug_images" | Out-Null

Write-Host "Release packaged successfully in $destDir"

$zipFile = "ReleaseBuild.zip"
Write-Host "Creating zip file..."
if (Test-Path $zipFile) { Remove-Item $zipFile }
Compress-Archive -Path "$destDir\*" -DestinationPath $zipFile
Write-Host "Zip file created: $zipFile"