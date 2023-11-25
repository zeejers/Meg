# Package and Release
```
dotnet pack -c release -o nupkg
dotnet tool install --add-source ./nupkg -g meg
dotnet nuget push ./nupkg/meg.0.0.1.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```