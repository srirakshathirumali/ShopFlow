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
COPY ["src/Services/ShopFlow.PaymentService/ShopFlow.PaymentService.API/ShopFlow.PaymentService.API.csproj", "src/Services/ShopFlow.PaymentService/ShopFlow.PaymentService.API/"]
COPY ["src/Services/ShopFlow.PaymentService/ShopFlow.PaymentService.Application/ShopFlow.PaymentService.Application.csproj", "src/Services/ShopFlow.PaymentService/ShopFlow.PaymentService.Application/"]
COPY ["src/Shared/ShopFlow.Contracts/ShopFlow.Contracts.csproj", "src/Shared/ShopFlow.Contracts/"]
COPY ["src/Services/ShopFlow.PaymentService/ShopFlow.PaymentService.Domain/ShopFlow.PaymentService.Domain.csproj", "src/Services/ShopFlow.PaymentService/ShopFlow.PaymentService.Domain/"]
COPY ["src/Services/ShopFlow.PaymentService/ShopFlow.PaymentService.Infrastructure/ShopFlow.PaymentService.Infrastructure.csproj", "src/Services/ShopFlow.PaymentService/ShopFlow.PaymentService.Infrastructure/"]
RUN dotnet restore "./src/Services/ShopFlow.PaymentService/ShopFlow.PaymentService.API/ShopFlow.PaymentService.API.csproj"
COPY . .
WORKDIR "/src/src/Services/ShopFlow.PaymentService/ShopFlow.PaymentService.API"
RUN dotnet build "./ShopFlow.PaymentService.API.csproj" -c Release -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ShopFlow.PaymentService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShopFlow.PaymentService.API.dll"]