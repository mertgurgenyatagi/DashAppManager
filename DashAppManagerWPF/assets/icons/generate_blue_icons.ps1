# Requires: PowerShell 5+, ImageMagick installed and in PATH
# This script tints all PNG icons in assets/icons/ with a blue color and saves them as *_blue.png

$iconDir = "c:\Users\Mert\Desktop\repos\DashAppManager\DashAppManagerWPF\assets\icons"
$blue = "#2563eb" # Tailwind blue-600
$magickPath = 'c:\Program Files\ImageMagick-7.1.2-Q16-HDRI\magick.exe'

Get-ChildItem -Path $iconDir -Filter *.png | ForEach-Object {
    $baseName = $_.BaseName
    $ext = $_.Extension
    $newName = Join-Path $iconDir ($baseName + "_blue" + $ext)
    & $magickPath $_.FullName -fill $blue -colorize 60% $newName
}

Write-Host "Blue icon variants generated in $iconDir."
