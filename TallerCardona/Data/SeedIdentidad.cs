using Microsoft.AspNetCore.Identity;
using TallerCardona.Models;

namespace TallerCardona.Data;

// Crea los roles y los usuarios de prueba al arrancar la aplicación.
//
// ¿Por qué no con HasData como DatosIniciales? Porque las contraseñas deben
// guardarse cifradas (hash) y eso lo hace UserManager. Con HasData tendríamos
// que escribir el hash a mano en la migración.
//
// Es seguro ejecutarlo cada vez que arranca: solo crea lo que todavía no existe.
public static class SeedIdentidad
{
    // Usuarios de prueba: (correo, contraseña, rol)
    private static readonly (string Correo, string Clave, string Rol)[] UsuariosPrueba =
    {
        ("admin@tallercardona.com", "Admin123!", Roles.Administrador),
        ("recepcion@tallercardona.com", "Recepcion123!", Roles.Recepcion),
        ("mecanico@tallercardona.com", "Mecanico123!", Roles.Mecanico),
    };

    public static async Task CargarAsync(IServiceProvider servicios)
    {
        // Los servicios de Identity son "scoped": se crea un scope temporal para usarlos.
        using var scope = servicios.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("SeedIdentidad");

        // 1) Roles
        foreach (var rol in Roles.Todos)
        {
            if (!await roleManager.RoleExistsAsync(rol))
            {
                await roleManager.CreateAsync(new IdentityRole(rol));
                logger.LogInformation("Rol creado: {Rol}", rol);
            }
        }

        // 2) Usuarios
        foreach (var (correo, clave, rol) in UsuariosPrueba)
        {
            var usuario = await userManager.FindByEmailAsync(correo);
            if (usuario is null)
            {
                usuario = new IdentityUser
                {
                    UserName = correo,
                    Email = correo,
                    EmailConfirmed = true
                };

                var resultado = await userManager.CreateAsync(usuario, clave);
                if (!resultado.Succeeded)
                {
                    var errores = string.Join("; ", resultado.Errors.Select(e => e.Description));
                    logger.LogError("No se pudo crear el usuario {Correo}: {Errores}", correo, errores);
                    continue;
                }
                logger.LogInformation("Usuario creado: {Correo}", correo);
            }

            if (!await userManager.IsInRoleAsync(usuario, rol))
                await userManager.AddToRoleAsync(usuario, rol);
        }
    }
}
