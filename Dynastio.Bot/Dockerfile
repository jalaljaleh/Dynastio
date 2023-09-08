#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Dynastio.Bot/Dynastio.Bot.csproj", "Dynastio.Bot/"]
COPY ["Dynastio.Data/Dynastio.Data.csproj", "Dynastio.Data/"]
COPY ["Dynastio.Bot.Global/Dynastio.Bot.Global.csproj", "Dynastio.Bot.Global/"]
COPY ["Dynastio.Bot.Globalization/Dynastio.Bot.Globalization.csproj", "Dynastio.Bot.Globalization/"]
COPY ["Dynastio.Bot.Test/Dynastio.Bot.Test.csproj", "Dynastio.Bot.Test/"]
COPY ["Dynastio.Net/Dynastio.Net.csproj", "Dynastio.Net/"]
RUN dotnet restore "Dynastio.Bot/Dynastio.Bot.csproj"
COPY . .
WORKDIR "/src/Dynastio.Bot"
RUN dotnet build "Dynastio.Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dynastio.Bot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dynastio.Bot.dll"]