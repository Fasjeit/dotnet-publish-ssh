Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$srcPath = "./src/DotnetPublishSsh"

dotnet clean $srcPath
dotnet restore $srcPath
dotnet build $srcPath
dotnet pack $srcPath
dotnet tool install --add-source $srcPath/nupkg/ --global dotnet-publish-ssh