# 1. Build Stage (.NET 10 SDK nutzen)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Kopiere alle .csproj-Dateien
COPY ["dotnet/app/Veloce.Knowledgebase/Veloce.Knowledgebase.csproj", "dotnet/app/Veloce.Knowledgebase/"]
COPY ["dotnet/src/Veloce.Knowledgebase.Application/Veloce.Knowledgebase.Application.csproj", "dotnet/src/Veloce.Knowledgebase.Application/"]
COPY ["dotnet/src/Veloce.Knowledgebase.Application.Contracts/Veloce.Knowledgebase.Application.Contracts.csproj", "dotnet/src/Veloce.Knowledgebase.Application.Contracts/"]
COPY ["dotnet/src/Veloce.Knowledgebase.Domain/Veloce.Knowledgebase.Domain.csproj", "dotnet/src/Veloce.Knowledgebase.Domain/"]
COPY ["dotnet/src/Veloce.Knowledgebase.Domain.Shared/Veloce.Knowledgebase.Domain.Shared.csproj", "dotnet/src/Veloce.Knowledgebase.Domain.Shared/"]
COPY ["dotnet/src/Veloce.Knowledgebase.EntityFrameworkCore/Veloce.Knowledgebase.EntityFrameworkCore.csproj", "dotnet/src/Veloce.Knowledgebase.EntityFrameworkCore/"]
COPY ["dotnet/src/Veloce.Knowledgebase.EntityFrameworkCore.PostgreSQL/Veloce.Knowledgebase.EntityFrameworkCore.PostgreSQL.csproj", "dotnet/src/Veloce.Knowledgebase.EntityFrameworkCore.PostgreSQL/"]
COPY ["dotnet/src/Veloce.Knowledgebase.HttpApi/Veloce.Knowledgebase.HttpApi.csproj", "dotnet/src/Veloce.Knowledgebase.HttpApi/"]

# NuGet-Pakete wiederherstellen
RUN dotnet restore "dotnet/app/Veloce.Knowledgebase/Veloce.Knowledgebase.csproj"

# Quellcode kopieren & veröffentlichen
COPY . .
WORKDIR "/src/dotnet/app/Veloce.Knowledgebase"
RUN dotnet publish "Veloce.Knowledgebase.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime Stage (.NET 10 ASP.NET Runtime nutzen)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Veloce.Knowledgebase.dll"]