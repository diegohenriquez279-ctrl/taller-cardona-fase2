# UX — Módulo de vehículos e historial

**Proyecto:** Taller Cardona S.A. — DAS901, Fase 2  
**Módulo:** Vehículos e historial  
**Rama:** `modulo/vehiculos`

## 1. Propósito y alcance

El módulo centraliza la ficha de cada vehículo y permite relacionarla con su cliente propietario. El personal puede registrar, buscar, consultar, actualizar y eliminar vehículos, además de revisar el historial de órdenes asociado.

El sistema se utiliza internamente en el taller. Los clientes aportan los datos del vehículo y son sus propietarios; el personal del taller opera el sistema. Proveedores no participan en los flujos de este módulo.

## 2. Usuarios y actores

Los actores parten de los usuarios identificados para Taller Cardona en el análisis inicial: clientes, empleados (mecánicos, técnicos y personal del taller) y administración. Se precisan aquí para las tareas del módulo, sin asumir que existan cuentas o permisos por rol en el prototipo.

| Actor | Relación con el módulo | Necesidad |
|---|---|---|
| Encargado del taller o empleado que recibe vehículos | Usuario principal del sistema | Registrar el vehículo y asociarlo al cliente correcto; encontrarlo y revisar su ficha e historial al recibirlo de nuevo |
| Mecánico o técnico | Usuario interno de consulta | Consultar los datos y servicios anteriores del vehículo para apoyar la inspección y el diagnóstico |
| Administración | Usuario interno que mantiene los registros | Corregir datos y conservar información ordenada para la operación del taller |
| Cliente propietario | Actor externo al sistema | Proporcionar los datos del vehículo y recibir atención vinculada al historial de ese vehículo |

**Fuera del alcance de esta pantalla:** proveedores, control de inventario y gestión de pagos.

## 3. Historias de usuario

| Código | Historia | Prioridad | Criterios de aceptación |
|---|---|---|---|
| HU-V01 | Como encargado del taller, quiero registrar un vehículo asociado a un cliente para mantener organizada su información. | Alta | Selecciono un cliente registrado y completo placa, marca, modelo, año y kilometraje. El sistema confirma el registro si los datos son válidos. |
| HU-V02 | Como encargado, quiero consultar y buscar vehículos para encontrar rápidamente el registro que necesito. | Alta | Puedo buscar por placa, marca, modelo o cliente; puedo limpiar la búsqueda y recibo un mensaje si no hay coincidencias. |
| HU-V03 | Como encargado, quiero actualizar los datos de un vehículo para mantener su información vigente. | Alta | El formulario presenta los datos actuales, permite editarlos y valida antes de guardar. |
| HU-V04 | Como encargado, quiero consultar el detalle de un vehículo para verificar sus datos antes de atenderlo. | Alta | El detalle muestra placa, marca, modelo, año, color, kilometraje y cliente propietario. |
| HU-V05 | Como encargado o técnico, quiero consultar el historial de servicios y reparaciones de un vehículo para conocer los trabajos realizados anteriormente. | Alta | Se muestran las órdenes asociadas, priorizando las más recientes. Si no existen órdenes, se informa que el historial está vacío. |
| HU-V06 | Como encargado, quiero validar los datos al registrar o editar un vehículo para evitar información incompleta o incorrecta. | Alta | Se señalan los campos inválidos, se conservan los datos escritos y no se guarda una placa duplicada. |
| HU-V07 | Como encargado, quiero eliminar un registro de vehículo ingresado por error para mantener confiada la lista. | Media | Antes de eliminar, el sistema pide confirmación. No permite eliminar un vehículo que tenga órdenes en su historial. |

## 4. Casos de uso

### CU-V01 — Registrar vehículo

- **Actor principal:** encargado del taller.
- **Precondición:** existe al menos un cliente registrado.
- **Flujo básico:** abrir Vehículos → Registrar vehículo → seleccionar cliente → completar datos → Guardar vehículo → ver confirmación en el listado.
- **Alternativas:** si falta un dato, el formulario conserva lo escrito y muestra el error; si la placa ya existe, se rechaza el registro y se señala el campo Placa.

### CU-V02 — Buscar y consultar historial

- **Actor principal:** encargado del taller o técnico.
- **Precondición:** el vehículo está registrado.
- **Flujo básico:** abrir Vehículos → buscar por placa, marca, modelo o cliente → seleccionar Ver historial → consultar la ficha y las órdenes.
- **Alternativas:** si no hay coincidencias, se presenta un mensaje; si el vehículo no tiene órdenes, se muestra el estado vacío del historial.

### CU-V03 — Actualizar vehículo

- **Actor principal:** encargado del taller.
- **Precondición:** el vehículo está registrado.
- **Flujo básico:** localizar vehículo → Editar → modificar campos → Guardar cambios → volver al listado con confirmación.
- **Alternativas:** las validaciones y la regla de placa única se aplican también al editar.

### CU-V04 — Eliminar vehículo

- **Actor principal:** encargado del taller.
- **Precondición:** el vehículo está registrado.
- **Flujo básico:** localizar vehículo → Eliminar → revisar confirmación → confirmar → volver al listado.
- **Alternativa:** si el vehículo tiene órdenes en su historial, el sistema bloquea la eliminación para conservar la trazabilidad.

## 5. Flujos de navegación

### Registro

Inicio → Vehículos → Registrar vehículo → seleccionar cliente → completar ficha → validar campos y placa → guardar y regresar al listado. Si hay errores, el usuario permanece en el formulario y corrige los campos señalados.

### Búsqueda e historial

Inicio → Vehículos → buscar por placa, marca, modelo o cliente → seleccionar resultado → Ver historial → revisar ficha y órdenes → volver al listado.

### Edición

Inicio → Vehículos → buscar registro → Editar → modificar datos → validar → guardar y regresar al listado.

### Eliminación

Inicio → Vehículos → localizar registro → Eliminar → confirmar. Si tiene órdenes asociadas, la acción se bloquea y el historial se conserva.

## 6. Mapa de navegación

![Mapa de navegación del módulo de vehículos](mapa_navegacion.jpeg)

## 7. Reglas y estados de interfaz

- Un vehículo pertenece a un cliente registrado y su historial se consulta desde la ficha del vehículo.
- La placa es obligatoria y única; al guardarse se normaliza a mayúsculas.
- Las validaciones deben indicar qué corregir sin borrar los demás datos ingresados.
- Una búsqueda vacía muestra un mensaje y permite limpiar el filtro.
- Un historial sin órdenes muestra un mensaje informativo, no una tabla vacía.
- La eliminación requiere confirmación y se bloquea si existen órdenes relacionadas.
- Los registros actuales se guardan en memoria y se reinician al detener la aplicación.
- Los perfiles identifican actores y necesidades; el prototipo no implementa inicio de sesión ni permisos por rol.

## 8. Nota de alineación

Este borrador se basa en la conversación compartida, el README del repositorio y el comportamiento implementado. Revisar la terminología final y el formato contra el documento oficial de evaluación antes de entregar.
