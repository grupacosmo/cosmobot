dotnet tool restore
dotnet tool run dotnet-format Formatting.csproj --include Assets/Scripts/ --verify-no-changes --severity warn -v d --check --fix-whitespace