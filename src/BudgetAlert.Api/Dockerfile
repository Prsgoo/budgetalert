FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/BudgetAlert.Api/BudgetAlert.Api.csproj", "src/BudgetAlert.Api/"]
COPY ["src/BudgetAlert.Application/BudgetAlert.Application.csproj", "src/BudgetAlert.Application/"]
COPY ["src/BudgetAlert.Domain/BudgetAlert.Domain.csproj", "src/BudgetAlert.Domain/"]
COPY ["src/BudgetAlert.Infrastructure/BudgetAlert.Infrastructure.csproj", "src/BudgetAlert.Infrastructure/"]
RUN dotnet restore "src/BudgetAlert.Api/BudgetAlert.Api.csproj"
COPY . .
RUN dotnet publish "src/BudgetAlert.Api/BudgetAlert.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BudgetAlert.Api.dll"]
