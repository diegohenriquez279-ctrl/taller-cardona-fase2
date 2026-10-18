using TallerCardona.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IClienteRepositorio, ClienteRepositorioMemoria>();
builder.Services.AddSingleton<IVehiculoRepositorio, VehiculoRepositorioMemoria>();
builder.Services.AddSingleton<IOrdenTrabajoRepositorio, OrdenTrabajoRepositorioMemoria>();
builder.Services.AddSingleton<IPresupuestoRepositorio, PresupuestoRepositorioMemoria>();
builder.Services.AddSingleton<IServicioRepositorio, ServicioRepositorioMemoria>();
builder.Services.AddSingleton<IRepuestoRepositorio, RepuestoRepositorioMemoria>();

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