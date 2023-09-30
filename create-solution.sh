rm csharp.sln

dotnet new sln -n csharp
dotnet sln add scyna
dotnet sln add example/hello
dotnet sln add example/hello-test
dotnet sln add example/registering
dotnet sln add example/registering-test

dotnet restore

