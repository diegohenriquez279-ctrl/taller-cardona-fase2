using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace TallerCardona.Services;

// Genera los tokens JWT que usa la API (la parte MVC usa cookie, no esto).
// Lee la configuración de la sección "Jwt" (Issuer, Audience, ExpiraMinutos y Key).
public class TokenService
{
    private readonly IConfiguration _configuracion;

    public TokenService(IConfiguration configuracion)
    {
        _configuracion = configuracion;
    }

    // Crea un token para el usuario con sus roles. Devuelve el texto del token y cuándo expira.
    public (string Token, DateTime Expira) Generar(IdentityUser usuario, IEnumerable<string> roles)
    {
        var expira = DateTime.UtcNow.AddMinutes(_configuracion.GetValue("Jwt:ExpiraMinutos", 60));

        // "Claims": datos del usuario que viajan dentro del token.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id),
            new(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, usuario.UserName ?? string.Empty),
        };
        // Cada rol va como un claim de tipo Role: así funcionan RequireRole y User.IsInRole.
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var credenciales = new SigningCredentials(ObtenerClave(_configuracion), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuracion["Jwt:Issuer"],
            audience: _configuracion["Jwt:Audience"],
            claims: claims,
            expires: expira,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }

    // Clave para firmar y validar tokens. La usa también Program.cs al configurar JwtBearer.
    public static SymmetricSecurityKey ObtenerClave(IConfiguration configuracion)
    {
        var clave = configuracion["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(clave) || clave.Length < 32)
            throw new InvalidOperationException(
                "Falta la clave Jwt:Key (mínimo 32 caracteres). En desarrollo está en appsettings.Development.json; " +
                "en producción configure la variable de entorno Jwt__Key.");

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave));
    }
}
