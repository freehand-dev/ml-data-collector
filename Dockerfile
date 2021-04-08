#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 58942
EXPOSE 58943

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["ml-data-collector/ml-data-collector.csproj", "ml-data-collector/"]
RUN dotnet restore "ml-data-collector/ml-data-collector.csproj"
COPY . .
WORKDIR "/src/ml-data-collector"
RUN dotnet build "ml-data-collector.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ml-data-collector.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ml-data-collector.dll"]