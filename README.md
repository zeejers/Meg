# MEG - Dotnet Migration Tool

## Package and Release
```
dotnet pack -c release -o nupkg
dotnet tool install --add-source ./nupkg -g meg
dotnet tool install -g meg
```