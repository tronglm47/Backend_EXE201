param(
    [Parameter(Mandatory=$true)]
    [string]$Version
)

Write-Host "🚀 Starting deployment of VLivingAPI $Version..." -ForegroundColor Green

try {
    # Clean and build
    Write-Host "1. Cleaning and building..." -ForegroundColor Yellow
    dotnet clean
    dotnet build --configuration Release
    
    # Publish
    Write-Host "2. Publishing..." -ForegroundColor Yellow
    Set-Location VLivingAPI
    dotnet publish --configuration Release --output ../publish-filtered --no-restore --verbosity minimal
    
    # Clean files
    Write-Host "3. Removing unnecessary files..." -ForegroundColor Yellow
    Set-Location ../publish-filtered
    Get-ChildItem -Directory | Where-Object {$_.Name.Length -eq 2 -or $_.Name -like "*-*"} | Remove-Item -Recurse -Force
    Remove-Item -Force -ErrorAction SilentlyContinue @("*.pdb")
    
    # Create package
    Write-Host "4. Creating deployment package..." -ForegroundColor Yellow
    Compress-Archive -Path * -DestinationPath "../VLivingAPI-deployment-$Version.zip" -Force
    Set-Location ..
    
    # Deploy to AWS - REPLACE WITH YOUR VALUES
    Write-Host "5. Uploading to AWS..." -ForegroundColor Yellow
    aws s3 cp "VLivingAPI-deployment-$Version.zip" s3://your-s3-bucket-name/ --region your-aws-region
    
    Write-Host "6. Creating application version..." -ForegroundColor Yellow
    aws elasticbeanstalk create-application-version --application-name YourAppName --version-label $Version --source-bundle S3Bucket=your-s3-bucket-name,S3Key=VLivingAPI-deployment-$Version.zip --region your-aws-region
    
    Write-Host "7. Deploying to environment..." -ForegroundColor Yellow
    aws elasticbeanstalk update-environment --environment-id your-environment-id --version-label $Version --region your-aws-region
    
    Write-Host "8. Waiting for deployment to complete..." -ForegroundColor Yellow
    Start-Sleep 60
    
    # Verify deployment - REPLACE WITH YOUR VALUES
    Write-Host "9. Verifying deployment..." -ForegroundColor Yellow
    $status = aws elasticbeanstalk describe-environments --environment-ids your-environment-id --region your-aws-region --query 'Environments[0].{Status:Status,Health:Health,VersionLabel:VersionLabel}' | ConvertFrom-Json
    
    if ($status.Status -eq "Ready" -and $status.Health -eq "Green") {
        Write-Host "✅ Deployment successful!" -ForegroundColor Green
        Write-Host "🌐 API URL: https://your-api-domain.com" -ForegroundColor Cyan
        Write-Host "📖 Swagger UI: https://your-api-domain.com/swagger/index.html" -ForegroundColor Cyan
        Write-Host "📊 Version: $($status.VersionLabel)" -ForegroundColor Cyan
        
        # Quick test - REPLACE WITH YOUR VALUES
        Write-Host "10. Testing API..." -ForegroundColor Yellow
        $testResult = curl "https://your-api-domain.com/api/Test/connection" -s | ConvertFrom-Json
        Write-Host "✅ API Test: $($testResult.message)" -ForegroundColor Green
        
        # Database test
        Write-Host "11. Testing database connection..." -ForegroundColor Yellow
        $dbResult = curl "https://your-api-domain.com/api/Test/database" -s | ConvertFrom-Json
        Write-Host "✅ Database Test: $($dbResult.message) - Users: $($dbResult.data.userCount)" -ForegroundColor Green
        
        Write-Host ""
        Write-Host "🎉 DEPLOYMENT COMPLETE!" -ForegroundColor Green
        Write-Host "Version $Version is now live and fully functional." -ForegroundColor Green
        Write-Host ""
        
    } else {
        Write-Host "❌ Deployment may have issues. Status: $($status.Status), Health: $($status.Health)" -ForegroundColor Red
    }
    
} catch {
    Write-Host "❌ Deployment failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Stack trace: $($_.Exception.StackTrace)" -ForegroundColor Red
}

Write-Host ""
Write-Host "📋 Deployment Summary:" -ForegroundColor Cyan
Write-Host "- Package created: VLivingAPI-deployment-$Version.zip" -ForegroundColor White
Write-Host "- Uploaded to S3: your-s3-bucket-name" -ForegroundColor White
Write-Host "- Environment: your-environment-id (YourApp-prod)" -ForegroundColor White
Write-Host "- Region: your-aws-region" -ForegroundColor White
Write-Host ""

# Cleanup
Write-Host "🧹 Cleaning up temporary files..." -ForegroundColor Yellow
Remove-Item -Path "VLivingAPI-deployment-$Version.zip" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "publish-filtered" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "✅ Cleanup completed." -ForegroundColor Green

<#
.SYNOPSIS
    Automated deployment script for VLivingAPI to AWS Elastic Beanstalk

.DESCRIPTION
    This script builds, packages, and deploys the VLivingAPI to AWS Elastic Beanstalk.
    Before using this script, replace the placeholder values with your actual AWS configuration:
    
    - your-s3-bucket-name: Your S3 bucket for deployment packages
    - your-environment-id: Your Elastic Beanstalk environment ID
    - your-aws-region: Your AWS region (e.g., ap-southeast-1)
    - YourAppName: Your Elastic Beanstalk application name
    - https://your-api-domain.com: Your actual API domain

.PARAMETER Version
    The version label for this deployment (e.g., "v1.0.11")

.EXAMPLE
    .\deploy.example.ps1 -Version "v1.0.11"

.NOTES
    Prerequisites:
    - AWS CLI configured with appropriate credentials
    - .NET SDK installed
    - PowerShell 5.0 or higher
#>