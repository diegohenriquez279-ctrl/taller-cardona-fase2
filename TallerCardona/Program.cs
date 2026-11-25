using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TallerCardona.Data;
using TallerCardona.Repositorios;
using TallerCardona.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ===== Base de datos (Diego) =====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();

// ===== Seguridad: Identity (usuarios y roles) =====
// Guarda usuarios y roles en la misma base (AppDbContext) y usa cookie para la parte MVC.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(opciones =>
    {
        // Política de contraseñas: mínimo 8 caracteres con mayúscula, minúscula, número y símbolo.
        opciones.Password.RequiredLength = 8;
        opciones.Password.RequireDigit = true;
        opciones.Password.RequireUppercase = true;
        opciones.Password.RequireLowercase = true;
        opciones.Password.RequireNonAlphanumeric = true;

        // Bloqueo: 5 intentos fallidos bloquean la cuenta 5 minutos.
        opciones.Lockout.MaxFailedAccessAttempts = 5;
        opciones.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        opciones.Lockout.AllowedForNewUsers = true;

        opciones.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Rutas de la cookie: a dónde se manda al usuario sin sesión o sin permiso.
builder.Services.ConfigureApplicationCookie(opciones =>
{
    opciones.LoginPath = "/Account/Login";
    opciones.LogoutPath = "/Account/Logout";
    opciones.AccessDeniedPath = "/Account/AccesoDenegado";
    opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
    opciones.SlidingExpiration = true;
});

// ===== Módulo Clientes (Diego) =====
builder.Services.AddScoped<IClienteRepositorio, ClienteRepositorio>();
builder.Services.AddScoped<IClienteService, ClienteService>();

// ===== Módulo Vehículos (Kimberly) =====
builder.Services.AddScoped<IVehiculoRepositorio, VehiculoRepositorio>();
builder.Services.AddScoped<IVehiculoService, VehiculoService>();

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

// Crea roles y usuarios de prueba si no existen (ver Data/SeedIdentidad.cs).
await SeedIdentidad.CargarAsync(app.Services);

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