# VLivingAPI - Configuration and Deployment Guide

## 🔧 Configuration Setup

### 1. Required Configuration Files

Copy and rename the template files for your environment:

```bash
# For Production
cp appsettings.Template.json appsettings.Production.json

# For Development  
cp appsettings.Template.json appsettings.Development.json

# For Staging
cp appsettings.Template.json appsettings.Staging.json
```

### 2. Configure Connection Strings

Update the `DefaultConnection` in your environment-specific appsettings file:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Initial Catalog=YOUR_DATABASE;Persist Security Info=False;User ID=YOUR_USERNAME;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### 3. Configure JWT Settings

Update the JWT configuration with your secure keys:

```json
{
  "JWT": {
    "Key": "YOUR_SECURE_SECRET_KEY_MINIMUM_32_CHARACTERS_LONG",
    "Issuer": "VLivingAPI-YourEnvironment", 
    "Audience": "VLivingApp-YourEnvironment",
    "ExpiryInMinutes": 60
  }
}
```

## 🚀 AWS Deployment

### Prerequisites
- AWS CLI configured with appropriate permissions
- .NET 8 SDK installed
- Azure SQL Database (or compatible SQL Server)

### Current Deployment Status
- **Environment:** vlivingapi-prod
- **Region:** ap-southeast-1 (Singapore)  
- **URL:** http://vlivingapi-prod.eba-3t3ifafu.ap-southeast-1.elasticbeanstalk.com
- **Swagger:** http://vlivingapi-prod.eba-3t3ifafu.ap-southeast-1.elasticbeanstalk.com/swagger/index.html

### Deployment Commands

```bash
# Build and publish
dotnet publish -c Release -o ../publish

# Create deployment package
Compress-Archive -Path "publish/*" -DestinationPath "deployment-package.zip" -Force

# Upload to S3
aws s3 cp deployment-package.zip s3://vliving-deployment-bucket/ --region ap-southeast-1

# Create application version
aws elasticbeanstalk create-application-version --region ap-southeast-1 --application-name VLivingAPI --version-label v1.x.x --source-bundle S3Bucket=vliving-deployment-bucket,S3Key=deployment-package.zip

# Deploy to environment
aws elasticbeanstalk update-environment --region ap-southeast-1 --application-name VLivingAPI --environment-name vlivingapi-prod --version-label v1.x.x
```

## 🔒 Security Notes

### Files NOT to commit to git:
- `appsettings.Production.json`
- `appsettings.Development.json` 
- `appsettings.Staging.json`
- `eb-config.json`
- `deployment-docs/`
- Any files containing connection strings or API keys

### Files safe to commit:
- `appsettings.Template.json` 
- `appsettings.Example.json`
- Source code files
- Configuration templates

## 🏗️ Architecture

- **Backend:** .NET 8 Web API
- **Database:** Azure SQL Database
- **Hosting:** AWS Elastic Beanstalk
- **Authentication:** JWT Bearer Token
- **Repository Pattern:** Generic Repository + Unit of Work

## 📋 Available Endpoints

### Test Endpoints
- `GET /api/Test/connection` - API connectivity test
- `GET /api/Test/database` - Database connection test  
- `GET /api/Test/health` - Health check with version info
- `GET /health` - Simple health endpoint

### Authentication Endpoints
- `POST /api/Auth/register` - User registration
- `POST /api/Auth/login` - User login
- `GET /api/Auth/userinfo` - Get user info (requires JWT)
- `POST /api/Auth/logout` - Logout
- `POST /api/Auth/refresh` - Refresh JWT token

### Other API Endpoints
See Swagger documentation for complete API reference.

## 🐛 Troubleshooting

### Common Issues

1. **502 Bad Gateway**
   - Check database connection string
   - Verify Azure SQL firewall rules
   - Check application logs

2. **Database Connection Failed**
   - Verify connection string format
   - Check Azure SQL server status
   - Ensure IP whitelisting for Azure SQL

3. **JWT Authentication Failed**
   - Verify JWT key is at least 32 characters
   - Check issuer and audience configuration
   - Ensure consistent configuration across environments

### Logs Access
```bash
# Get environment health
aws elasticbeanstalk describe-environment-health --region ap-southeast-1 --environment-name vlivingapi-prod --attribute-names All

# Request log bundle
aws elasticbeanstalk request-environment-info --region ap-southeast-1 --environment-name vlivingapi-prod --info-type bundle

# Retrieve logs
aws elasticbeanstalk retrieve-environment-info --region ap-southeast-1 --environment-name vlivingapi-prod --info-type bundle
```

## 📞 Support

For deployment issues or configuration questions, refer to the deployment documentation or contact the development team.

---
Last Updated: September 23, 2025  
Current Version: v1.3.0-full