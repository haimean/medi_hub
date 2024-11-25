#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["", ""]
RUN dotnet restore "MediHub.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "MediHub.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MediHub.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
ARG ASPNETCORE_ENVIRONMENT

WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:8010
ENV ASPNETCORE_ENVIRONMENT $ASPNETCORE_ENVIRONMENT
ENTRYPOINT ["dotnet", "MediHub.dll"]