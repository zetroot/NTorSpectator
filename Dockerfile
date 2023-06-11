FROM --platform=linux/arm64 mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 8000

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["NTorSpectator/NTorSpectator.csproj", "NTorSpectator/"]
RUN dotnet restore -r linux-arm64 "NTorSpectator/NTorSpectator.csproj"
COPY . .
WORKDIR "/src/NTorSpectator"
RUN dotnet build -r linux-arm64 --no-self-contained --no-restore "NTorSpectator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NTorSpectator.csproj" -c Release -o /app/publish -r linux-arm64 --no-self-contained /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NTorSpectator.dll"]
