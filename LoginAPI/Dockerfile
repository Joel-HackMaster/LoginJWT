#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LoginAPI/LoginAPI.csproj", "LoginAPI/"]
COPY ["LoginAPI.BLL/LoginAPI.BLL.csproj", "LoginAPI.BLL/"]
COPY ["LoginAPI.DAL/LoginAPI.DAL.csproj", "LoginAPI.DAL/"]
COPY ["LoginAPI.Model/LoginAPI.Model.csproj", "LoginAPI.Model/"]
COPY ["LoginAPI.DTO/LoginAPI.DTO.csproj", "LoginAPI.DTO/"]
COPY ["LoginAPI.Utility/LoginAPI.Utility.csproj", "LoginAPI.Utility/"]
COPY ["LoginAPI.IOC/LoginAPI.IOC.csproj", "LoginAPI.IOC/"]
RUN dotnet restore "./LoginAPI/LoginAPI.csproj"
COPY . .
WORKDIR "/src/LoginAPI"
RUN dotnet build "./LoginAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LoginAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LoginAPI.dll"]