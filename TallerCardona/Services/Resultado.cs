namespace TallerCardona.Services;

// Respuesta de los servicios al crear, editar o eliminar:
// indica si la operación salió bien y el mensaje para mostrar al usuario.
public class Resultado
{
    public bool Exito { get; private set; }
    public string Mensaje { get; private set; } = string.Empty;

    public static Resultado Ok(string mensaje) => new() { Exito = true, Mensaje = mensaje };

    public static Resultado Error(string mensaje) => new() { Exito = false, Mensaje = mensaje };
}
