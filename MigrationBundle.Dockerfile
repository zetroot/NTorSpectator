FROM --platform=linux/arm64 mcr.microsoft.com/dotnet/runtime:7.0 AS base
#FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0.102-jammy-amd64 AS build
RUN dotnet tool install --global dotnet-ef
ENV PATH="${PATH}:/root/.dotnet/tools"
WORKDIR /src
COPY ["src/", "./"]
RUN mkdir "/publish"
WORKDIR "/src/NTorSpectator.Database"
RUN dotnet ef migrations bundle -r linux-arm64 -f -o /publish/bundle --verbose

FROM base AS final
WORKDIR /app
COPY --from=build /publish .
