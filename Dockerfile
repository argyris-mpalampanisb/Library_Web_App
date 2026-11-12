FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY Library_Web_App/*.csproj .
RUN dotnet restore
COPY Library_Web_App/ .
RUN dotnet publish Library_Web_App.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Library_Web_App.dll"]