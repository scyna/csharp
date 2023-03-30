# dotnet nuget remove source "github"
# dotnet nuget add source --name "github" "https://nuget.pkg.github.com/scyna/index.json" --username hongsan --password "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" --store-password-in-clear-text
dotnet pack --configuration Release
dotnet nuget push "bin/Release/Scyna.Net.1.0.5.nupkg" --source "github"