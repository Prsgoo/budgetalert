# Azure Infrastructure

Provisions Azure SQL (free tier) and Azure Service Bus (Basic tier) in one command.

## Prerequisites

- Azure CLI installed (`winget install Microsoft.AzureCLI`)
- Logged in (`az login`)
- Your current public IP (`curl -s ifconfig.me` or `(Invoke-WebRequest -Uri https://ifconfig.me -UseBasicParsing).Content`)

## Deploy

```bash
# Create resource group
az group create --name budgetalert-rg --location westeurope

# Deploy - outputs the connection strings when done
az deployment group create \
  --resource-group budgetalert-rg \
  --template-file infra/main.bicep \
  --parameters infra/main.bicepparam \
  --parameters sqlAdminPassword=<your-sql-password> \
  --parameters developerIp=<your-public-ip>
```

Copy the two output values into `appsettings.json` for both Api and Worker:

- `sqlConnectionString` - goes into `ConnectionStrings:Default`
- `serviceBusConnectionString` - goes into `Messaging:AzureServiceBus:ConnectionString`

Also set `Messaging:Provider` to `AzureServiceBus`.

## Post-deployment SQL step

Bicep cannot create an Entra ID SQL user (requires an active DB connection). Run this once in Azure Data Studio or the portal Query editor, connected as the SQL admin:

```sql
CREATE USER [your-email@domain.com] FROM EXTERNAL PROVIDER;
ALTER ROLE db_owner ADD MEMBER [your-email@domain.com];
```

This is required for PRS-105 (Managed Identity / DefaultAzureCredential access).

## Cost

The SQL database is serverless with autopause after 1 hour idle - costs cents per active hour. Service Bus Basic is ~$0.05/month. Total for a short demo session is negligible against the $200 free credit.

## Tear down

```bash
az group delete --name budgetalert-rg --yes
```

This deletes all resources and stops all billing immediately.
