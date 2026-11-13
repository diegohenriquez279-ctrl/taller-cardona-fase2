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
builder.Services.AddScoped<IClienteRepositorio, ClienteRepositorio>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// ===== Módulo Vehículos (Kimberly) =====
// TEMPORAL: la versión en memoria usa su propia lista de clientes.
// Kimberly reemplaza estas líneas por VehiculoRepositorio y VehiculoService.
builder.Services.AddSingleton<IVehiculoRepositorio>(_ =>
    new VehiculoRepositorioMemoria(new ClienteRepositorioMemoria()));

// ===== Módulo Órdenes de trabajo (Ariel) =====
builder.Services.AddScoped<IOrdenTrabajoRepositorio, OrdenTrabajoRepositorio>();
builder.Services.AddScoped<IOrdenTrabajoService, OrdenTrabajoService>();

// ===== Módulo Presupuestos y catálogo (Andrea) =====
builder.Services.AddSingleton<IPresupuestoRepositorio, PresupuestoRepositorioMemoria>();
builder.Services.AddSingleton<IServicioRepositorio, ServicioRepositorioMemoria>();
builder.Services.AddSingleton<IRepuestoRepositorio, RepuestoRepositorioMemoria>();

// ===== Módulo Pagos (Carlos) =====
// TEMPORAL: versión en memoria. Carlos la reemplaza por PagoRepositorio y PagoService.
builder.Services.AddSingleton<IPagoRepositorio, PagoRepositorioMemoria>();

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
