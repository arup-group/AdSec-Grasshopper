
function Has-Version {
  param ($version)

  # Check if the version argument is provided
  if ($version.Count -eq 0) {
      Write-Host "Please provide the version number as an argument. Usage: .\bump-version.ps1 <new-version>"
        exit
  }

  # Get the new version from the CLI argument
  return $version[0]
}

$newVersion = Has-Version($args)

# Function to validate the version format (X.X.X where X is a number)
function Validate-VersionFormat {
    param (
        [string]$version
    )

    # Regex pattern for validating version format (X.X.X)
    $versionPattern = '^\d+\.\d+\.\d+$'

    # Check if version matches the pattern
    return $version -match $versionPattern
}

# Function to update a file's contents based on a search/replacement pattern
function Update-FileContent {
    param (
        [string]$filePath,
        [string]$searchPattern,
        [string]$replacementPattern,
        [string]$successMessage
    )

    # Read the content of the file
    $content = Get-Content $filePath

    # Replace based on the provided pattern and replacement
    $updatedContent = $content -replace $searchPattern, $replacementPattern

    # Write the updated content back to the file
    Set-Content $filePath -Value $updatedContent

    Write-Host $successMessage
}

# Check if the version format is valid
if (-not (Validate-VersionFormat $newVersion)) {
    Write-Host "Invalid version format. Please use the format: X.X.X where X is a number."
    exit
}

# Current year, used to stamp the "published" copyright year in the entries below
$currentYear = (Get-Date).Year

# Define the paths and patterns for each file to update.
# Copyright entries use a capture group around the fixed prefix so only the
# trailing year is replaced, e.g. "1985 - 2025" -> "1985 - 2026".
$filesToUpdate = @(
    @{
        FilePath = ".\AdSecGH\AdSecGH.csproj"
        SearchPattern = '<Version>(.*?)<\/Version>'
        ReplacementPattern = "<Version>$newVersion-beta</Version>"
        SuccessMessage = "Updated version in .\AdSecGH\AdSecGH.csproj to $newVersion"
    },
    @{
        FilePath = ".\AdSecGH\AdSecGHInfo.cs"
        SearchPattern = 'string Vers = "(.*?)"'
        ReplacementPattern = 'string Vers = "' + $newVersion + '"'
        SuccessMessage = "Updated version in .\AdSecGH\AdSecGHInfo.cs to $newVersion"
    },
    @{
        FilePath = ".\LICENSE"
        SearchPattern = '(Copyright \(c\) 2021-)\d{4}'
        ReplacementPattern = "`${1}$currentYear"
    },
    @{
        FilePath = ".\AdSecGH\LICENSE"
        SearchPattern = '(Copyright \(c\) 2021-)\d{4}'
        ReplacementPattern = "`${1}$currentYear"
    },
    @{
        FilePath = ".\AdSecGH\AdSecGHInfo.cs"
        SearchPattern = '(Copyright © Oasys 1985 - )\d{4}'
        ReplacementPattern = "`${1}$currentYear"
    },
    @{
        FilePath = ".\AdSecGH\UI\AboutBox.cs"
        SearchPattern = '(Copyright © Oasys 1985 - )\d{4}'
        ReplacementPattern = "`${1}$currentYear"
    },
    @{
        FilePath = ".\AdSecGH\AdSecGH.csproj"
        SearchPattern = '(Copyright © Oasys 1985 - )\d{4}'
        ReplacementPattern = "`${1}$currentYear"
    }
)

# Loop through each file and apply its update
foreach ($file in $filesToUpdate) {
    $message = if ($file.SuccessMessage) { $file.SuccessMessage } else { "Updated copyright year in $($file.FilePath) to $currentYear" }
    Update-FileContent -filePath $file.FilePath -searchPattern $file.SearchPattern -replacementPattern $file.ReplacementPattern -successMessage $message
}

Write-Host "Version update completed."
