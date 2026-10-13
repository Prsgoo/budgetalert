@description('Azure region for all resources')
param location string = 'eastus'

@description('Unique suffix appended to resource names to avoid collisions')
param uniqueSuffix string = uniqueString(resourceGroup().id)

@description('SQL Server administrator login')
param sqlAdminLogin string = 'budgetalert-admin'

@description('SQL Server administrator password - passed at deploy time, never committed')
@secure()
param sqlAdminPassword string

@description('Your current developer IP - run: curl -s ifconfig.me')
param developerIp string

@description('Object ID of the identity to grant Service Bus Data Owner - run: az ad signed-in-user show --query id -o tsv')
param principalId string = ''

// Azure SQL Server
resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: 'budgetalert-sql-${uniqueSuffix}'
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
  }
}

// Serverless database - autopauses after 1 hour idle, costs cents per hour when active
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: 'BudgetAlert'
  location: location
  sku: { name: 'GP_S_Gen5', tier: 'GeneralPurpose', family: 'Gen5', capacity: 1 }
  properties: { autoPauseDelay: 60 }
}

// Allow Azure services (0.0.0.0/0.0.0.0 is the Azure-internal magic range)
resource sqlFirewallAllowAzure 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAllAzureIPs'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Allow local dev machine to reach Azure SQL directly
resource sqlFirewallAllowLocalDev 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowLocalDev'
  properties: {
    startIpAddress: developerIp
    endIpAddress: developerIp
  }
}

// Azure Service Bus - Basic tier supports queues only (no topics)
resource serviceBus 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: 'budgetalert-sb-${uniqueSuffix}'
  location: location
  sku: { name: 'Basic', tier: 'Basic' }
}

// Single queue - correct model for one producer, one consumer
resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2022-10-01-preview' = {
  parent: serviceBus
  name: 'budget-events'
}

// Assign Azure Service Bus Data Owner to the provided identity (skipped when principalId is empty)
resource sbRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(principalId)) {
  scope: serviceBus
  name: guid(serviceBus.id, principalId, 'ServiceBusDataOwner')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '090c5cfd-751d-490a-894a-3ce6f1109419')
    principalId: principalId
  }
}

// Outputs - paste directly into appsettings.json
#disable-next-line outputs-should-not-contain-secrets
output sqlConnectionString string = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=BudgetAlert;User Id=${sqlAdminLogin};Password=${sqlAdminPassword};TrustServerCertificate=False;Encrypt=True;'
#disable-next-line outputs-should-not-contain-secrets
output serviceBusConnectionString string = listKeys('${serviceBus.id}/AuthorizationRules/RootManageSharedAccessKey', serviceBus.apiVersion).primaryConnectionString
