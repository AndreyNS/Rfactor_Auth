
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 10021
EXPOSE 10022


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["VoskRecognition/VoskRecognition.csproj", "VoskRecognition/"]
RUN dotnet restore "./VoskRecognition/VoskRecognition.csproj"
COPY . .
WORKDIR "/src/VoskRecognition"
RUN dotnet build "./VoskRecognition.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./VoskRecognition.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VoskRecognition.dll"]