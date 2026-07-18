# HireKarlo Azure Deployment Guide

## Prerequisites
1. Azure account (Free tier available)
2. Azure CLI installed (`winget install Microsoft.AzureCLI`)
3. .NET 9 SDK installed

## Step 1: Create Azure Resources

```powershell
# Login to Azure
az login

# Create Resource Group
az group create --name hirekarlo-rg --location eastus

# Create App Service Plan (Free tier - F1)
az appservice plan create --name hirekarlo-plan --resource-group hirekarlo-rg --sku F1 --is-linux

# Create Web App for API
az webapp create --name hirekarlo-api --resource-group hirekarlo-rg --plan hirekarlo-plan --runtime "DOTNET|9.0"

# Create Web App for Frontend
az webapp create --name hirekarlo-web --resource-group hirekarlo-rg --plan hirekarlo-plan --runtime "DOTNET|9.0"

# Create Azure SQL Database (Basic tier - free for first 12 months)
az sql server create --name hirekarlo-db-server --resource-group hirekarlo-rg --location eastus --admin-user sqladmin --admin-password "YourSecurePassword123!"
az sql db create --name HireKarlo --resource-group hirekarlo-rg --server hirekarlo-db-server --edition Basic --capacity 5
```

## Step 2: Configure App Settings

```powershell
# Set API connection string
az webapp config connection-string set --name hirekarlo-api --resource-group hirekarlo-rg --connection-string-type SQLAzure --settings DefaultConnection="Server=tcp:hirekarlo-db-server.database.windows.net,1433;Database=HireKarlo;User ID=sqladmin;Password=YourSecurePassword123!;Encrypt=True;"

# Set other settings
az webapp config appsettings set --name hirekarlo-api --resource-group hirekarlo-rg --settings Auth__JwtSecret="YourSuperSecretKeyAtLeast32CharactersLong!"
```

## Step 3: Publish and Deploy

```powershell
# Publish API
cd src/Presentation/HireKarlo.Api
dotnet publish -c Release -o ./publish

# Deploy API to Azure
az webapp deploy --resource-group hirekarlo-rg --name hirekarlo-api --src-path ./publish.zip --type zip

# Publish Web
cd ../HireKarlo.Web/HireKarlo.Web
dotnet publish -c Release -o ./publish

# Deploy Web to Azure
az webapp deploy --resource-group hirekarlo-rg --name hirekarlo-web --src-path ./publish.zip --type zip
```

## Step 4: Get Free Domain

### Option A: Use Azure subdomain (Immediate)
- API: https://hirekarlo-api.azurewebsites.net
- Web: https://hirekarlo-web.azurewebsites.net

### Option B: Free Domain from Freenom
1. Go to https://www.freenom.com
2. Register a free .tk, .ml, .ga, .cf, or .gq domain
3. Point DNS to your Azure web app:
   - Create CNAME record: `www` -> `hirekarlo-web.azurewebsites.net`
   - Create A record: `@` -> Azure Web App IP

### Option C: Free Domain from GitHub Student Developer Pack
If you're a student, get free .me domain from Namecheap

## Step 5: Enable SSL (Free with Azure)

```powershell
# SSL is automatically enabled on *.azurewebsites.net domains
# For custom domains:
az webapp config ssl bind --name hirekarlo-web --resource-group hirekarlo-rg --certificate-thumbprint <cert-thumbprint> --ssl-type SNI
```

## URLs After Deployment
- **Website**: https://hirekarlo-web.azurewebsites.net
- **API**: https://hirekarlo-api.azurewebsites.net
- **API Docs**: https://hirekarlo-api.azurewebsites.net/swagger

## Costs (Estimated Monthly)
- App Service Plan (F1 Free): $0
- Azure SQL (Basic): $5/month (or free trial)
- Domain (.tk free or .com ~$12/year)
- **Total**: ~$5/month or less

## Troubleshooting
- Check logs: `az webapp log tail --name hirekarlo-api --resource-group hirekarlo-rg`
- Restart app: `az webapp restart --name hirekarlo-api --resource-group hirekarlo-rg`
