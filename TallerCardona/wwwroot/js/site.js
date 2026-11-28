// Funciones comunes de JavaScript para todo el sitio (se carga en _Layout.cshtml).

// Devuelve el token antiforgery que _Layout deja en <meta name="token-antiforgery">.
// Úsenlo en todo fetch que haga POST, PUT o DELETE a un controlador MVC:
//
//   fetch('/Ordenes/CambiarEstado', {
//       method: 'POST',
//       headers: {
//           'Content-Type': 'application/json',
//           'RequestVerificationToken': window.tokenAntiforgery()
//       },
//       body: JSON.stringify({ id: 5, estado: 'Terminada' })
//   });
//
// Sin este encabezado el servidor responde 400 (filtro global AutoValidateAntiforgeryToken).
// Las llamadas a /api NO lo necesitan: usan el token JWT.
window.tokenAntiforgery = function () {
    const meta = document.querySelector('meta[name="token-antiforgery"]');
    return meta ? meta.content : '';
};
