#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Dynastio.Bot/Dynastio.Bot.csproj", "Dynastio.Bot/"]
COPY ["Dynastio.Bot.Database/Dynastio.Bot.Database.csproj", "Dynastio.Bot.Database/"]
COPY ["Dynastio.Bot.Global/Dynastio.Bot.Global.csproj", "Dynastio.Bot.Global/"]
COPY ["Dynastio.Net/Dynastio.Net.csproj", "Dynastio.Net/"]
COPY ["Dynastio.Bot.Globalization/Dynastio.Bot.Globalization.csproj", "Dynastio.Bot.Globalization/"]
COPY ["Dynastio.Graphic/Dynastio.Graphic.csproj", "Dynastio.Graphic/"]
RUN dotnet restore "./Dynastio.Bot/Dynastio.Bot.csproj"
COPY . .
WORKDIR "/src/Dynastio.Bot"
RUN dotnet build "./Dynastio.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Dynastio.Bot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dynastio.Bot.dll"]