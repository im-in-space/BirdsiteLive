#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS publish
COPY ./src/ ./src/
RUN dotnet publish "/src/BirdsiteLive/BirdsiteLive.csproj" -c Release -o /app/publish
RUN dotnet publish "/src/BSLManager/BSLManager.csproj" -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeAllContentForSelfExtract=true -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN apt-get update && apt-get install -y libncurses-dev && rm -rf /var/lib/apt/lists/*
ENTRYPOINT ["dotnet", "BirdsiteLive.dll"]
