$src = '../NosEmu.sln'
$files = Get-ChildItem $src
dotnet build $files --configuration Release

pause