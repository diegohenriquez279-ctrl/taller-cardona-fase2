# API REST de Vehículos

La API del módulo está en `TallerCardona/Controllers/Api/VehiculosApiController.cs` y usa `IVehiculoService`, por lo que comparte las validaciones de la aplicación MVC. Las respuestas usan `VehiculoDto` para no exponer directamente la entidad ni sus relaciones de Entity Framework.

## Acceso

Todos los endpoints requieren un JWT obtenido con `POST /api/auth/login`. Para consultar, registrar y actualizar se necesita el rol **Administrador** o **Recepción**. Para eliminar se necesita **Administrador**. En Swagger, primero se inicia sesión, luego se pega el token en **Authorize**.

## Endpoints

| Método | Ruta | Permiso | Respuesta esperada |
|---|---|---|---|
| GET | `/api/vehiculos?busqueda=P123` | Administrador / Recepción | `200` y lista; filtro opcional por placa, marca, modelo o cliente |
| GET | `/api/vehiculos/{id}` | Administrador / Recepción | `200` con el vehículo o `404` si no existe |
| POST | `/api/vehiculos` | Administrador / Recepción | `201` y encabezado `Location`; `400` si falla validación o regla de negocio |
| PUT | `/api/vehiculos/{id}` | Administrador / Recepción | `204`; `400` por datos/reglas inválidos o `404` si no existe |
| DELETE | `/api/vehiculos/{id}` | Solo Administrador | `204`; `400` si tiene órdenes asociadas o `404` si no existe |

La falta de token debe responder `401`; un usuario autenticado sin el rol requerido debe recibir `403`. Las validaciones del DTO incluyen placa, marca, modelo, año, kilometraje y cliente. Las reglas del servicio normalizan la placa, evitan duplicados y preservan el historial de órdenes.

## Ejemplo de cuerpo para POST y PUT

```json
{
  "placa": "P123-456",
  "marca": "Toyota",
  "modelo": "Corolla",
  "anio": 2018,
  "color": "Gris",
  "kilometraje": 68500,
  "clienteId": 1
}
```

## Verificación pendiente

La implementación está añadida al código, pero falta probar los cinco endpoints desde Swagger en la aplicación integrada. Registrar capturas de respuestas correctas, validaciones `400`, registro inexistente `404`, falta de token `401` y rol insuficiente `403` antes de marcar la API como verificada.
