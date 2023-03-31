dotnet nuget remove source "github"
dotnet nuget add source --name "github" "https://nuget.pkg.github.com/scyna/index.json" --username hongsan --password "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" --store-password-in-clear-text
dotnet pack --configuration Release
dotnet nuget push "bin/Release/Scyna.Net.1.0.6.nupkg" --source "github"