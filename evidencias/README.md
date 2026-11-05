# Evidencias de publicación y pruebas

API: [Swagger público](https://veterinaria-s12-cris.onrender.com/swagger/index.html).

Ejecución real del 4 de noviembre de 2026, 19:13, hora de Lima. 45 pruebas correctas contra Render, con PostgreSQL en Neon. Datos ficticios.

Las imágenes son capturas del informe del cliente HTTP Python. El visor del informe es local; cada solicitud y respuesta registrada corresponde a la URL remota que aparece en la captura. No son capturas de pruebas ejecutadas contra localhost.

- [Resultados completos en JSON](pruebas-nube/resultados.json).
- [Informe completo en HTML](pruebas-nube/resultados.html) (descargar y abrir en el navegador).
- [Swagger publicado](despliegue/swagger-publicado.png).
- [Servicio activo en Render Free](despliegue/render-publicado.png).
- [Base de datos en Neon](despliegue/neon-base-creada.png).
- [Repositorio de GitHub](despliegue/github-repositorio.png).

| N.º | Prueba | Código | Captura |
| --- | --- | --- | --- |
| 01 | Swagger disponible | 200 | [Ver](pruebas-nube/capturas/01.png) |
| 02 | OpenAPI público | 200 | [Ver](pruebas-nube/capturas/02.png) |
| 03 | Listar propietarios | 200 | [Ver](pruebas-nube/capturas/03.png) |
| 04 | Listar mascotas | 200 | [Ver](pruebas-nube/capturas/04.png) |
| 05 | Crear propietario | 201 | [Ver](pruebas-nube/capturas/05.png) |
| 06 | Crear segundo propietario | 201 | [Ver](pruebas-nube/capturas/06.png) |
| 07 | Consultar propietario creado | 200 | [Ver](pruebas-nube/capturas/07.png) |
| 08 | Documento duplicado POST | 409 | [Ver](pruebas-nube/capturas/08.png) |
| 09 | Documento duplicado PUT | 409 | [Ver](pruebas-nube/capturas/09.png) |
| 10 | Required automático | 400 | [Ver](pruebas-nube/capturas/10.png) |
| 11 | StringLength automático | 400 | [Ver](pruebas-nube/capturas/11.png) |
| 12 | Formato de documento | 400 | [Ver](pruebas-nube/capturas/12.png) |
| 13 | Id autogenerado propietario | 400 | [Ver](pruebas-nube/capturas/13.png) |
| 14 | Id propietario no coincide | 400 | [Ver](pruebas-nube/capturas/14.png) |
| 15 | Actualizar propietario | 204 | [Ver](pruebas-nube/capturas/15.png) |
| 16 | Verificar actualización propietario | 200 | [Ver](pruebas-nube/capturas/16.png) |
| 17 | Ruta anidada vacía | 200 | [Ver](pruebas-nube/capturas/17.png) |
| 18 | Crear mascota solo con FK | 201 | [Ver](pruebas-nube/capturas/18.png) |
| 19 | Consultar mascota con propietario y sin ciclo | 200 | [Ver](pruebas-nube/capturas/19.png) |
| 20 | Filtros combinados | 200 | [Ver](pruebas-nube/capturas/20.png) |
| 21 | Filtros sin coincidencias | 200 | [Ver](pruebas-nube/capturas/21.png) |
| 22 | Filtro inválido | 400 | [Ver](pruebas-nube/capturas/22.png) |
| 23 | Ruta anidada con mascota | 200 | [Ver](pruebas-nube/capturas/23.png) |
| 24 | Eliminar propietario con dependientes | 409 | [Ver](pruebas-nube/capturas/24.png) |
| 25 | FK inexistente POST | 400 | [Ver](pruebas-nube/capturas/25.png) |
| 26 | FK inexistente PUT | 400 | [Ver](pruebas-nube/capturas/26.png) |
| 27 | Range peso automático | 400 | [Ver](pruebas-nube/capturas/27.png) |
| 28 | Range edad automático | 400 | [Ver](pruebas-nube/capturas/28.png) |
| 29 | Required mascota automático | 400 | [Ver](pruebas-nube/capturas/29.png) |
| 30 | Id autogenerado mascota | 400 | [Ver](pruebas-nube/capturas/30.png) |
| 31 | Id mascota no coincide | 400 | [Ver](pruebas-nube/capturas/31.png) |
| 32 | Actualizar y reasignar mascota | 204 | [Ver](pruebas-nube/capturas/32.png) |
| 33 | Verificar actualización mascota | 200 | [Ver](pruebas-nube/capturas/33.png) |
| 34 | Eliminar mascota | 204 | [Ver](pruebas-nube/capturas/34.png) |
| 35 | Mascota inexistente GET | 404 | [Ver](pruebas-nube/capturas/35.png) |
| 36 | Mascota inexistente PUT | 404 | [Ver](pruebas-nube/capturas/36.png) |
| 37 | Mascota inexistente DELETE | 404 | [Ver](pruebas-nube/capturas/37.png) |
| 38 | Eliminar propietario | 204 | [Ver](pruebas-nube/capturas/38.png) |
| 39 | Propietario inexistente GET | 404 | [Ver](pruebas-nube/capturas/39.png) |
| 40 | Propietario inexistente PUT | 404 | [Ver](pruebas-nube/capturas/40.png) |
| 41 | Propietario inexistente DELETE | 404 | [Ver](pruebas-nube/capturas/41.png) |
| 42 | Padre inexistente en ruta anidada | 404 | [Ver](pruebas-nube/capturas/42.png) |
| 43 | CORS origen permitido | 204 | [Ver](pruebas-nube/capturas/43.png) |
| 44 | CORS origen ajeno | 204 | [Ver](pruebas-nube/capturas/44.png) |
| 45 | Limpieza propietario temporal | 204 | [Ver](pruebas-nube/capturas/45.png) |
