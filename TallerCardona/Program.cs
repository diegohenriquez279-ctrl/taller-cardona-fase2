using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using TallerCardona.Data;
using TallerCardona.Middleware;
using TallerCardona.Models;
using TallerCardona.Repositorios;
using TallerCardona.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== Logging con Serilog =====
// Escribe en la consola y en Logs/taller-AAAAMMDD.txt (un archivo nuevo cada día,
// se guardan los últimos 30). Úsenlo en cualquier clase pidiendo ILogger<SuClase>
// en el constructor: _logger.LogInformation("Cliente {Id} creado", id);
builder.Host.UseSerilog((contexto, configuracion) => configuracion
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(contexto.HostingEnvironment.ContentRootPath, "Logs", "taller-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"));

// Filtro global AutoValidateAntiforgeryToken: TODOS los POST/PUT/DELETE de MVC exigen el
// token antiforgery (aunque alguien olvide poner [ValidateAntiForgeryToken]).
// La API no lo usa porque ApiControllerBase tiene [IgnoreAntiforgeryToken].
// JsonStringEnumConverter: los enums viajan como texto en el JSON ("Recibido", "EnProceso",
// "Efectivo"...) en vez de números, tanto en la API como en los Json() de MVC.
builder.Services.AddControllersWithViews(opciones =>
        opciones.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
    .AddJsonOptions(opciones =>
        opciones.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

// Nombre del encabezado para mandar el token en peticiones fetch/AJAX.
// En JavaScript: headers: { "RequestVerificationToken": window.tokenAntiforgery() }
builder.Services.AddAntiforgery(opciones => opciones.HeaderName = "RequestVerificationToken");

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

    // Si una ruta /api cae en la cookie (por ejemplo, una ruta /api que no existe),
    // responder 401/403 en vez de redirigir a la página de login (HTML).
    opciones.Events.OnRedirectToLogin = contexto =>
    {
        if (contexto.Request.Path.StartsWithSegments("/api"))
            contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
        else
            contexto.Response.Redirect(contexto.RedirectUri);
        return Task.CompletedTask;
    };
    opciones.Events.OnRedirectToAccessDenied = contexto =>
    {
        if (contexto.Request.Path.StartsWithSegments("/api"))
            contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
        else
            contexto.Response.Redirect(contexto.RedirectUri);
        return Task.CompletedTask;
    };
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
// Integración: PresupuestosController pide IPresupuestoService y no estaba registrado
// (la página Presupuestos fallaba al abrirse). Se registra la clase que ya existía.
builder.Services.AddScoped<IPresupuestoService, PresupuestoService>();

// ===== Módulo Pagos (Carlos) =====
builder.Services.AddScoped<IPagoRepositorio, PagoRepositorio>();
builder.Services.AddScoped<IPagoService, PagoService>();

// ===== Swagger (documentación y pruebas de la API) =====
// Disponible en /swagger, también en producción porque se usa en la demo.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Taller Cardona",
        Version = "v1",
        Description = "Primero use POST /api/auth/login, copie el token y péguelo en el botón Authorize."
    });

    // Botón "Authorize": se pega solo el token (Swagger agrega "Bearer " automáticamente).
    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pegue aquí el token que devuelve /api/auth/login."
    });
    opciones.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", documento)] = new List<string>()
    });

    // Lee los comentarios /// del código (TallerCardona.xml) para describir cada endpoint.
    var archivoXml = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    if (File.Exists(archivoXml))
        opciones.IncludeXmlComments(archivoXml);
});

var app = builder.Build();

// Crea roles y usuarios de prueba si no existen (ver Data/SeedIdentidad.cs).
await SeedIdentidad.CargarAsync(app.Services);

// ===== Manejo de errores =====
// Va primero para atrapar los errores de todo lo que viene después.
// /api -> JSON ProblemDetails; MVC -> página /Home/Error con id de seguimiento.
app.UseMiddleware<ManejoErroresMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// 404 y otros códigos sin contenido -> página amable /Home/Estado/{codigo}.
// Solo para la parte MVC: la API responde sus propios códigos (401, 404...) sin HTML.
app.UseWhen(
    contexto => !contexto.Request.Path.StartsWithSegments("/api"),
    rama => rama.UseStatusCodePagesWithReExecute("/Home/Estado/{0}"));

// ===== Encabezados de seguridad básicos (en todas las respuestas) =====
app.Use(async (contexto, siguiente) =>
{
    var encabezados = contexto.Response.Headers;
    encabezados["X-Content-Type-Options"] = "nosniff";                    // el navegador no "adivina" tipos de archivo
    encabezados["X-Frame-Options"] = "DENY";                              // nadie puede meter el sitio en un iframe
    encabezados["Referrer-Policy"] = "strict-origin-when-cross-origin";   // no filtra rutas internas a otros sitios
    await siguiente();
});

// Una línea de log por cada petición (método, ruta, código y tiempo).
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

// Swagger va ANTES de autenticación/autorización para que la página cargue sin sesión
// (los endpoints de la API siguen pidiendo token).
app.UseSwagger();
app.UseSwaggerUI(opciones =>
{
    opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "API Taller Cardona v1");
    opciones.DocumentTitle = "API Taller Cardona";
});

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
