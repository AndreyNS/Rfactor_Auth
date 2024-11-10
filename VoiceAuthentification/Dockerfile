
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 10201
EXPOSE 10202

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VoiceAuthentification/VoiceAuthentification.csproj", "VoiceAuthentification/"]
RUN dotnet restore "./VoiceAuthentification/VoiceAuthentification.csproj"
COPY . .
WORKDIR "/src/VoiceAuthentification"
RUN dotnet build "./VoiceAuthentification.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./VoiceAuthentification.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VoiceAuthentification.dll"]