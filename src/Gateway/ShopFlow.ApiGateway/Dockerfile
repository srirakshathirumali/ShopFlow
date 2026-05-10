FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Gateway/ShopFlow.ApiGateway/ShopFlow.ApiGateway.csproj", "src/Gateway/ShopFlow.ApiGateway/"]

RUN dotnet restore "src/Gateway/ShopFlow.ApiGateway/ShopFlow.ApiGateway.csproj"

COPY . .

WORKDIR "/src/src/Gateway/ShopFlow.ApiGateway"
RUN dotnet build "ShopFlow.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ShopFlow.ApiGateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ShopFlow.ApiGateway.dll"]