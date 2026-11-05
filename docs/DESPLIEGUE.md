# Publicación en Render y Neon

La API ya está publicada en [Swagger](https://veterinaria-s12-cris.onrender.com/swagger/index.html), con código en [Cris-div/Lab-S012-NET](https://github.com/Cris-div/Lab-S012-NET), Render Free y una base PostgreSQL 17 del proyecto `veterinaria-s12` en Neon Free. Esta guía permite repetir la publicación en servicios propios. No es necesario modificar el repositorio del profesor.

## 1 Crear un repositorio propio

Crear en GitHub un repositorio nuevo, por ejemplo `veterinaria-api-s12`, sin README inicial. Desde la carpeta del laboratorio:

```powershell
git init
git add .
git status
git commit -m "Implementa laboratorio S12 de veterinaria"
git branch -M main
git remote add origin https://github.com/TU-USUARIO/veterinaria-api-s12.git
git push -u origin main
```

Revisar `git status` antes del commit: `.local`, `.env*`, resultados y carpetas de compilación deben estar ausentes. Reemplazar TU-USUARIO por tu cuenta. El remoto debe apuntar a tu repositorio.

## 2 Crear la base PostgreSQL en Neon

1. Registrarse en [Neon](https://console.neon.tech/) y mantener el plan **Free**.
2. Crear un proyecto, seleccionar PostgreSQL 17 y una región cercana a la elegida para Render. Usar la base creada por Neon, normalmente `neondb`.
3. Abrir SQL Editor de esa base y ejecutar todo `database/Veterinaria.sql` una sola vez. El script tiene una transacción; si las tablas ya existen, no lo vuelvas a ejecutar ni borres datos para forzar su ejecución.
4. Comprobar con `SELECT * FROM "Propietarios";` y `SELECT * FROM "Mascotas";`: habrá tres registros en cada tabla.
5. En Connect, obtener host, database, username y password. Elegir la conexión directa, sin pooler, para generar el modelo si necesitas repetir Scaffold. Para la API se puede usar la conexión con pooler.
6. Construir la cadena en formato Npgsql; no pegar una URL `postgresql://` directamente en esta aplicación:

```text
Host=HOST_DE_NEON;Port=5432;Database=BASE_DE_NEON;Username=USUARIO_DE_NEON;Password=CONTRASEÑA_DE_NEON;SSL Mode=VerifyFull;Channel Binding=Require
```

Este bloque es una plantilla. Introducir el valor real solo en el panel de secretos de Render. No subirlo a GitHub ni incluirlo en capturas. Si la contraseña contiene `;` u otros delimitadores, utilizar el formato de comillas admitido por Npgsql.

## 3 Publicar la API en Render

1. Registrarse en [Render](https://dashboard.render.com/) y conectar el repositorio propio de GitHub.
2. Crear **New → Blueprint** y seleccionar ese repositorio. Render detecta `render.yaml`. Como alternativa, crear un Web Service con runtime Docker, Dockerfile `./Dockerfile`, contexto raíz y plan **Free**.
3. Establecer las variables pendientes:

| Variable | Valor |
| --- | --- |
| ConnectionStrings__Veterinaria | Cadena Npgsql real de Neon, solo en Environment de Render |
| Cors__AllowedOrigin | http://localhost:3000 o el origen HTTPS exacto de un frontend propio |
| ASPNETCORE_ENVIRONMENT | Production |
| PublicOrigin | URL HTTPS del servicio, sin ruta, por ejemplo https://veterinaria-s12-cris.onrender.com |

4. Confirmar que el servicio está en plan Free e iniciar el despliegue. El contenedor escucha en 8080, puerto que Render detecta. El health check consulta `/api/propietarios`, por lo que también verifica acceso a la BD. Si todavía no conoces la URL del nuevo servicio, agrega `PublicOrigin` en Environment cuando Render la asigne y vuelve a desplegar. Esta variable permite generar enlaces Location y el servidor de Swagger con HTTPS.
5. Abrir `https://TU-SERVICIO.onrender.com/swagger`. Confirmar que `/openapi/v1.json`, `/api/propietarios` y `/api/mascotas` devuelven datos reales de Neon.

La API no ejecuta automáticamente SQL ni crea tablas: el paso del SQL Editor es obligatorio. En Render, una publicación sin la variable de conexión falla con un mensaje de configuración; una base sin tablas falla en el health check.

## 4 Probar y capturar evidencias

Ejecutar desde esta carpeta:

```powershell
python tests/test_api.py https://TU-SERVICIO.onrender.com --output artifacts/pruebas-nube
```

Si configuraste otro origen CORS, usar además `--origin https://EL-ORIGEN-CONFIGURADO`.

Revisar que todas las pruebas indiquen OK y que el JSON marque `passed: true`. Abrir el HTML de resultados para consultar todas las solicitudes y respuestas. Obtener capturas reales con la URL pública visible y cada resultado esperado. Para capturas desde Swagger, reproducir los casos indicados por el cliente y el archivo `.http`, usando registros temporales nuevos. No usar capturas de localhost ni del contenido de esta guía como evidencia de nube.

Completar en `docs/INFORME.md` el enlace de tu repositorio, la URL de Swagger y la ubicación de las capturas. Verificar que una petición desde otra computadora o red responde antes de entregar.

## Justificación de proveedores

Render admite servicios web gratuitos y contenedores Docker, lo que permite ejecutar ASP.NET Core sin depender de un runtime nativo del proveedor. Incluye un dominio público y TLS. Su plan gratuito suspende el servicio tras 15 minutos sin tráfico; el primer request puede tardar alrededor de un minuto. Es adecuado para la demostración académica dentro de sus límites de uso.

Neon ofrece PostgreSQL administrado en un plan Free. La documentación consultada el 4 de noviembre de 2026 indica 1 GB de almacenamiento por proyecto y suspensión del cómputo por inactividad. Para este laboratorio, seis registros iniciales y pruebas breves caben en ese alcance. Se elige Neon en vez de Render Postgres Free porque Render documenta una caducidad de 30 días para sus bases gratuitas. Consultar las condiciones actuales antes de crear los servicios.

Se cambia SQL Server por PostgreSQL usando `Npgsql.EntityFrameworkCore.PostgreSQL` y `UseNpgsql`. El script utiliza identidad, expresiones regulares y tipos propios de PostgreSQL. Se mantienen EF Core, LINQ, Scaffold Database First y el acceso directo al DbContext de la guía.

Fuentes oficiales:

- [Render Free](https://render.com/docs/free).
- [Render Web Services](https://render.com/docs/web-services).
- [Neon Plans](https://neon.com/docs/introduction/plans).
- [Npgsql EF Core](https://www.npgsql.org/efcore/).
- [Microsoft EF Core Scaffold](https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding/).
