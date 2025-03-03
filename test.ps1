dotnet build AdSecGh.sln

$projects = @(
    "AdSecCoreTests\AdSecCoreTests.csproj"
    # "AdSecGhTests\AdSecGhTests.csproj"
    # "IntegrationTests\IntegrationTests.csproj"
)


foreach ($project in $projects) {
    $trxPath = "$(Get-Location)\$(Split-Path -Leaf $project).trx"
    Write-Host "Running tests for $project..."
    dotnet test $project --no-build --logger "trx;LogFileName=$trxPath"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Tests failed for $project. Exiting..."
        exit $LASTEXITCODE
    }
    Start-Sleep -Seconds 2  # Add a delay between tests
}
# Generate a report
Write-Host "Generating test report..."
reportgenerator "-reports:**/TestResults/*.trx" "-targetdir:test-results" "-reporttypes:Html"

# Open the report
Start-Process "test-results/index.html"

Write-Host "Press any key to exit..."
[void][System.Console]::ReadKey($true)
