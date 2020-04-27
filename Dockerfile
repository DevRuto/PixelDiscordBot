#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["PixelDiscordBot/PixelDiscordBot.csproj", "PixelDiscordBot/"]
RUN dotnet restore "PixelDiscordBot/PixelDiscordBot.csproj"
COPY . .
WORKDIR "/src/PixelDiscordBot"
RUN dotnet build "PixelDiscordBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PixelDiscordBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:8090
ENTRYPOINT ["dotnet", "PixelDiscordBot.dll"]