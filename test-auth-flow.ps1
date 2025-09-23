# VLivingAPI Authentication Test Script
# Test authentication flow trên https://localhost:7027

$baseUrl = "https://localhost:7027"

Write-Host "=== VLivingAPI Authentication Testing ===" -ForegroundColor Green
Write-Host "Base URL: $baseUrl" -ForegroundColor Cyan
Write-Host ""

# Tắt SSL verification cho localhost testing
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

# Test 1: Register a test user
Write-Host "1. Registering Test User..." -ForegroundColor Yellow
$registerData = @{
    Username = "testuser$(Get-Random -Maximum 9999)"
    Email = "test$(Get-Random -Maximum 9999)@example.com"
    Password = "TestPassword123!"
    FullName = "Test User"
    PhoneNumber = "1234567890"
    Bio = "Test user for API testing"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/auth/register" -Method POST -Body $registerData -ContentType "application/json" -SkipCertificateCheck
    Write-Host "✅ Register Status: $($response.StatusCode)" -ForegroundColor Green
    Write-Host "Register Response: $($response.Content)" -ForegroundColor Cyan
    
    # Parse register response để lấy username
    $registerResult = $response.Content | ConvertFrom-Json
    $username = ($registerData | ConvertFrom-Json).Username
    
} catch {
    Write-Host "❌ Register Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
    exit
}
Write-Host ""

# Test 2: Login to get token
Write-Host "2. Login to Get Token..." -ForegroundColor Yellow
$loginData = @{
    Username = $username
    Password = "TestPassword123!"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginData -ContentType "application/json" -SkipCertificateCheck
    Write-Host "✅ Login Status: $($response.StatusCode)" -ForegroundColor Green
    $loginResponse = $response.Content | ConvertFrom-Json
    $token = $loginResponse.token
    Write-Host "Token received: $($token.Substring(0, 50))..." -ForegroundColor Cyan
    
} catch {
    Write-Host "❌ Login Error: $($_.Exception.Message)" -ForegroundColor Red
    exit
}
Write-Host ""

# Test 3: Test Auth endpoint with token
Write-Host "3. Testing Auth Endpoint (with token)..." -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $token"
    }
    
    $authResponse = Invoke-WebRequest -Uri "$baseUrl/api/test/auth" -Method GET -Headers $headers -SkipCertificateCheck
    Write-Host "✅ Auth Test Status: $($authResponse.StatusCode)" -ForegroundColor Green
    Write-Host "Auth Test Response: $($authResponse.Content)" -ForegroundColor Cyan
    
} catch {
    Write-Host "❌ Auth Test Error: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 4: Test Auth endpoint without token (should fail)
Write-Host "4. Testing Auth Endpoint (without token - should fail)..." -ForegroundColor Yellow
try {
    $noAuthResponse = Invoke-WebRequest -Uri "$baseUrl/api/test/auth" -Method GET -SkipCertificateCheck
    Write-Host "⚠️ Unexpected Success: $($noAuthResponse.StatusCode)" -ForegroundColor DarkYellow
} catch {
    Write-Host "✅ Expected Error (401 Unauthorized): $($_.Exception.Message)" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Testing Complete ===" -ForegroundColor Green