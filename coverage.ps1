function GenerateTestCoverageReport {
    param (
        [string[]]$Projects,         # Array of paths to project test DLLs.
        [string]$ResultsDirectory = ".\results",    # Path to the directory where results should be stored. Default is '.\results'.
        [string]$Solution             # Path to the solution file to be built.
    )

    # Validate Solution
    if (-not $Solution) {
        # Try to find a solution file if not provided
        $solutionFiles = Get-ChildItem -Path "." -Filter "*.sln" | Select-Object -First 2
        if ($solutionFiles.Count -eq 1) {
            $Solution = $solutionFiles[0].FullName
            Write-Output "Using found solution file: $Solution"
        }
        elseif ($solutionFiles.Count -gt 1) {
            Write-Error "Multiple solution files found. Please specify which one to use."
            return
        }
        else {
            Write-Error "No solution file found. Please provide a solution file to build."
            return
        }
    }
    # Check if reportgenerator is installed
    if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
        Write-Output "ReportGenerator not found. Installing..."
        dotnet tool install -g dotnet-reportgenerator-globaltool
    }

    dotnet build $Solution
    # Validate projects
    if (-not $Projects) {
        $Projects = Get-TestProjectDlls
        if (-not $Projects) {
            Write-Error "No test projects found. Please provide at least one project to run tests against."
            return
        }
        foreach($project in $Projects)
        {
            Write-Output "Found: $project projects to run tests against"
        }
    }
    if (-not $ResultsDirectory) {
        Write-Error "You must provide a results directory."
        return
    }

    # Ensure results directory exists
    if (-not (Test-Path -Path $ResultsDirectory)) {
        New-Item -ItemType Directory -Path $ResultsDirectory | Out-Null
    }
    #

    $coverageFiles = @()

    # Loop through each project, run tests, and gather coverage reports
    foreach ($project in $Projects) {
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project)
        $projectResultsDirectory = "$ResultsDirectory\$projectName"

        # Run Tests
        dotnet test --collect:"XPlat Code Coverage" /TestAdapterPath:"$env:UserProfile\.nuget\packages\coverlet.collector\6.0.0\build" --results-directory "$projectResultsDirectory" $project

        # Find the latest coverage report
        $coverageFile = Get-ChildItem -Path "$projectResultsDirectory\**\*" -Filter 'coverage.cobertura.xml' | Sort-Object LastWriteTime -Descending | Select-Object -First 1

        if ($coverageFile) {
            # Add coverage file to the list for combined reporting
            $coverageFiles += $coverageFile.FullName
        }
        else {
            Write-Error "No coverage report found for $projectName."
        }
    }

    # Generate a combined report if coverage files exist
    if ($coverageFiles.Count -gt 0) {
        $combinedReportOutputDirectory = "$ResultsDirectory\CombinedCoverageReport"
        reportgenerator -reports:($coverageFiles -join ";") -targetdir:$combinedReportOutputDirectory

        # Display the combined report
        $indexFilePath = "$combinedReportOutputDirectory\index.html"
        if (Test-Path -Path $indexFilePath) {
            Start-Process $indexFilePath
        }
        else {
            Write-Error "Combined report generation failed."
        }
    }
    else {
        Write-Error "No coverage files were found to generate a combined report."
    }
}

function Get-TestProjectDlls {
    # Get all test project DLLs from the current directory and subdirectories
    $testDlls = Get-ChildItem -Path . -Recurse -Filter "*.dll" | Where-Object {
        $_.DirectoryName -match "bin\\.*\\" -and $_.Name -match "Tests.dll$"
    }

    if ($testDlls.Count -eq 0) {
        return $null
    }
    else {
        return $testDlls.FullName
    }
}

# Example usage:
# GenerateTestCoverageReport -Projects @(".\AdSecCoreTests\bin\Debug\net7.0\AdSecCoreTests.dll", ".\AdSecGHTests\bin\x64\Debug\net48\AdSecGHTests.dll", ".\IntegrationTests\bin\x64\Debug\net48\IntegrationTests.dll") -ResultsDirectory ".\results" -Solution AdSecGH.sln
#

GenerateTestCoverageReport -Solution AdSecGH.sln
