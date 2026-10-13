using 'main.bicep'

param location = 'eastus'
param sqlAdminLogin = 'budgetalert-admin'
param sqlAdminPassword = '' // passed at deploy time via --parameters sqlAdminPassword=<your-password>
param developerIp = '' // passed at deploy time via --parameters developerIp=<your-ip>
