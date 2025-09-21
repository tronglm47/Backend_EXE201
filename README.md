# VLiving API - Backend

## 📋 **Project Overview**

VLiving API là backend service cho ứng dụng VLiving, cung cấp các API endpoints cho authentication, user management và các tính năng khác.

## 🏗️ **Architecture**

```
VLivingAPI (Web API Layer)
    ↓
Services (Business Logic Layer)
    ↓
Repositories (Data Access Layer)
    ↓
Database (SQL Server)
```

## 🚀 **Tech Stack**

- **.NET 8.0** - Framework
- **ASP.NET Core Web API** - Web framework
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **JWT Bearer** - Authentication
- **Swagger/OpenAPI** - API Documentation

## ⚙️ **Setup Instructions**

### 1. **Clone Repository**
```bash
git clone https://github.com/tronglm47/Backend_EXE201.git
cd Backend_EXE201
```

### 2. **Configuration Setup**
```bash
# Copy example config file
cp VLivingAPI/appsettings.Example.json VLivingAPI/appsettings.json

# Edit appsettings.json với thông tin của bạn:
# - Database connection string
# - JWT secret key
```

### 3. **Database Setup**
```bash
# Update database với Entity Framework migrations
dotnet ef database update --project Repositories --startup-project VLivingAPI
```

### 4. **Install Dependencies**
```bash
dotnet restore
```

### 5. **Build & Run**
```bash
dotnet build
dotnet run --project VLivingAPI
```

## 📡 **API Endpoints**

### **Authentication**
- `POST /api/auth/register` - Đăng ký user mới
- `POST /api/auth/login` - Đăng nhập
- `GET /api/auth/userinfo` - Lấy thông tin user (requires auth)
- `GET /api/auth/test` - Test authentication

### **Example Requests**

#### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com",
  "password": "password123",
  "fullName": "Test User",
  "phoneNumber": "0123456789",
  "profilePictureUrl": "",
  "bio": "",
  "role": "User"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "testuser",
  "password": "password123"
}
```

## 🔧 **Configuration**

### **appsettings.json Structure**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_DATABASE_CONNECTION_STRING"
  },
  "Jwt": {
    "Key": "YOUR_JWT_SECRET_KEY",
    "Issuer": "https://localhost:7027",
    "Audience": "https://localhost:7027"
  }
}
```

## 🗄️ **Database Schema**

### **Users Table**
- UserId (int, PK)
- Username (string, unique)
- Email (string, unique)
- Password (string, plain text)
- Role (string)
- FullName (string, nullable)
- PhoneNumber (string, nullable)
- ProfilePictureUrl (string, nullable)
- Bio (string, nullable)
- CreatedAt (datetime)

## 🔐 **Security Features**

- **JWT Authentication** - Secure token-based auth
- **Input Validation** - Data annotations validation
- **Error Handling** - Generic error messages để không leak info
- **Security Logging** - Track login attempts và failures
- **Password Storage** - Plain text (simple approach cho development)

## 📝 **Development Notes**

- **Simple Authentication** - Password stored as plain text cho easy development
- **Clean Architecture** - Separated layers cho maintainability
- **DTOs** - Request/Response objects trong Services layer
- **Logging** - Structured logging với ILogger
- **Swagger** - Auto-generated API documentation

## 🚧 **Development Workflow**

1. **Make changes** trong code
2. **Build** với `dotnet build`
3. **Test** với Swagger UI tại `https://localhost:7027/swagger`
4. **Commit** changes
5. **Push** lên GitHub

## 📚 **Documentation**

- **Swagger UI**: `https://localhost:7027/swagger` (khi chạy local)
- **API Docs**: Xem files `*_API_DOCS.md` trong project
- **Security Guide**: Xem `SECURITY_IMPROVEMENTS.md`

## 🤝 **Contributing**

1. **Create branch** từ `main`
2. **Make changes**
3. **Test thoroughly**
4. **Create Pull Request**
5. **Code Review** before merge

## 📞 **Support**

Nếu có issues hoặc questions, tạo GitHub issue hoặc contact team.

---

**Happy Coding! 🚀**