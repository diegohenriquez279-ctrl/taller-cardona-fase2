using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TallerCardona.Migrations
{
    /// <inheritdoc />
    public partial class InicialTallerCardona : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Repuestos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repuestos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Servicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PrecioBase = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Placa = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Kilometraje = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenesTrabajo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaEntrega = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProblemaReportado = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Diagnostico = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    KilometrajeIngreso = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VehiculoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesTrabajo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenesTrabajo_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesRepuesto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    OrdenTrabajoId = table.Column<int>(type: "int", nullable: false),
                    RepuestoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesRepuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesRepuesto_OrdenesTrabajo_OrdenTrabajoId",
                        column: x => x.OrdenTrabajoId,
                        principalTable: "OrdenesTrabajo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesRepuesto_Repuestos_RepuestoId",
                        column: x => x.RepuestoId,
                        principalTable: "Repuestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesServicio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    OrdenTrabajoId = table.Column<int>(type: "int", nullable: false),
                    ServicioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesServicio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesServicio_OrdenesTrabajo_OrdenTrabajoId",
                        column: x => x.OrdenTrabajoId,
                        principalTable: "OrdenesTrabajo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesServicio_Servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "Servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrdenTrabajoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_OrdenesTrabajo_OrdenTrabajoId",
                        column: x => x.OrdenTrabajoId,
                        principalTable: "OrdenesTrabajo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Presupuestos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalEstimado = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OrdenTrabajoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presupuestos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Presupuestos_OrdenesTrabajo_OrdenTrabajoId",
                        column: x => x.OrdenTrabajoId,
                        principalTable: "OrdenesTrabajo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Correo", "Direccion", "Nombre", "Telefono" },
                values: new object[,]
                {
                    { 1, "carlos.martinez@correo.com", "Colonia Escalón, La Libertad", "Carlos Martínez", "7712-3456" },
                    { 2, null, "Cantón Melara, La Libertad", "María Hernández", "7845-9012" },
                    { 3, "jose.ramirez@correo.com", null, "José Ramírez", "6098-7654" },
                    { 4, "ana.perez@correo.com", "Puerto de La Libertad", "Ana Lucía Pérez", "7023-4411" },
                    { 5, null, "Zaragoza, La Libertad", "Roberto Flores", "7555-1290" }
                });

            migrationBuilder.InsertData(
                table: "Repuestos",
                columns: new[] { "Id", "Descripcion", "Nombre", "Precio", "Stock" },
                values: new object[,]
                {
                    { 1, "Filtro para mantenimiento del motor", "Filtro de aceite", 8.00m, 15 },
                    { 2, "Juego de pastillas para frenos", "Pastillas de freno", 22.00m, 10 },
                    { 3, "Juego de bujías para motor", "Bujías", 12.00m, 20 },
                    { 4, "Galón de aceite sintético", "Aceite de motor 5W-30", 28.00m, 12 },
                    { 5, "Batería libre de mantenimiento", "Batería 12V", 95.00m, 4 },
                    { 6, "Filtro de aire del motor", "Filtro de aire", 10.00m, 8 }
                });

            migrationBuilder.InsertData(
                table: "Servicios",
                columns: new[] { "Id", "Activo", "Descripcion", "Nombre", "PrecioBase" },
                values: new object[,]
                {
                    { 1, true, "Cambio de aceite del motor", "Cambio de aceite", 25.00m },
                    { 2, true, "Alineación de dirección y balanceo de ruedas", "Alineación y balanceo", 30.00m },
                    { 3, true, "Inspección del sistema de frenos", "Revisión de frenos", 20.00m },
                    { 4, true, "Lectura de códigos de falla con escáner", "Diagnóstico computarizado", 15.00m },
                    { 5, true, "Cambio de bujías, filtros y limpieza de inyectores", "Afinamiento de motor", 45.00m },
                    { 6, true, "Instalación y prueba de batería nueva", "Cambio de batería", 10.00m }
                });

            migrationBuilder.InsertData(
                table: "Vehiculos",
                columns: new[] { "Id", "Anio", "ClienteId", "Color", "Kilometraje", "Marca", "Modelo", "Placa" },
                values: new object[,]
                {
                    { 1, 2018, 1, "Gris", 68500m, "Toyota", "Corolla", "P123-456" },
                    { 2, 2020, 2, "Azul", 42300m, "Honda", "Civic", "P789-012" },
                    { 3, 2017, 3, "Blanco", 91200m, "Nissan", "Versa", "P345-678" },
                    { 4, 2015, 4, "Rojo", 120400m, "Mitsubishi", "L200", "P456-789" },
                    { 5, 2021, 1, "Negro", 35800m, "Toyota", "Hilux", "P234-567" }
                });

            migrationBuilder.InsertData(
                table: "OrdenesTrabajo",
                columns: new[] { "Id", "Diagnostico", "Estado", "FechaEntrega", "FechaIngreso", "KilometrajeIngreso", "Observaciones", "ProblemaReportado", "VehiculoId" },
                values: new object[,]
                {
                    { 1, "Requiere cambio de aceite y filtro", "Entregado", new DateTime(2026, 9, 15, 16, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 14, 8, 30, 0, 0, DateTimeKind.Unspecified), 68500m, null, "Mantenimiento preventivo de los 70,000 km", 1 },
                    { 2, "Pastillas delanteras gastadas", "Entregado", new DateTime(2026, 10, 7, 15, 30, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 5, 9, 0, 0, 0, DateTimeKind.Unspecified), 42300m, null, "Ruido al frenar", 2 },
                    { 3, "Dirección desalineada y llantas desbalanceadas", "EnReparacion", null, new DateTime(2026, 10, 28, 10, 15, 0, 0, DateTimeKind.Unspecified), 91200m, null, "El vehículo vibra a alta velocidad", 3 },
                    { 4, null, "EnDiagnostico", null, new DateTime(2026, 11, 9, 7, 45, 0, 0, DateTimeKind.Unspecified), 120400m, null, "No arranca por las mañanas", 4 },
                    { 5, null, "Recibido", null, new DateTime(2026, 11, 10, 11, 0, 0, 0, DateTimeKind.Unspecified), 35800m, null, "Cambio de aceite", 5 }
                });

            migrationBuilder.InsertData(
                table: "DetallesRepuesto",
                columns: new[] { "Id", "Cantidad", "OrdenTrabajoId", "PrecioUnitario", "RepuestoId" },
                values: new object[,]
                {
                    { 1, 1, 1, 8.00m, 1 },
                    { 2, 1, 1, 28.00m, 4 },
                    { 3, 1, 2, 22.00m, 2 }
                });

            migrationBuilder.InsertData(
                table: "DetallesServicio",
                columns: new[] { "Id", "Cantidad", "OrdenTrabajoId", "PrecioUnitario", "ServicioId" },
                values: new object[,]
                {
                    { 1, 1, 1, 25.00m, 1 },
                    { 2, 1, 2, 20.00m, 3 },
                    { 3, 1, 3, 30.00m, 2 },
                    { 4, 1, 4, 15.00m, 4 }
                });

            migrationBuilder.InsertData(
                table: "Pagos",
                columns: new[] { "Id", "Estado", "Fecha", "MetodoPago", "Monto", "OrdenTrabajoId" },
                values: new object[,]
                {
                    { 1, "Registrado", new DateTime(2026, 9, 15, 16, 0, 0, 0, DateTimeKind.Unspecified), "Efectivo", 61.00m, 1 },
                    { 2, "Registrado", new DateTime(2026, 10, 5, 11, 30, 0, 0, DateTimeKind.Unspecified), "Tarjeta", 20.00m, 2 },
                    { 3, "Registrado", new DateTime(2026, 10, 7, 15, 30, 0, 0, DateTimeKind.Unspecified), "Transferencia", 22.00m, 2 }
                });

            migrationBuilder.InsertData(
                table: "Presupuestos",
                columns: new[] { "Id", "Estado", "Fecha", "Observaciones", "OrdenTrabajoId", "TotalEstimado" },
                values: new object[,]
                {
                    { 1, "Autorizado", new DateTime(2026, 9, 14, 10, 0, 0, 0, DateTimeKind.Unspecified), null, 1, 61.00m },
                    { 2, "Autorizado", new DateTime(2026, 10, 5, 11, 0, 0, 0, DateTimeKind.Unspecified), null, 2, 42.00m },
                    { 3, "Pendiente", new DateTime(2026, 10, 29, 9, 0, 0, 0, DateTimeKind.Unspecified), "Esperando confirmación del cliente", 3, 30.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetallesRepuesto_OrdenTrabajoId",
                table: "DetallesRepuesto",
                column: "OrdenTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesRepuesto_RepuestoId",
                table: "DetallesRepuesto",
                column: "RepuestoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesServicio_OrdenTrabajoId",
                table: "DetallesServicio",
                column: "OrdenTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesServicio_ServicioId",
                table: "DetallesServicio",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesTrabajo_VehiculoId",
                table: "OrdenesTrabajo",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_OrdenTrabajoId",
                table: "Pagos",
                column: "OrdenTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_OrdenTrabajoId",
                table: "Presupuestos",
                column: "OrdenTrabajoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_ClienteId",
                table: "Vehiculos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_Placa",
                table: "Vehiculos",
                column: "Placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetallesRepuesto");

            migrationBuilder.DropTable(
                name: "DetallesServicio");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Presupuestos");

            migrationBuilder.DropTable(
                name: "Repuestos");

            migrationBuilder.DropTable(
                name: "Servicios");

            migrationBuilder.DropTable(
                name: "OrdenesTrabajo");

            migrationBuilder.DropTable(
                name: "Vehiculos");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
