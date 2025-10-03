# AWS Environment Variables Setup Guide

This document explains how to set up environment variables for VLiving API deployment on AWS.

## Required Environment Variables

### Database Configuration
```bash
ConnectionStrings__DefaultConnection="Server=tcp:vlivingdb.database.windows.net,1433;Initial Catalog=VLivingDb;Persist Security Info=False;User ID=VLiving@vlivingdb;Password=Sixma123@;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### JWT Configuration
```bash
JWT__Key="YourProductionSecretKeyWithMinimum32Characters"
JWT__Issuer="VLivingAPI"
JWT__Audience="VLivingApp"
JWT__ExpiryInMinutes="60"
```

### Email Configuration
```bash
EmailSettings__SmtpServer="smtp.gmail.com"
EmailSettings__SmtpPort="587"
EmailSettings__Username="vliving2025@gmail.com"
EmailSettings__Password="musjatcoslntnrzu"
EmailSettings__FromEmail="vliving2025@gmail.com"
EmailSettings__FromName="VLiving Team"
EmailSettings__EnableSsl="true"
EmailSettings__BaseUrl="https://your-production-domain.com"
EmailSettings__DeleteTokenAfterUse="true"
```

### Google Cloud Storage
```bash
GoogleCloudStorage__BucketName="vliving-storage-image"
GoogleCloudStorage__ProjectId="valiant-index-438307-p0"
GoogleCloudStorage__UseEnvironmentCredentials="false"
GoogleCloudStorage__CredentialPath="./valiant-index-438307-p0-65335d65d5f5.json"
```

## AWS Systems Manager Parameter Store Setup

For production deployment, store sensitive values in AWS Systems Manager Parameter Store:

```bash
# Create parameters in AWS SSM
aws ssm put-parameter --name "/vliving/database-connection" --value "YOUR_DATABASE_CONNECTION_STRING" --type "SecureString" --region us-east-1

aws ssm put-parameter --name "/vliving/jwt-key" --value "YOUR_JWT_SECRET_KEY" --type "SecureString" --region us-east-1

aws ssm put-parameter --name "/vliving/email-username" --value "vliving2025@gmail.com" --type "SecureString" --region us-east-1

aws ssm put-parameter --name "/vliving/email-password" --value "musjatcoslntnrzu" --type "SecureString" --region us-east-1
```

## ECS Environment Variables

In your ECS task definition, reference these parameters:

```json
"secrets": [
  {
    "name": "ConnectionStrings__DefaultConnection",
    "valueFrom": "arn:aws:ssm:us-east-1:YOUR_ACCOUNT_ID:parameter/vliving/database-connection"
  },
  {
    "name": "JWT__Key",
    "valueFrom": "arn:aws:ssm:us-east-1:YOUR_ACCOUNT_ID:parameter/vliving/jwt-key"
  }
]
```

## Docker Environment Variables

For local testing with Docker:

```bash
docker run -d \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="YOUR_CONNECTION_STRING" \
  -e JWT__Key="YOUR_JWT_KEY" \
  vliving-api:latest
```

## AWS App Runner Configuration

If using AWS App Runner, set environment variables in the service configuration:

```json
{
  "EnvironmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Production",
    "JWT__Issuer": "VLivingAPI",
    "JWT__Audience": "VLivingApp"
  }
}
```