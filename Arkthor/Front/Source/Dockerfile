FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .

COPY ArkThor.UI/. ./ArkThor.UI
COPY ArkThor.API/. ./ArkThor.API

RUN dotnet restore

# install npm
RUN apt-get update && \
    apt-get install -y npm

# copy everything else and build app
WORKDIR ./ArkThor.UI
COPY ArkThor.UI/package*.json ./
RUN npm install
COPY ArkThor.UI/. .
RUN dotnet build --configuration Release --no-restore

RUN dotnet publish -c Release -o published -r linux-x64 --self-contained true /p:UseAppHost=true

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /source/ArkThor.UI/published ./
ENTRYPOINT ["dotnet", "ArkThor.UI.dll"]
