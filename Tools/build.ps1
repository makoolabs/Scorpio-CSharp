$version = "1.0.11"
$name = "sco"


$today = Get-Date
$date = $today.ToString('yyyy-MM-dd')
Remove-Item ../bin/* -Force -Recurse

Set-Location ../Scorpio
$fileData = @"
namespace Scorpio {
    public static class Version {
        public const string version = "$version";
        public const string date = "$date";
    }
}
"@
$fileData | Out-File -Encoding utf8 ./src/Version.cs
dotnet restore
dotnet build
dotnet pack -p:PackageVersion=$version -o ../bin

Set-Location ../ScorpioExec
dotnet publish -c release -o ../bin/$name-win-x64 -r win-x64
dotnet publish -c release -o ../bin/$name-osx-x64 -r osx-x64
dotnet publish -c release -o ../bin/$name-linux-x64 -r linux-x64


Compress-Archive ../bin/sco-win-x64 ../bin/$name-$version-win-x64.zip -Force
Compress-Archive ../bin/sco-osx-x64 ../bin/$name-$version-osx-x64.zip -Force
Compress-Archive ../bin/sco-linux-x64 ../bin/$name-$version-linux-x64.zip -Force

Remove-Item ../ScorpioTest/Assets/Scorpio/ -Force -Recurse
Remove-Item ../ScorpioTest/Assets/Editor/ScorpioReflect -Force -Recurse
Copy-Item ../Scorpio/src/ ../ScorpioTest/Assets/Scorpio/ -Recurse -Force 
Copy-Item ../ScorpioReflect/src/ ../ScorpioTest/Assets/Editor/ScorpioReflect/ -Recurse -Force
