dotnet tool uninstall --global dotnet-publish-ssh
dotnet clean
dotnet restore
dotnet build
dotnet pack
dotnet tool install --add-source .\nupkg\ --global dotnet-publish-ssh