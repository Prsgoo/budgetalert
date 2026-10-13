FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["workers/BudgetAlert.Worker/BudgetAlert.Worker.csproj", "workers/BudgetAlert.Worker/"]
COPY ["src/BudgetAlert.Application/BudgetAlert.Application.csproj", "src/BudgetAlert.Application/"]
COPY ["src/BudgetAlert.Domain/BudgetAlert.Domain.csproj", "src/BudgetAlert.Domain/"]
COPY ["src/BudgetAlert.Infrastructure/BudgetAlert.Infrastructure.csproj", "src/BudgetAlert.Infrastructure/"]
RUN dotnet restore "workers/BudgetAlert.Worker/BudgetAlert.Worker.csproj"
COPY . .
RUN dotnet publish "workers/BudgetAlert.Worker/BudgetAlert.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BudgetAlert.Worker.dll"]
