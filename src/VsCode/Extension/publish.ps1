set DOTNET_PUBLISH=1
dotnet publish --framework netcoreapp2.0 ..\..\Unix\Host\Broker\Impl\Microsoft.R.Host.Broker.Unix.csproj
dotnet publish --framework netcoreapp2.0 ..\LanguageServer\Impl\Microsoft.R.LanguageServer.csproj
set DOTNET_PUBLISH
