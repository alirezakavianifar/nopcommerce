# Running nopCommerce Source

This guide explains how to build and run the nopCommerce application from source.

## Prerequisites
- **.NET 9.0 SDK**: Required version `9.0.100` as specified in `global.json`.
- **Database**: MS SQL Server (2012+), PostgreSQL, or MySQL.
- **Tools**: PowerShell (for the runner script) or the .NET CLI.

---

## 🚀 How to Run

### Method 1: Using the Developer Runner (Recommended)
We include a PowerShell script that handles the build process and **automatically restarts** the app (useful for plugin installations).

1. Open a PowerShell terminal in the project root.
2. Run the script:
   ```powershell
   ./run.ps1
   ```
3. The app will be available at:
   - `http://localhost:5000`
   - `https://localhost:5001`

### Method 2: Using the .NET CLI
Run the web presentation project directly:

```powershell
dotnet run --project src\Presentation\Nop.Web\Nop.Web.csproj
```

### Method 3: Using Docker
If you have Docker installed, you can spin up the app and a SQL Server instance:

```powershell
docker-compose up -d
```
*The app will listen on port 80 (http://localhost:80).*

---

## 🛠 First-Time Setup
When you first run the app, you will see the **Installation Page**:

1. **Admin Account**: Enter an email and password for your admin user.
2. **Database**: 
   - Choose your database engine.
   - Enter your connection string or server details.
   - Click **Install**.
3. **Wait**: The installation process takes a few moments to create tables and seed data.

---

## 📚 Related Documentation
- [README.md](README.md) - General overview and marketing features.
- [TESTING_GUIDE.md](TESTING_GUIDE.md) - Detailed guide for testing custom plugins (Group Purchase, etc).
- [PHASES_3_6_TESTING.md](PHASES_3_6_TESTING.md) - Technical implementation and verification guide.
