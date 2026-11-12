using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Repositorios;
using TallerCardona.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ===== Base de datos (Diego) =====
// La cadena de conexión está en appsettings.json. Si su servidor se llama
// distinto, usen "Administrar secretos de usuario" (ver el plan de trabajo).
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

// ===== Módulo Clientes (Diego) =====
builder.Services.AddSingleton<IClienteRepositorio, ClienteRepositorioMemoria>();

// ===== Módulo Vehículos (Kimberly) =====
builder.Services.AddSingleton<IVehiculoRepositorio, VehiculoRepositorioMemoria>();

// ===== Módulo Órdenes de trabajo (Ariel) =====
builder.Services.AddSingleton<IOrdenTrabajoRepositorio, OrdenTrabajoRepositorioMemoria>();

// ===== Módulo Presupuestos y catálogo (Andrea) =====
builder.Services.AddSingleton<IPresupuestoRepositorio, PresupuestoRepositorioMemoria>();
builder.Services.AddSingleton<IServicioRepositorio, ServicioRepositorioMemoria>();
builder.Services.AddSingleton<IRepuestoRepositorio, RepuestoRepositorioMemoria>();

// ===== Módulo Pagos (Carlos) =====
// Aquí van IPagoRepositorio / PagoRepositorio e IPagoService / PagoService

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
