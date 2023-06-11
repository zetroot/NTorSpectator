FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 8000

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["NTorSpectator/NTorSpectator.csproj", "NTorSpectator/"]
RUN dotnet restore "NTorSpectator/NTorSpectator.csproj"
COPY . .
WORKDIR "/src/NTorSpectator"
RUN dotnet build "NTorSpectator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NTorSpectator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NTorSpectator.dll"]
