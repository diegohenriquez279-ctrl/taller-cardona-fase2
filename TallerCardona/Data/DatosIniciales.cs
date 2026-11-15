using Microsoft.EntityFrameworkCore;
using TallerCardona.Models;

namespace TallerCardona.Data;

// Datos de prueba (seed). Se insertan con la migración, así todos
// tenemos los mismos registros para probar y para el video.
// Las fechas son fijas porque HasData no acepta DateTime.Now.
public static class DatosIniciales
{
    public static void Cargar(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>().HasData(
            new Cliente { Id = 1, Nombre = "Carlos Martínez", Telefono = "7712-3456", Correo = "carlos.martinez@correo.com", Direccion = "Colonia Escalón, La Libertad" },
            new Cliente { Id = 2, Nombre = "María Hernández", Telefono = "7845-9012", Direccion = "Cantón Melara, La Libertad" },
            new Cliente { Id = 3, Nombre = "José Ramírez", Telefono = "6098-7654", Correo = "jose.ramirez@correo.com" },
            new Cliente { Id = 4, Nombre = "Ana Lucía Pérez", Telefono = "7023-4411", Correo = "ana.perez@correo.com", Direccion = "Puerto de La Libertad" },
            new Cliente { Id = 5, Nombre = "Roberto Flores", Telefono = "7555-1290", Direccion = "Zaragoza, La Libertad" }
        );

        modelBuilder.Entity<Vehiculo>().HasData(
            new Vehiculo { Id = 1, Placa = "P123-456", Marca = "Toyota", Modelo = "Corolla", Anio = 2018, Color = "Gris", Kilometraje = 68500, ClienteId = 1 },
            new Vehiculo { Id = 2, Placa = "P789-012", Marca = "Honda", Modelo = "Civic", Anio = 2020, Color = "Azul", Kilometraje = 42300, ClienteId = 2 },
            new Vehiculo { Id = 3, Placa = "P345-678", Marca = "Nissan", Modelo = "Versa", Anio = 2017, Color = "Blanco", Kilometraje = 91200, ClienteId = 3 },
            new Vehiculo { Id = 4, Placa = "P456-789", Marca = "Mitsubishi", Modelo = "L200", Anio = 2015, Color = "Rojo", Kilometraje = 120400, ClienteId = 4 },
            new Vehiculo { Id = 5, Placa = "P234-567", Marca = "Toyota", Modelo = "Hilux", Anio = 2021, Color = "Negro", Kilometraje = 35800, ClienteId = 1 }
        );

        modelBuilder.Entity<Servicio>().HasData(
            new Servicio { Id = 1, Nombre = "Cambio de aceite", Descripcion = "Cambio de aceite del motor", PrecioBase = 25.00m, Activo = true },
            new Servicio { Id = 2, Nombre = "Alineación y balanceo", Descripcion = "Alineación de dirección y balanceo de ruedas", PrecioBase = 30.00m, Activo = true },
            new Servicio { Id = 3, Nombre = "Revisión de frenos", Descripcion = "Inspección del sistema de frenos", PrecioBase = 20.00m, Activo = true },
            new Servicio { Id = 4, Nombre = "Diagnóstico computarizado", Descripcion = "Lectura de códigos de falla con escáner", PrecioBase = 15.00m, Activo = true },
            new Servicio { Id = 5, Nombre = "Afinamiento de motor", Descripcion = "Cambio de bujías, filtros y limpieza de inyectores", PrecioBase = 45.00m, Activo = true },
            new Servicio { Id = 6, Nombre = "Cambio de batería", Descripcion = "Instalación y prueba de batería nueva", PrecioBase = 10.00m, Activo = true }
        );

        modelBuilder.Entity<Repuesto>().HasData(
            new Repuesto { Id = 1, Nombre = "Filtro de aceite", Descripcion = "Filtro para mantenimiento del motor", Precio = 8.00m, Stock = 15 },
            new Repuesto { Id = 2, Nombre = "Pastillas de freno", Descripcion = "Juego de pastillas para frenos", Precio = 22.00m, Stock = 10 },
            new Repuesto { Id = 3, Nombre = "Bujías", Descripcion = "Juego de bujías para motor", Precio = 12.00m, Stock = 20 },
            new Repuesto { Id = 4, Nombre = "Aceite de motor 5W-30", Descripcion = "Galón de aceite sintético", Precio = 28.00m, Stock = 12 },
            new Repuesto { Id = 5, Nombre = "Batería 12V", Descripcion = "Batería libre de mantenimiento", Precio = 95.00m, Stock = 4 },
            new Repuesto { Id = 6, Nombre = "Filtro de aire", Descripcion = "Filtro de aire del motor", Precio = 10.00m, Stock = 8 }
        );

        modelBuilder.Entity<OrdenTrabajo>().HasData(
            new OrdenTrabajo { Id = 1, VehiculoId = 1, FechaIngreso = new DateTime(2026, 9, 14, 8, 30, 0), FechaEntrega = new DateTime(2026, 9, 15, 16, 0, 0), ProblemaReportado = "Mantenimiento preventivo de los 70,000 km", Diagnostico = "Requiere cambio de aceite y filtro", Estado = EstadoOrden.Entregado, KilometrajeIngreso = 68500 },
            new OrdenTrabajo { Id = 2, VehiculoId = 2, FechaIngreso = new DateTime(2026, 10, 5, 9, 0, 0), FechaEntrega = new DateTime(2026, 10, 7, 15, 30, 0), ProblemaReportado = "Ruido al frenar", Diagnostico = "Pastillas delanteras gastadas", Estado = EstadoOrden.Entregado, KilometrajeIngreso = 42300 },
            new OrdenTrabajo { Id = 3, VehiculoId = 3, FechaIngreso = new DateTime(2026, 10, 28, 10, 15, 0), ProblemaReportado = "El vehículo vibra a alta velocidad", Diagnostico = "Dirección desalineada y llantas desbalanceadas", Estado = EstadoOrden.EnReparacion, KilometrajeIngreso = 91200 },
            new OrdenTrabajo { Id = 4, VehiculoId = 4, FechaIngreso = new DateTime(2026, 11, 9, 7, 45, 0), ProblemaReportado = "No arranca por las mañanas", Estado = EstadoOrden.EnDiagnostico, KilometrajeIngreso = 120400 },
            new OrdenTrabajo { Id = 5, VehiculoId = 5, FechaIngreso = new DateTime(2026, 11, 10, 11, 0, 0), ProblemaReportado = "Cambio de aceite", Estado = EstadoOrden.Recibido, KilometrajeIngreso = 35800 }
        );

        modelBuilder.Entity<DetalleServicio>().HasData(
            new DetalleServicio { Id = 1, OrdenTrabajoId = 1, ServicioId = 1, Cantidad = 1, PrecioUnitario = 25.00m },
            new DetalleServicio { Id = 2, OrdenTrabajoId = 2, ServicioId = 3, Cantidad = 1, PrecioUnitario = 20.00m },
            new DetalleServicio { Id = 3, OrdenTrabajoId = 3, ServicioId = 2, Cantidad = 1, PrecioUnitario = 30.00m },
            new DetalleServicio { Id = 4, OrdenTrabajoId = 4, ServicioId = 4, Cantidad = 1, PrecioUnitario = 15.00m }
        );

        modelBuilder.Entity<DetalleRepuesto>().HasData(
            new DetalleRepuesto { Id = 1, OrdenTrabajoId = 1, RepuestoId = 1, Cantidad = 1, PrecioUnitario = 8.00m },
            new DetalleRepuesto { Id = 2, OrdenTrabajoId = 1, RepuestoId = 4, Cantidad = 1, PrecioUnitario = 28.00m },
            new DetalleRepuesto { Id = 3, OrdenTrabajoId = 2, RepuestoId = 2, Cantidad = 1, PrecioUnitario = 22.00m }
        );

        modelBuilder.Entity<Presupuesto>().HasData(
            new Presupuesto { Id = 1, OrdenTrabajoId = 1, Fecha = new DateTime(2026, 9, 14, 10, 0, 0), TotalEstimado = 61.00m, Estado = EstadoPresupuesto.Autorizado },
            new Presupuesto { Id = 2, OrdenTrabajoId = 2, Fecha = new DateTime(2026, 10, 5, 11, 0, 0), TotalEstimado = 42.00m, Estado = EstadoPresupuesto.Autorizado },
            new Presupuesto { Id = 3, OrdenTrabajoId = 3, Fecha = new DateTime(2026, 10, 29, 9, 0, 0), TotalEstimado = 30.00m, Estado = EstadoPresupuesto.Pendiente, Observaciones = "Esperando confirmación del cliente" }
        );

        modelBuilder.Entity<Pago>().HasData(
            new Pago { Id = 1, OrdenTrabajoId = 1, Fecha = new DateTime(2026, 9, 15, 16, 0, 0), Monto = 61.00m, MetodoPago = MetodoPago.Efectivo, Estado = "Registrado" },
            new Pago { Id = 2, OrdenTrabajoId = 2, Fecha = new DateTime(2026, 10, 5, 11, 30, 0), Monto = 20.00m, MetodoPago = MetodoPago.Tarjeta, Estado = "Registrado" },
            new Pago { Id = 3, OrdenTrabajoId = 2, Fecha = new DateTime(2026, 10, 7, 15, 30, 0), Monto = 22.00m, MetodoPago = MetodoPago.Transferencia, Estado = "Registrado" }
        );
    }
}
