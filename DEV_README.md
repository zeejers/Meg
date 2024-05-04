# Package and Release

```bash
dotnet pack -c release ./Meg.fsproj -o nupkg
dotnet tool install --add-source ./nupkg -g meg
dotnet nuget push ./nupkg/meg.0.0.1.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

<!-- Some testing -->

```
dotnet run --framework "net8.0" -- migrate -d dev -c 'Server=localhost; Port=54322; Data
base=gggg; User Id=postgres; Password=postgres;'
```
