# cbuild

# dotnet build AdSecGH.sln -c Release
# dotnet stryker -s AdSecCore.sln --test-project AdSecCoreTests/AdSecCoreTests.csproj -l advanced --dev-mode --no-incremental -v:d
# dotnet stryker -s AdSecCore.sln --test-project AdSecCoreTests/AdSecCoreTests.csproj -l advanced --no-incremental -v:d

# testing
# dotnet test --collect:"XPlat Code Coverage" /TestAdapterPath:$env:UserProfile\.nuget\packages\coverlet.collector\6.0.2\build --results-directory .\results\adsecCore AdSecCoreTests\bin\x64\Release\net7.0\AdSecCoreTests.dll
# dotnet test --collect:"XPlat Code Coverage" /TestAdapterPath:$env:UserProfile\.nuget\packages\coverlet.collector\6.0.2\build --results-directory .\results\adsecgh AdSecGHTests\bin\x64\Release\net48\AdSecGHTests.dll
# dotnet test --collect:"XPlat Code Coverage" /TestAdapterPath:$env:UserProfile\.nuget\packages\coverlet.collector\6.0.2\build --results-directory .\results\integration  IntegrationTests\bin\x64\Release\net48\IntegrationTests.dll

Get-ChildItem -Path "AdSecCore\" -Recurse -Filter *.cs | ForEach-Object {
    $content = Get-Content $_.FullName
    "`r`n" + ($content -join "`r`n") | Set-Content $_.FullName
}
