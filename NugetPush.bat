set version=%1
set apiKey=%2

cd %~dp0\Collie.Abstractions\bin\Release && dotnet nuget push Collie.Abstractions.%version%.nupkg --api-key %apiKey% --source https://api.nuget.org/v3/index.json
cd %~dp0\Collie\bin\Release && dotnet nuget push Collie.%version%.nupkg --api-key %apiKey% --source https://api.nuget.org/v3/index.json
cd %~dp0\Collie.Compatibility.Abstractions\bin\Release && dotnet nuget push Collie.Compatibility.Abstractions.%version%.nupkg --api-key %apiKey% --source https://api.nuget.org/v3/index.json
cd %~dp0\Collie.Compatibility\bin\Release && dotnet nuget push Collie.Compatibility.%version%.nupkg --api-key %apiKey% --source https://api.nuget.org/v3/index.json
cd %~dp0