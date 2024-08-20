# Package and Release

```bash
VERSION="1.0.0"
dotnet pack -c release ./src/Meg/Meg.fsproj -o nupkg
dotnet tool install --add-source ./nupkg -g meg
dotnet nuget push "./nupkg/meg.${VERSION}.nupkg" --api-key $NUGET_API_KEY --source "https://api.nuget.org/v3/index.json"
```

<!-- Some testing -->

```
dotnet run --framework "net8.0" --project src/Meg/Meg.fsproj -- migrate -d dev -c 'Server=localhost; Port=54322; Data
base=gggg; User Id=postgres; Password=postgres;'
```

Migration test cmd thing:

```bash
meg gen migration Tester Tester id:id guid:guid uuid:uuid integer:integer int:int float:float numeric:numeric bool:bool boolean:boolean string:string text:text binary:binary array:array record:record date:date time:time
timestamp:timestamp utcdatetime:utcdatetime datetimetz:datetimetz fkfield:guid:references:people:id
```
