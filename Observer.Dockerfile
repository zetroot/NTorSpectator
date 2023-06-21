FROM --platform=linux/arm64 mcr.microsoft.com/dotnet/aspnet:7.0-jammy-arm64v8 AS base
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8000
EXPOSE 8000

FROM mcr.microsoft.com/dotnet/sdk:7.0.102-jammy-amd64 AS build
WORKDIR /src
COPY ["src/NTorSpectator.Database/NTorSpectator.Database.csproj", "NTorSpectator.Database/"]
COPY ["src/NTorSpectator.Services/NTorSpectator.Services.csproj", "NTorSpectator.Services/"]
COPY ["src/NTorSpectator.Observer/NTorSpectator.Observer.csproj", "NTorSpectator.Observer/"]
RUN dotnet restore -r linux-arm64 "NTorSpectator.Observer/NTorSpectator.Observer.csproj"
COPY /src .
WORKDIR "/src/NTorSpectator.Observer"
RUN dotnet build -r linux-arm64 --no-self-contained --no-restore "NTorSpectator.Observer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NTorSpectator.Observer.csproj" -c Release -o /app/publish -r linux-arm64 --no-self-contained

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NTorSpectator.Observer.dll"]
