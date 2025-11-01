# 🚀 Hướng dẫn Deploy Backend API

## 📋 Các phương án Deploy

### **Phương án 1: IIS (Windows Server)** ⭐ Khuyến nghị cho Windows

#### **Yêu cầu:**
- Windows Server hoặc Windows có IIS
- .NET 9.0 Runtime
- SQL Server đã cấu hình

#### **Các bước:**

1. **Publish ứng dụng:**
```bash
dotnet publish -c Release -o ./publish
```

2. **Cài đặt .NET Hosting Bundle:**
- Download: https://dotnet.microsoft.com/download/dotnet/9.0
- Cài đặt: `dotnet-hosting-9.0.x-win.exe`

3. **Tạo Site trong IIS:**
- Mở IIS Manager
- Right-click Sites → Add Website
- Site name: `RoomForLeaseAPI`
- Physical path: `C:\inetpub\wwwroot\room-for-lease-api\publish`
- Binding: Port 80 hoặc 443

4. **Cấu hình Application Pool:**
- .NET CLR Version: No Managed Code
- Managed Pipeline Mode: Integrated
- Identity: ApplicationPoolIdentity

5. **Cập nhật appsettings.Production.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=RoomForLeaseDb;..."
  },
  "Cors": {
    "AllowedOrigins": ["https://your-fe-domain.com"]
  }
}
```

---

### **Phương án 2: Kestrel + Reverse Proxy (Nginx/Apache)** 🔥 Cho Linux/Windows

#### **Yêu cầu:**
- Linux/Windows Server
- Nginx hoặc Apache
- .NET 9.0 Runtime

#### **Các bước:**

1. **Publish ứng dụng:**
```bash
dotnet publish -c Release -o ./publish
```

2. **Tạo systemd service (Linux):**
```bash
sudo nano /etc/systemd/system/room-for-lease-api.service
```

Nội dung:
```ini
[Unit]
Description=Room For Lease API
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /var/www/room-for-lease-api/room_for_lease_api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=room-for-lease-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

3. **Cấu hình Nginx:**
```nginx
server {
    listen 80;
    server_name api.yourdomain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

4. **Khởi động service:**
```bash
sudo systemctl enable room-for-lease-api
sudo systemctl start room-for-lease-api
sudo systemctl restart nginx
```

---

### **Phương án 3: Docker** 🐳 Khuyến nghị cho Production

#### **Tạo Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["room_for_lease_api.csproj", "./"]
RUN dotnet restore "room_for_lease_api.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "room_for_lease_api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "room_for_lease_api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "room_for_lease_api.dll"]
```

#### **Tạo docker-compose.yml:**
```yaml
version: '3.8'

services:
  api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=RoomForLeaseDb;User Id=sa;Password=YourPassword123!
    depends_on:
      - db
    restart: always

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - sql_data:/var/opt/mssql
    restart: always

volumes:
  sql_data:
```

#### **Deploy:**
```bash
docker-compose up -d
```

---

### **Phương án 4: Cloud Platforms** ☁️

#### **Azure App Service:**
```bash
# Login Azure
az login

# Create App Service
az webapp create --resource-group MyResourceGroup --plan MyPlan --name room-for-lease-api

# Deploy
dotnet publish -c Release
az webapp deploy --resource-group MyResourceGroup --name room-for-lease-api --src-path ./publish
```

#### **AWS Elastic Beanstalk:**
```bash
# Install EB CLI
pip install awsebcli

# Initialize
eb init -p docker room-for-lease-api
eb create room-for-lease-api-env
eb deploy
```

---

## ⚙️ Cấu hình Production

### **1. Tạo appsettings.Production.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=RoomForLeaseDb;User Id=sa;Password=YourSecurePassword;TrustServerCertificate=true"
  },
  "Jwt": {
    "Key": "YOUR_VERY_LONG_SECURE_RANDOM_KEY_HERE_AT_LEAST_32_CHARACTERS",
    "Issuer": "room-for-lease-api",
    "Audience": "room-for-lease-fe",
    "ExpiryMinutes": 120
  },
  "Cors": {
    "AllowedOrigins": [
      "https://your-frontend-domain.com",
      "https://www.your-frontend-domain.com"
    ]
  }
}
```

### **2. Cấu hình CORS cho Production:**
```csharp
// Program.cs - Đã có sẵn
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

### **3. Bảo mật:**
- ✅ Đổi JWT Key thành random key mạnh
- ✅ Sử dụng HTTPS
- ✅ Cấu hình firewall
- ✅ Giới hạn CORS origins
- ✅ Validate input đầy đủ

---

## 📝 Checklist trước khi Deploy

- [ ] Update `appsettings.Production.json` với connection string đúng
- [ ] Đổi JWT Key thành key mạnh
- [ ] Cấu hình CORS origins cho FE domain
- [ ] Chạy migrations: `dotnet ef database update`
- [ ] Test API trên production environment
- [ ] Cấu hình HTTPS/SSL
- [ ] Setup logging
- [ ] Backup database

---

## 🔗 URLs sau khi Deploy

- **API Base URL:** `http://your-server-ip:5000` hoặc `https://api.yourdomain.com`
- **Swagger:** `https://api.yourdomain.com/swagger`
- **Health Check:** `https://api.yourdomain.com/health` (nếu có)

---

## 🐛 Troubleshooting

### **Lỗi 500 Internal Server Error:**
- Kiểm tra logs: `dotnet logs` hoặc Windows Event Viewer
- Kiểm tra connection string
- Kiểm tra migrations đã chạy chưa

### **Lỗi CORS:**
- Kiểm tra `AllowedOrigins` trong appsettings
- Kiểm tra CORS middleware đã được add chưa

### **Lỗi Database Connection:**
- Kiểm tra SQL Server đang chạy
- Kiểm tra connection string
- Kiểm tra firewall rules

---

## 📞 Hỗ trợ

Nếu gặp vấn đề, kiểm tra:
1. Logs của application
2. SQL Server logs
3. IIS/Nginx logs
4. Windows Event Viewer

