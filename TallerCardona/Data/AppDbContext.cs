using Microsoft.EntityFrameworkCore;
using TallerCardona.Models;

namespace TallerCardona.Data;

// Contexto de Entity Framework: representa la base TallerCardonaDB.
// Cada DbSet es una tabla. Las relaciones y reglas de las columnas
// se configuran en OnModelCreating (Fluent API).
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
    public DbSet<OrdenTrabajo> OrdenesTrabajo => Set<OrdenTrabajo>();
    public DbSet<DetalleServicio> DetallesServicio => Set<DetalleServicio>();
    public DbSet<DetalleRepuesto> DetallesRepuesto => Set<DetalleRepuesto>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Repuesto> Repuestos => Set<Repuesto>();
    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<Pago> Pagos => Set<Pago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Clientes ----------
        modelBuilder.Entity<Cliente>(entidad =>
        {
            entidad.ToTable("Clientes");
            entidad.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
            entidad.Property(c => c.Telefono).HasMaxLength(9).IsRequired();
            entidad.Property(c => c.Correo).HasMaxLength(100);
            entidad.Property(c => c.Direccion).HasMaxLength(200);
        });

        // ---------- Vehículos (un cliente tiene muchos vehículos) ----------
        modelBuilder.Entity<Vehiculo>(entidad =>
        {
            entidad.ToTable("Vehiculos");
            entidad.Property(v => v.Placa).HasMaxLength(10).IsRequired();
            entidad.HasIndex(v => v.Placa).IsUnique();
            entidad.Property(v => v.Marca).HasMaxLength(50).IsRequired();
            entidad.Property(v => v.Modelo).HasMaxLength(50).IsRequired();
            entidad.Property(v => v.Color).HasMaxLength(30);
            entidad.Property(v => v.Kilometraje).HasColumnType("decimal(10,2)");

            entidad.HasOne(v => v.Cliente)
                .WithMany(c => c.Vehiculos)
                .HasForeignKey(v => v.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Órdenes de trabajo (un vehículo tiene muchas órdenes) ----------
        modelBuilder.Entity<OrdenTrabajo>(entidad =>
        {
            entidad.ToTable("OrdenesTrabajo");
            entidad.Property(o => o.ProblemaReportado).HasMaxLength(500).IsRequired();
            entidad.Property(o => o.Diagnostico).HasMaxLength(500);
            entidad.Property(o => o.Observaciones).HasMaxLength(500);
            entidad.Property(o => o.KilometrajeIngreso).HasColumnType("decimal(10,2)");
            entidad.Property(o => o.Estado).HasConversion<string>().HasMaxLength(30);

            entidad.HasOne(o => o.Vehiculo)
                .WithMany(v => v.Ordenes)
                .HasForeignKey(o => o.VehiculoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Detalle de servicios de una orden ----------
        modelBuilder.Entity<DetalleServicio>(entidad =>
        {
            entidad.ToTable("DetallesServicio");
            entidad.Property(d => d.PrecioUnitario).HasColumnType("decimal(10,2)");
            entidad.Ignore(d => d.Subtotal); // se calcula en C#, no se guarda

            entidad.HasOne(d => d.OrdenTrabajo)
                .WithMany(o => o.Servicios)
                .HasForeignKey(d => d.OrdenTrabajoId)
                .OnDelete(DeleteBehavior.Cascade);

            entidad.HasOne(d => d.Servicio)
                .WithMany()
                .HasForeignKey(d => d.ServicioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Detalle de repuestos de una orden ----------
        modelBuilder.Entity<DetalleRepuesto>(entidad =>
        {
            entidad.ToTable("DetallesRepuesto");
            entidad.Property(d => d.PrecioUnitario).HasColumnType("decimal(10,2)");
            entidad.Ignore(d => d.Subtotal);

            entidad.HasOne(d => d.OrdenTrabajo)
                .WithMany(o => o.Repuestos)
                .HasForeignKey(d => d.OrdenTrabajoId)
                .OnDelete(DeleteBehavior.Cascade);

            entidad.HasOne(d => d.Repuesto)
                .WithMany()
                .HasForeignKey(d => d.RepuestoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Catálogo de servicios ----------
        modelBuilder.Entity<Servicio>(entidad =>
        {
            entidad.ToTable("Servicios");
            entidad.Property(s => s.Nombre).HasMaxLength(100).IsRequired();
            entidad.Property(s => s.Descripcion).HasMaxLength(250);
            entidad.Property(s => s.PrecioBase).HasColumnType("decimal(10,2)");
        });

        // ---------- Catálogo de repuestos ----------
        modelBuilder.Entity<Repuesto>(entidad =>
        {
            entidad.ToTable("Repuestos");
            entidad.Property(r => r.Nombre).HasMaxLength(100).IsRequired();
            entidad.Property(r => r.Descripcion).HasMaxLength(250);
            entidad.Property(r => r.Precio).HasColumnType("decimal(10,2)");
        });

        // ---------- Presupuestos (una orden tiene un solo presupuesto) ----------
        modelBuilder.Entity<Presupuesto>(entidad =>
        {
            entidad.ToTable("Presupuestos");
            entidad.Property(p => p.TotalEstimado).HasColumnType("decimal(10,2)");
            entidad.Property(p => p.Estado).HasConversion<string>().HasMaxLength(20);
            entidad.Property(p => p.Observaciones).HasMaxLength(500);

            entidad.HasOne(p => p.OrdenTrabajo)
                .WithOne(o => o.Presupuesto)
                .HasForeignKey<Presupuesto>(p => p.OrdenTrabajoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Pagos (una orden puede tener varios pagos) ----------
        modelBuilder.Entity<Pago>(entidad =>
        {
            entidad.ToTable("Pagos");
            entidad.Property(p => p.Monto).HasColumnType("decimal(10,2)");
            entidad.Property(p => p.MetodoPago).HasConversion<string>().HasMaxLength(20);
            entidad.Property(p => p.Estado).HasMaxLength(20);

            entidad.HasOne(p => p.OrdenTrabajo)
                .WithMany(o => o.Pagos)
                .HasForeignKey(p => p.OrdenTrabajoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Datos de prueba para que todos tengan la misma información al crear la base
        DatosIniciales.Cargar(modelBuilder);
    }
}
