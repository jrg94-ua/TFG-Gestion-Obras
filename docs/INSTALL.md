# Guía de Instalación de .NET SDK

## Descargar e Instalar .NET 8.0

### Para Windows

1. Visita la página oficial de descargas:
   ```
   https://dotnet.microsoft.com/download/dotnet/8.0
   ```

2. Descarga el instalador de **.NET SDK 8.0** (no solo el Runtime)

3. Ejecuta el instalador y sigue las instrucciones

4. Reinicia Visual Studio Code

5. Verifica la instalación abriendo PowerShell:
   ```powershell
   dotnet --version
   ```

   Deberías ver algo como: `8.0.x`

---

## Crear la Estructura del Proyecto ASP.NET Core

Una vez instalado .NET, ejecuta estos comandos en PowerShell desde la carpeta raíz del proyecto:

```powershell
# Navegar a la carpeta src
cd c:\Users\jorge\Desktop\TFG-JORGE\src

# Crear solución principal
dotnet new sln -n GestionObras

# Crear proyecto Web API
dotnet new webapi -n GestionObras.API -o GestionObras.API

# Crear proyecto Core (Dominio)
dotnet new classlib -n GestionObras.Core -o GestionObras.Core

# Crear proyecto Infrastructure (Acceso a datos)
dotnet new classlib -n GestionObras.Infrastructure -o GestionObras.Infrastructure

# Crear proyecto Blazor (Frontend)
dotnet new blazorserver -n GestionObras.Web -o GestionObras.Web

# Agregar proyectos a la solución
dotnet sln add GestionObras.API/GestionObras.API.csproj
dotnet sln add GestionObras.Core/GestionObras.Core.csproj
dotnet sln add GestionObras.Infrastructure/GestionObras.Infrastructure.csproj
dotnet sln add GestionObras.Web/GestionObras.Web.csproj

# Crear referencias entre proyectos (arquitectura en capas)
dotnet add GestionObras.API/GestionObras.API.csproj reference GestionObras.Core/GestionObras.Core.csproj
dotnet add GestionObras.API/GestionObras.API.csproj reference GestionObras.Infrastructure/GestionObras.Infrastructure.csproj
dotnet add GestionObras.Infrastructure/GestionObras.Infrastructure.csproj reference GestionObras.Core/GestionObras.Core.csproj
dotnet add GestionObras.Web/GestionObras.Web.csproj reference GestionObras.Core/GestionObras.Core.csproj
```

---

## Instalar Paquetes NuGet Necesarios

### Para el proyecto Core (sin dependencias externas por Clean Architecture)
```powershell
cd GestionObras.Core
# No requiere paquetes externos
cd ..
```

### Para el proyecto Infrastructure
```powershell
cd GestionObras.Infrastructure

# Entity Framework Core para SQL Server
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# Entity Framework Core Tools (para migraciones)
dotnet add package Microsoft.EntityFrameworkCore.Tools

# Entity Framework Core Design
dotnet add package Microsoft.EntityFrameworkCore.Design

cd ..
```

### Para el proyecto API
```powershell
cd GestionObras.API

# ASP.NET Core Identity para autenticación
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore

# JWT para tokens
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

# Swagger para documentación API
dotnet add package Swashbuckle.AspNetCore

cd ..
```

### Para el proyecto Web (Blazor)
```powershell
cd GestionObras.Web

# Bootstrap Icons
dotnet add package BlazorBootstrap

cd ..
```

---

## Estructura Final del Proyecto

Después de ejecutar los comandos, tu estructura debería verse así:

```
src/
├── GestionObras.sln
├── GestionObras.API/           (Web API REST)
│   ├── Controllers/
│   ├── Program.cs
│   └── appsettings.json
├── GestionObras.Core/          (Dominio - sin dependencias)
│   ├── Entities/
│   ├── Interfaces/
│   └── Services/
├── GestionObras.Infrastructure/ (Acceso a datos y servicios externos)
│   ├── Data/
│   ├── Repositories/
│   └── Services/
└── GestionObras.Web/           (Frontend Blazor)
    ├── Pages/
    ├── Shared/
    └── Program.cs
```

---

## Ejecutar el Proyecto

```powershell
# Ejecutar la API
cd GestionObras.API
dotnet run

# En otra terminal, ejecutar el frontend Blazor
cd GestionObras.Web
dotnet run
```

---

## Próximos Pasos

1. Configurar la cadena de conexión a SQL Server en `appsettings.json`
2. Crear las entidades de dominio en `GestionObras.Core`
3. Configurar el DbContext en `GestionObras.Infrastructure`
4. Crear las primeras migraciones de Entity Framework
5. Implementar los controladores básicos en `GestionObras.API`

---

**Nota**: Si no tienes instalado SQL Server, puedes usar **SQL Server Express** (gratis) o **SQLite** para desarrollo.
