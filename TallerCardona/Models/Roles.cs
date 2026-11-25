namespace TallerCardona.Models;

// Nombres de los roles del sistema en un solo lugar.
// Úsenlos así: [Authorize(Roles = Roles.Administrador)] o User.IsInRole(Roles.Mecanico),
// para no escribir el texto a mano y equivocarse en una letra.
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Recepcion = "Recepcion";
    public const string Mecanico = "Mecanico";

    // Lista con todos los roles (la usa el seeder para crearlos).
    public static readonly string[] Todos = { Administrador, Recepcion, Mecanico };
}
