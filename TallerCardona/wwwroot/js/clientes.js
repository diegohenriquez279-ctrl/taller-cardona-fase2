// Búsqueda en vivo de clientes (Views/Clientes/Index.cshtml).
//
// Mientras el usuario escribe, pide /Clientes/Buscar?texto=... con fetch y vuelve a
// dibujar SOLO el <tbody> de la tabla, sin recargar la página.
// Si este archivo falla o JavaScript está apagado, el formulario normal sigue funcionando.
(function () {
    'use strict';

    const input = document.getElementById('txtBusqueda');
    const formulario = document.getElementById('formBusqueda');
    const tabla = document.getElementById('tablaClientes');
    const cuerpo = document.getElementById('cuerpoClientes');
    const sinResultados = document.getElementById('sinResultados');
    if (!input || !cuerpo) return; // no estamos en la página de clientes

    const urlBuscar = input.dataset.urlBuscar;        // /Clientes/Buscar
    const urlBase = input.dataset.urlBase;            // /Clientes (para armar Ver/Editar/Eliminar)
    const puedeEliminar = input.dataset.puedeEliminar === 'true';

    // Escapa caracteres especiales de HTML. SIEMPRE úsenla antes de meter texto que viene
    // del servidor dentro de innerHTML: si un cliente se llama "<script>...", se muestra
    // como texto y no se ejecuta (evita ataques XSS).
    function escapar(texto) {
        return String(texto ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    // Arma el HTML de una fila. Los nombres de propiedad llegan en camelCase (nombre, telefono...).
    function filaHtml(c) {
        const id = encodeURIComponent(c.id);
        let acciones =
            `<a href="${urlBase}/Details/${id}" class="btn btn-sm btn-outline-secondary">Ver</a> ` +
            `<a href="${urlBase}/Edit/${id}" class="btn btn-sm btn-outline-primary">Editar</a>`;
        if (puedeEliminar) {
            acciones += ` <a href="${urlBase}/Delete/${id}" class="btn btn-sm btn-outline-danger">Eliminar</a>`;
        }
        return `<tr>
            <td>${escapar(c.nombre)}</td>
            <td>${escapar(c.telefono)}</td>
            <td>${escapar(c.correo || '—')}</td>
            <td class="text-center">${escapar(c.cantidadVehiculos)}</td>
            <td class="text-end">${acciones}</td>
        </tr>`;
    }

    // Redibuja la tabla con la lista recibida.
    function dibujar(clientes) {
        cuerpo.innerHTML = clientes.map(filaHtml).join('');
        const hay = clientes.length > 0;
        tabla.classList.toggle('d-none', !hay);
        sinResultados.classList.toggle('d-none', hay);
    }

    // Para descartar respuestas viejas si el usuario sigue escribiendo.
    let controlador = null;

    async function buscar() {
        const texto = input.value.trim();
        if (controlador) controlador.abort();
        controlador = new AbortController();

        try {
            const respuesta = await fetch(`${urlBuscar}?texto=${encodeURIComponent(texto)}`, {
                headers: { 'Accept': 'application/json' },
                signal: controlador.signal
            });
            // Si la sesión expiró, el servidor redirige al login: recargamos para que se vea.
            if (respuesta.redirected || !respuesta.ok) {
                formulario.submit();
                return;
            }
            dibujar(await respuesta.json());

            // Actualiza la dirección (?busqueda=...) para que F5 conserve el filtro.
            const url = new URL(window.location.href);
            if (texto) url.searchParams.set('busqueda', texto); else url.searchParams.delete('busqueda');
            history.replaceState(null, '', url);
        } catch (error) {
            if (error.name !== 'AbortError') console.error('Búsqueda en vivo falló:', error);
        }
    }

    // "Debounce": espera 300 ms sin teclear antes de buscar (no una petición por letra).
    let temporizador = null;
    input.addEventListener('input', function () {
        clearTimeout(temporizador);
        temporizador = setTimeout(buscar, 300);
    });

    // Enter o el botón Buscar: buscar al instante sin recargar.
    formulario.addEventListener('submit', function (e) {
        e.preventDefault();
        clearTimeout(temporizador);
        buscar();
    });
})();
