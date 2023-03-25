# https://hub.docker.com/_/microsoft-dotnet
#FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
#WORKDIR /source

# copy csproj and restore as distinct layers
#COPY *.sln .
#COPY Dashboard/*.csproj ./Dashboard/
#RUN dotnet restore

# copy everything else and build app
#COPY Dashboard/. ./Dashboard/
#WORKDIR /source/Dashboard
#RUN dotnet publish -c release -o /app --no-restore

# final stage/image
#FROM mcr.microsoft.com/dotnet/aspnet:7.0
#WORKDIR /app
#COPY --from=build /app ./
#ENTRYPOINT ["dotnet", "ArkThor_dashboard.dll"]

# Final- Create Image with Pre-Published Build artifacts
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
EXPOSE 80
EXPOSE 443
EXPOSE 24297
COPY published/ ./
ENTRYPOINT ["dotnet", "ArkThor_dashboard.dll"]