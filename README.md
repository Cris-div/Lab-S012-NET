# Laboratorio S12 Veterinaria API

API REST para registrar propietarios y mascotas de una veterinaria. Proyecto independiente construido con ASP.NET Core .NET 10, Entity Framework Core y PostgreSQL. El repositorio de [Edwin Arévalo](https://github.com/EdwinArevalo/products-api/tree/main) se consultó únicamente como guía de estructura y uso de controladores con DbContext; no se clonó ni se modificó.

## Estado

Implementación local terminada y validada: 11 operaciones, Swagger público, SQL con datos, Scaffold real desde PostgreSQL, validaciones, CORS y pruebas HTTP. Las instrucciones de despliegue están en [docs/DESPLIEGUE.md](docs/DESPLIEGUE.md).

**Pendiente para la entrega final:** crear las cuentas gratuitas, subir este proyecto a un repositorio propio, publicar API y base de datos, ejecutar las pruebas sobre esa URL y capturar sus resultados. Los resultados locales no reemplazan las evidencias de la nube.

## Ejecutar en Windows

Requisitos: SDK .NET 10, Docker Desktop iniciado con contenedores Linux y Python 3 para las pruebas.

Desde esta carpeta, en PowerShell:

```powershell
./scripts/Start-Local.ps1
```

El script crea un PostgreSQL 17 local en el puerto 55432, ejecuta `database/Veterinaria.sql` al crear el contenedor y arranca la API. Conserva la contraseña local en `.local/connection.txt`, excluida de Git. No imprime la cadena de conexión.

- Swagger: https://localhost:7162/swagger
- HTTP: http://localhost:5162/swagger (el perfil de desarrollo redirige a HTTPS)
- Para confiar en el certificado de desarrollo, si hace falta: `dotnet dev-certs https --trust`.
- Detener API: Ctrl+C. Detener BD conservando sus datos: `docker stop labnet-s12-postgres`.
- El SQL se ejecuta **solo al crear una base vacía**. Reiniciar el contenedor no vuelve a insertar los ejemplos.

## Entidades

| Entidad | Campos | Relación |
| --- | --- | --- |
| Propietario | Id, Nombre, Documento, Telefono | Tiene muchas mascotas |
| Mascota | Id, Nombre, Especie, Edad, Peso, PropietarioId | Pertenece a un propietario |

`Documento` representa un DNI de ocho dígitos. La FK es obligatoria en la base; la navegación `Propietario?` permite crear una mascota enviando solo `propietarioId`. `[JsonIgnore]` en la colección evita ciclos y la inserción accidental de un grafo de mascotas al registrar al propietario.

Reglas con HTTP 409:

1. No se permite repetir el documento de un propietario, en POST ni PUT.
2. No se permite eliminar un propietario con mascotas. Primero deben eliminarse o reasignarse.

La base refuerza ambas reglas mediante una restricción UNIQUE y una FK con `ON DELETE RESTRICT`. Los controladores también contemplan conflictos de concurrencia de estas restricciones.

## Endpoints

| Método | Ruta | Resultado correcto |
| --- | --- | --- |
| GET | /api/propietarios | 200 |
| GET | /api/propietarios/{id} | 200 |
| GET | /api/propietarios/{id}/mascotas | 200 |
| POST | /api/propietarios | 201 y Location |
| PUT | /api/propietarios/{id} | 204 |
| DELETE | /api/propietarios/{id} | 204 |
| GET | /api/mascotas?propietarioId=1&buscar=Luna&especie=Perro | 200 |
| GET | /api/mascotas/{id} | 200 |
| POST | /api/mascotas | 201 y Location |
| PUT | /api/mascotas/{id} | 204 |
| DELETE | /api/mascotas/{id} | 204 |

Los filtros son opcionales y se combinan con AND. `buscar` utiliza `Contains` y distingue mayúsculas de acuerdo con PostgreSQL. Una búsqueda sin coincidencias devuelve `200 []`; un recurso individual inexistente devuelve 404. Los cuerpos inválidos, identificadores distintos entre ruta y cuerpo y FK inexistentes devuelven 400. Todos los accesos de los controladores son asíncronos con EF Core y LINQ, sin DTOs, repositorios ni SQL de acceso a datos en los controladores.

Ejemplo de POST propietario:

```json
{"nombre":"María Pérez","documento":"76543210","telefono":"987123456"}
```

Ejemplo de POST mascota, usando el Id recibido al crear al propietario:

```json
{"nombre":"Nala","especie":"Gato","edad":2,"peso":3.8,"propietarioId":1}
```

Los PUT incluyen `id` y todos los campos escalares. Los POST omiten `id` o envían cero.

## Base de datos primero

El SQL se ejecutó en PostgreSQL y se generaron realmente el contexto y las entidades mediante:

```powershell
dotnet tool restore
dotnet ef dbcontext scaffold 'Name=ConnectionStrings:Veterinaria' Npgsql.EntityFrameworkCore.PostgreSQL --project Veterinaria.Api --output-dir Models --context VeterinariaContext --namespace Veterinaria.Api.Models --no-onconfiguring --no-pluralize --force
```

La configuración se suministró mediante `ConnectionStrings__Veterinaria`, no como texto en el código. Después de generar se renombraron las clases `Mascotas` → `Mascota` y `Propietarios` → `Propietario`, conservando las tablas plurales con `ToTable`. Se añadieron las Data Annotations, `JsonIgnore` y la navegación nullable. No se utilizan migraciones ni `EnsureCreated`.

Para regenerar sin sobrescribir esos ajustes:

```powershell
./scripts/Start-Local.ps1 -DatabaseOnly
./scripts/Scaffold.ps1
```

El resultado de esa regeneración va a `.local/scaffold`. Revisar diferencias y volver a aplicar los ajustes anteriores antes de incorporarlo al proyecto.

## Pruebas

Se incluye un cliente automatizado que comprueba todos los endpoints, los códigos 200/201/204/400/404/409, Location, persistencia de actualizaciones, filtros, navegación, reglas de negocio y CORS. Crea registros temporales y los elimina al finalizar. Ejecutarlo solo sobre una base de laboratorio.

Para HTTP local sin redirección HTTPS, abrir una terminal:

```powershell
$env:ConnectionStrings__Veterinaria = (Get-Content .local/connection.txt -Raw).Trim()
$env:ASPNETCORE_ENVIRONMENT = 'Production'
dotnet run --project Veterinaria.Api --no-launch-profile --urls http://localhost:5163
```

En otra terminal:

```powershell
python tests/test_api.py http://localhost:5163 --allow-local --output artifacts/pruebas-locales
```

Sobre la API publicada:

```powershell
python tests/test_api.py https://TU-SERVICIO.onrender.com --output artifacts/pruebas-nube
```

Si cambias el origen permitido, añade `--origin https://tu-frontend.example`. El cliente espera hasta 180 segundos por petición, para admitir el arranque de servicios gratuitos. Genera un JSON y un HTML con URL, fecha, solicitud, respuesta y código de cada prueba. El HTML facilita revisar resultados; para la entrega todavía hay que obtener capturas reales de las pruebas en nube. También se incluye `Veterinaria.Api/Veterinaria.Api.http` para pruebas manuales.

## Configuración y seguridad

| Variable de entorno | Uso |
| --- | --- |
| ConnectionStrings__Veterinaria | Cadena Npgsql; obligatoria |
| Cors__AllowedOrigin | Origen exacto permitido; por defecto http://localhost:3000 |
| ASPNETCORE_ENVIRONMENT | Production en Render |

No se incluyen credenciales ni cadenas reales en archivos versionables. `.local`, `.env*`, `artifacts`, `bin` y `obj` están excluidos. Swagger y OpenAPI permanecen habilitados en Production como exige el laboratorio. Render ofrece HTTPS en su proxy; dentro del contenedor la API escucha HTTP en 8080. No hay autenticación, siguiendo el alcance del laboratorio; usar datos ficticios.

El informe y los espacios para completar los enlaces de entrega se encuentran en [docs/INFORME.md](docs/INFORME.md).
