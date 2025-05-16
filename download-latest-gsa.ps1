$repoOwner = "arup-group"
$repoName = "GSA-Grasshopper"
$apiUrl = "https://api.github.com/repos/$repoOwner/$repoName/releases/latest"

# Set GitHub-required User-Agent header
$headers = @{ "User-Agent" = "PowerShell" }

try {
    # Get latest release info
    $release = Invoke-RestMethod -Uri $apiUrl -Headers $headers

    $tagName = $release.tag_name
    Write-Host "Latest release: $tagName"

    if ($release.assets.Count -eq 0) {
        Write-Host "No assets found in the latest release."
        return
    }

    # Find asset matching pattern gsa-X.X.X-rh7_0-win.yak
    $asset = $release.assets | Where-Object { $_.name -match '^gsa-\d+\.\d+\.\d+-rh7_0-win\.yak$' }

    if (-not $asset) {
        Write-Host "No matching asset found."
        return
    }

    $downloadUrl = $asset.browser_download_url
    $fileName = $asset.name

    Write-Host "Downloading: $fileName"
    Invoke-WebRequest -Uri $downloadUrl -OutFile $fileName -Headers $headers

    Write-Host "Downloaded to: $(Resolve-Path $fileName)"
}
catch {
    Write-Host "Error: $_"
}

# rename yak to zip

if ($fileName -like "*.yak") {
    $newFileName = $fileName -replace '\.yak$', '.zip'
    Rename-Item -Path $fileName -NewName $newFileName
    Write-Host "Renamed to: $(Resolve-Path $newFileName)"
    $fileName = $newFileName
} else {
    Write-Host "Downloaded file is not a YAK archive. Skipping renaming."
}

# Extract zip file to GSA-GH folder
if ($fileName -like "*.zip") {
    $extractPath = "GSA-GH"
    if (Test-Path $extractPath) {
        Remove-Item -Recurse -Force $extractPath
    }
    Expand-Archive -Path $fileName -DestinationPath $extractPath
    Write-Host "Extracted to: $(Resolve-Path $extractPath)"
} else {
    Write-Host "Downloaded file is not a ZIP archive. Skipping extraction."
}
