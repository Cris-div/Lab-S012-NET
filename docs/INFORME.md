# Laboratorio S12 API REST de veterinaria

## Tema y problema

La API organiza los datos de propietarios y mascotas de una veterinaria. Permite registrar responsables, asociarles sus mascotas, actualizar los datos y consultar animales por propietario, nombre y especie. Evita la duplicación de responsables por documento y la eliminación de propietarios que aún tienen mascotas.

## Modelo y relación

Propietario contiene Id, Nombre, Documento y Telefono. Mascota contiene Id, Nombre, Especie, Edad, Peso y PropietarioId. Un propietario puede tener muchas mascotas; cada mascota pertenece a un propietario. La base utiliza una FK obligatoria y una relación 1:N con eliminación restringida.

El modelo se generó con Scaffold desde una base PostgreSQL creada con el script SQL del proyecto. Se agregaron validaciones `[Required]`, `[StringLength]`, `[Range]` y validaciones de formato. La propiedad `Propietario?` acepta nulo en el JSON para permitir un POST con solo la FK; la base mantiene la relación obligatoria. `[JsonIgnore]` en la colección de mascotas evita ciclos sin DTOs.

## Reglas de negocio

1. Un documento de propietario no puede repetirse: POST y PUT responden 409 si ya pertenece a otro registro.
2. Un propietario con mascotas no puede eliminarse: DELETE responde 409 hasta eliminar o reasignar los dependientes.

Las restricciones UNIQUE y FOREIGN KEY refuerzan estas reglas en la base. Los datos inválidos y las FK inexistentes responden 400; los recursos inexistentes, 404. Los POST devuelven 201 y Location, los GET correctos 200 y los PUT y DELETE correctos 204.

## Implementación

ASP.NET Core .NET 10 con dos controladores `[ApiController]` y rutas `api/[controller]`. Los controladores utilizan directamente `VeterinariaContext`, con consultas asíncronas LINQ y métodos como `ToListAsync`, `FindAsync`, `AnyAsync`, `Include` y `AsNoTracking`. Hay 11 operaciones: cinco por entidad y una ruta anidada para consultar las mascotas de un propietario. El GET de mascotas admite filtros combinados por query string.

Swagger y OpenAPI están habilitados también en Production. CORS acepta un origen concreto configurable. La cadena de conexión se lee desde una variable de entorno y no está incorporada a archivos versionables.

## Proveedores utilizados

Render Free aloja la API en Docker y Neon Free aloja PostgreSQL 17 en el proyecto `veterinaria-s12`. Se sustituyó el proveedor SQL Server de la guía por Npgsql, usando `UseNpgsql` y un script PostgreSQL. La elección permite publicar con servicios gratuitos y conservar Database First. La justificación, fuentes y procedimiento están en `DESPLIEGUE.md`.

## Verificación realizada

El 4 de noviembre de 2026 se compiló la API en Release sin advertencias ni errores y se ejecutaron 45 comprobaciones HTTP correctas contra PostgreSQL 17 local. Cubren todas las operaciones, los seis códigos requeridos, las reglas de negocio, filtros, Location, persistencia, serialización y CORS. Los registros temporales creados durante las pruebas se eliminaron al finalizar. Los resultados locales se encuentran en `artifacts/pruebas-locales/` y están excluidos del repositorio.

También se construyó la imagen Docker y se ejecutó una comprobación del contenedor: Swagger devolvió 200 y el endpoint de mascotas devolvió los tres registros de ejemplo. Tras las pruebas se verificó que permanecen exactamente tres propietarios y tres mascotas iniciales.

El 4 de noviembre de 2026 se publicó la API en Render y se ejecutó el SQL en Neon. Después de configurar el origen HTTPS público se repitieron las 45 comprobaciones sobre `https://veterinaria-s12-cris.onrender.com`: todas pasaron. Incluyen los 11 endpoints y los códigos 200, 201, 204, 400, 404 y 409. La ejecución quedó registrada a las 19:13, hora de Lima (00:13 UTC del día siguiente). Los encabezados Location de los POST apuntan al recurso creado en HTTPS.

Las capturas muestran el informe del cliente HTTP Python con la URL remota, solicitud, respuesta y código recibido. El visor del informe se abrió localmente para capturarlo; las solicitudes registradas se ejecutaron contra Render y no contra localhost. El JSON conserva los cuerpos y cabeceras completos. También se incluyen capturas de Swagger público, Render activo, GitHub y los registros iniciales de Neon.

## Enlaces y evidencias de entrega

| Entregable | Estado |
| --- | --- |
| Repositorio propio | [Cris-div/Lab-S012-NET](https://github.com/Cris-div/Lab-S012-NET) |
| Swagger público | [Veterinaria API en Render](https://veterinaria-s12-cris.onrender.com/swagger/index.html) |
| Base de datos en la nube | Neon Free, PostgreSQL 17, proyecto `veterinaria-s12`; SQL ejecutado |
| Pruebas sobre URL publicada | 45 de 45 correctas; [JSON completo](../evidencias/pruebas-nube/resultados.json) |
| Capturas de cada endpoint y código | [Índice de 45 capturas](../evidencias/README.md) |

## Observaciones y conclusiones

El enfoque Database First permite que el esquema SQL sea el origen del modelo; las anotaciones añadidas tras Scaffold complementan las restricciones de la base y producen respuestas 400 automáticamente. Las reglas de negocio devuelven 409 y preservan la relación entre responsables y mascotas.

La separación entre configuración y código permite usar una base local para desarrollar y Neon para publicar sin incorporar contraseñas al repositorio. Los planes gratuitos suspenden recursos por inactividad, por lo que el primer acceso puede tardar. La API y la base de datos quedaron publicadas y se verificaron las respuestas desde un cliente externo. Un origen público fijo permite generar enlaces HTTPS detrás del proxy sin confiar en cabeceras enviadas por cualquier cliente. Las capturas y el registro completo permiten revisar los resultados de la ejecución en nube.
