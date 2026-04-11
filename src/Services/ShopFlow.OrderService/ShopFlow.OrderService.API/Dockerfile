# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/ShopFlow.OrderService/ShopFlow.OrderService.API/ShopFlow.OrderService.API.csproj", "src/Services/ShopFlow.OrderService/ShopFlow.OrderService.API/"]
COPY ["src/Services/ShopFlow.OrderService/ShopFlow.OrderService.Application/ShopFlow.OrderService.Application.csproj", "src/Services/ShopFlow.OrderService/ShopFlow.OrderService.Application/"]
COPY ["src/Shared/ShopFlow.Contracts/ShopFlow.Contracts.csproj", "src/Shared/ShopFlow.Contracts/"]
COPY ["src/Services/ShopFlow.OrderService/ShopFlow.OrderService.Domain/ShopFlow.OrderService.Domain.csproj", "src/Services/ShopFlow.OrderService/ShopFlow.OrderService.Domain/"]
COPY ["src/Services/ShopFlow.OrderService/ShopFlow.OrderService.Infrastructure/ShopFlow.OrderService.Infrastructure.csproj", "src/Services/ShopFlow.OrderService/ShopFlow.OrderService.Infrastructure/"]
COPY ["src/Services/ShopFlow.InventoryService/ShopFlow.InventoryService.Domain/ShopFlow.InventoryService.Domain.csproj", "src/Services/ShopFlow.InventoryService/ShopFlow.InventoryService.Domain/"]
RUN dotnet restore "./src/Services/ShopFlow.OrderService/ShopFlow.OrderService.API/ShopFlow.OrderService.API.csproj"
COPY . .
WORKDIR "/src/src/Services/ShopFlow.OrderService/ShopFlow.OrderService.API"
RUN dotnet build "./ShopFlow.OrderService.API.csproj" -c Release -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ShopFlow.OrderService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShopFlow.OrderService.API.dll"]