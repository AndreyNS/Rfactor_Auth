FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 10221
EXPOSE 10222


USER root
RUN apt-get update && apt-get install -y libatomic1
RUN apt-get update && apt-get install -y ffmpeg


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
COPY --from=build /src/VoskRecognition/VoskModel /app/VoskModel

ENV LD_LIBRARY_PATH=/app/VoskModel

ENTRYPOINT ["dotnet", "VoskRecognition.dll"]