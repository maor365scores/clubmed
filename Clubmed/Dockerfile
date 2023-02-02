FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Clubmed/Clubmed.csproj", "Clubmed/"]
RUN dotnet restore "Clubmed/Clubmed.csproj"
COPY . .
WORKDIR "/src/Clubmed"
RUN dotnet build "Clubmed.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Clubmed.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Clubmed.dll"]
