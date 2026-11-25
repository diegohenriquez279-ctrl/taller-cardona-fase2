using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TallerCardona.Data;
using TallerCardona.Models;
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

// ===== Seguridad: JWT para la API =====
// La cookie de Identity sigue siendo el esquema POR DEFECTO (lo usa toda la parte MVC).
// JWT se agrega como SEGUNDO esquema y solo lo usan los controladores que heredan de
// Controllers/Api/ApiControllerBase (ahí se indica AuthenticationSchemes = JwtBearer).
//
// La clave "Jwt:Key" NO va en appsettings.json:
//   - Desarrollo: está en appsettings.Development.json (clave de prueba).
//   - Producción (Azure): crear la variable de entorno / App Setting  Jwt__Key = <clave larga y secreta>
//     (el doble guion bajo "__" equivale a "Jwt:Key"). Ejemplo en PowerShell:
//       $env:Jwt__Key = "una-clave-muy-larga-de-al-menos-32-caracteres"
builder.Services.AddScoped<TokenService>();
builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opciones =>
    {
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = TokenService.ObtenerClave(builder.Configuration),
            // Para que User.Identity.Name y los roles del token funcionen igual que con la cookie.
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

// ===== Seguridad: políticas de autorización =====
// En los controladores se usan por nombre: [Authorize(Policy = "AdminORecepcion")]
//
// ¿Cómo sirven las MISMAS políticas para MVC (cookie) y API (JWT)?
// Las políticas solo piden ROLES; no dicen con qué esquema se autentica el usuario.
// El esquema lo pone cada controlador:
//   - Controladores MVC: no indican nada -> se usa la cookie (esquema por defecto).
//   - Controladores API: heredan de ApiControllerBase, que indica JWT.
// ASP.NET junta el [Authorize] de la clase base con el [Authorize(Policy=...)] del
// controlador, así que en la API se exige "token JWT válido + rol de la política".
// Por eso NO hace falta duplicar las políticas para la API.
builder.Services.AddAuthorization(opciones =>
{
    // Política de respaldo: TODO requiere usuario autenticado, salvo lo que tenga [AllowAnonymous].
    // Así, si alguien olvida poner [Authorize] en un controlador, igual queda protegido.
    opciones.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Solo el administrador (por ejemplo, eliminar registros).
    opciones.AddPolicy("SoloAdministrador", p => p.RequireRole(Roles.Administrador));

    // Administrador o recepción (clientes, vehículos, presupuestos, pagos...).
    opciones.AddPolicy("AdminORecepcion", p => p.RequireRole(Roles.Administrador, Roles.Recepcion));

    // Cualquier persona del taller (los tres roles), por ejemplo para órdenes de trabajo.
    opciones.AddPolicy("PersonalTaller", p => p.RequireRole(Roles.Todos));
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
app.UseAuthentication(); // ¿quién es el usuario? (lee la cookie o el token)
app.UseAuthorization();  // ¿tiene permiso? (revisa [Authorize] y las políticas)
// CSS, JS e imágenes se sirven sin sesión (si no, la política de respaldo
// bloquearía hasta los estilos de la página de login).
app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();