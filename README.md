# Taller Cardona S.A. – Prototipo web (Fase 2)

Proyecto de cátedra de **Desarrollo de Aplicaciones con Software Propietario (DAS901)**, grupo G01T, UDB Virtual.
Sistema de gestión para un taller de mecánica automotriz de La Libertad: clientes, vehículos, órdenes de trabajo, presupuestos y pagos.

- **Tecnología:** ASP.NET Core MVC con C# (.NET 10)
- **Datos:** en memoria por ahora (la base de datos llega en una fase posterior)
- **Entrega de la Fase 2:** sábado 17 de octubre de 2026

## Cómo arrancar

1. Instalar **Visual Studio Community** con la carga de trabajo *Desarrollo de ASP.NET y web*.
2. Tener una cuenta de GitHub y vincularla en Visual Studio (Archivo → Configuración de la cuenta).
3. Aceptar la invitación al repositorio que llega por correo.
4. En Visual Studio: *Clonar un repositorio* y pegar la dirección de este repositorio.
5. Abrir `TallerCardona.slnx` y presionar ▶ **https** para comprobar que corre.

## Cómo trabajamos en Git

- `main` es la versión oficial. **Nadie sube cambios directo a main.**
- Cada quien trabaja en su propia rama, con el nombre `modulo/<nombre>`, por ejemplo `modulo/vehiculos`.
- Antes de empezar a trabajar cada día, traer los cambios de main (Git → Extraer).
- Commits pequeños, con nuestra propia cuenta, y con un mensaje que diga qué se hizo. Formato: `Módulo: acción`.
  Ejemplo: `Vehículos: agrega validación de año y kilometraje`
- Para unir el trabajo se abre un *pull request* de la rama hacia `main` y otro compañero lo revisa.
- Cada quien toca solo los archivos de su módulo. En `Program.cs` y en el menú del `_Layout` solo se agrega la línea propia y se avisa en el grupo.

## Módulos

| # | Módulo | Rama | Responsable |
|---|--------|------|-------------|
| 1 | Clientes (+ arquitectura y repositorio) | `modulo/clientes` | Diego |
| 2 | Vehículos e historial (+ UX) | `modulo/vehiculos` | Por definir |
| 3 | Órdenes de trabajo y tablero (+ documento UX/UI) | `modulo/ordenes` | Por definir |
| 4 | Presupuestos y catálogo (+ integración y video) | `modulo/presupuestos` | Por definir |
| 5 | Pagos y reportes (+ UI y estilos) | `modulo/pagos` | Por definir |

## Estructura del proyecto

- `Models/`: clases del dominio (las 9 del Avance 1) y enumeraciones.
- `Services/`: interfaz y repositorio en memoria de cada módulo.
- `Controllers/`: un controlador por módulo.
- `Views/`: una carpeta por controlador, más `Shared/` para el layout y las vistas parciales.
- `wwwroot/`: CSS, JavaScript e imágenes.
- `docs/`: entregables que no son código (UX, UI, documento).

## Convenciones de nombres

- Clases, métodos y propiedades en español y en PascalCase: `OrdenTrabajo`, `ListarPorVehiculo`.
- Controlador en plural: `ClientesController`. Sus vistas van en `Views/Clientes/`.
- Repositorios: interfaz `IClienteRepositorio` e implementación `ClienteRepositorioMemoria`.
- Validaciones con Data Annotations en el modelo, con mensajes en español.
- Toda la interfaz del sistema va en español.