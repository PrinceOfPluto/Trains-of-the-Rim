$sourceDir = "..\"
$destinationDir = "..\..\Trains of the Rim"

$foldersToInclude = @("Defs", "Languages", "Patches", "Sounds", "Textures", "Assemblies");

# Check if source directory exists
if (-Not (Test-Path -Path $sourceDir)) {
    Write-Host "Source directory does not exist: $sourceDir"
    exit 1
}

# Check if destination directory exists; create it if it does not
if (-Not (Test-Path -Path $destinationDir)) {
    try {
        New-Item -ItemType Directory -Path $destinationDir
        Write-Host "Created destination directory: $destinationDir"
    } catch {
        Write-Host "Failed to create destination directory: $destinationDir"
        exit 1
    }
}

foreach ($folderName in $foldersToInclude) {
    $sourceFolderPath = Join-Path -Path $sourceDir -ChildPath $folderName
    $destinationFolderPath = Join-Path -Path $destinationDir -ChildPath $folderName

    if (Test-Path -Path $sourceFolderPath) {
        try {
            Copy-Item -Path $sourceFolderPath -Destination $destinationFolderPath -Recurse -Force
            Write-Host "Copied folder $folderName to $destinationDir"
        } catch {
            Write-Host "Failed to copy folder $folderName"
        }
    } else {
        Write-Host "Folder not found: $sourceFolderPath"
    }
}