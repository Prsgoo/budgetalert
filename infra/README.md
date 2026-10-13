# Azure Infrastructure

Provisions Azure SQL (serverless) and Azure Service Bus (Basic tier) using Bicep.

## Prerequisites

- Azure CLI installed (`winget install Microsoft.AzureCLI`)
- Logged in (`az login`)
- Your current public IP (`curl -s ifconfig.me` or `(Invoke-WebRequest -Uri https://ifconfig.me -UseBasicParsing).Content`)

## Entra ID app registration

Run once in the Azure portal before deploying:

1. Entra ID -> App registrations -> New registration
2. Name: `BudgetAlert`, Supported account types: single tenant
3. Note the `TenantId` and `ClientId` from the Overview page
4. Expose an API: set the Application ID URI to `api://<your-client-id>`, then add a scope named `access_as_user`
5. Add a redirect URI under the **Single-page application** platform: `http://localhost:5143/swagger/oauth2-redirect.html`

See [AZURE_SETUP.md](AZURE_SETUP.md) for step-by-step instructions.

## Deploy

```bash
# Create resource group
az group create --name budgetalert-rg --location spaincentral

# Get your principal ID for Service Bus role assignment
PRINCIPAL_ID=$(az ad signed-in-user show --query id -o tsv)

# Deploy - outputs connection details when done
az deployment group create \
  --resource-group budgetalert-rg \
  --template-file infra/main.bicep \
  --parameters infra/main.bicepparam \
  --parameters sqlAdminPassword=<your-sql-password> \
  --parameters developerIp=<your-public-ip> \
  --parameters principalId=$PRINCIPAL_ID
```

Copy the output values:

- `sqlConnectionString` - SQL admin connection (initial setup only)
- `serviceBusConnectionString` - shared key connection (emulator path only)

For passwordless access, use the connection strings from `appsettings.AzureServiceBus.example.json` instead.

## Post-deployment SQL step (passwordless access)

Bicep cannot create an Entra ID SQL user - it requires an active DB connection. Run once in Azure Data Studio or the portal Query editor, connected as the Entra ID admin:

```sql
CREATE USER [your-email@domain.com] FROM EXTERNAL PROVIDER;
ALTER ROLE db_owner ADD MEMBER [your-email@domain.com];
```

After this step, use the passwordless connection string in your appsettings:

```
Server=<name>.database.windows.net; Database=BudgetAlert; Authentication=Active Directory Default; TrustServerCertificate=False; Encrypt=True;
```

## Production config (passwordless, no secrets)

After deploying and completing the SQL step, configure the application with no passwords or keys:

| Config | Value |
|--------|-------|
| `ConnectionStrings:Default` | passwordless SQL string above |
| `Messaging:Provider` | `AzureServiceBus` |
| `Messaging:AzureServiceBus:FullyQualifiedNamespace` | `<name>.servicebus.windows.net` |
| `Auth:Enabled` | `true` |
| `Auth:AzureAd:TenantId` | from app registration |
| `Auth:AzureAd:ClientId` | from app registration |
| `Auth:AzureAd:Audience` | `api://<your-client-id>` |

`az login` is sufficient when running locally; Managed Identity takes over in Azure-hosted deployments.

## Cost

Azure SQL serverless autopause kicks in after 1 hour idle - cost is per compute-second when active. Service Bus Basic is ~$0.05/month for low message volumes. Tear down the resource group when done to stop all billing.

## Tear down

```bash
az group delete --name budgetalert-rg --yes
```
