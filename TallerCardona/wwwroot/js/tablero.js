// Cambio de estado sin recargar en el tablero de órdenes (Views/Ordenes/Tablero.cshtml).
//
// Al presionar ← o → en una tarjeta, en vez de mandar el formulario y recargar toda la
// página, se llama a /Ordenes/CambiarEstadoAjax con fetch. Si el servidor acepta el cambio:
//   - la tarjeta se mueve a la columna del estado nuevo,
//   - se redibujan sus flechas (las que tocan según la nueva columna),
//   - se actualizan los números de arriba y de cada columna,
//   - se muestra el mensaje del servidor.
// Si el servidor rechaza el cambio (por ejemplo, saltar una etapa), solo se muestra el mensaje.
// Si este archivo falla o JavaScript está apagado, los formularios siguen funcionando como antes.
(function () {
    'use strict';

    const tablero = document.getElementById('tablero');
    const zonaMensaje = document.getElementById('mensajeTablero');
    if (!tablero) return; // no estamos en el tablero

    const estados = tablero.dataset.estados.split(',');   // Recibido,EnDiagnostico,...,Entregado
    const urlCambiar = tablero.dataset.urlCambiar;          // /Ordenes/CambiarEstadoAjax
    const urlBase = tablero.dataset.urlBase;                // /Ordenes (para los formularios de respaldo)
    const ultimo = estados.length - 1;

    // Muestra el mensaje arriba del tablero con el mismo estilo de los mensajes de la app.
    // Se usa textContent (no innerHTML): el texto del servidor nunca se ejecuta como HTML (evita XSS).
    function mostrarMensaje(texto, exito) {
        const alerta = document.createElement('div');
        alerta.className = 'alert ' + (exito ? 'alert-success' : 'alert-danger');
        alerta.textContent = texto;
        zonaMensaje.replaceChildren(alerta);
    }

    // Crea uno de los formularios de flecha, igual al que dibuja la vista Razor.
    // Así, aunque luego fallara el JavaScript, el botón seguiría funcionando.
    function crearFlecha(idOrden, estadoDestino, esSiguiente) {
        const form = document.createElement('form');
        form.method = 'post';
        form.action = `${urlBase}/CambiarEstado/${encodeURIComponent(idOrden)}`;

        const estado = document.createElement('input');
        estado.type = 'hidden';
        estado.name = 'estado';
        estado.value = estadoDestino;

        const token = document.createElement('input');
        token.type = 'hidden';
        token.name = '__RequestVerificationToken';
        token.value = window.tokenAntiforgery();

        const boton = document.createElement('button');
        boton.type = 'submit';
        boton.className = esSiguiente ? 'btn btn-sm btn-primary' : 'btn btn-sm btn-outline-primary';
        boton.title = esSiguiente ? 'Mover a la siguiente etapa' : 'Mover a la etapa anterior';
        boton.textContent = esSiguiente ? '→' : '←';

        form.append(estado, token, boton);
        return form;
    }

    // Deja en la tarjeta solo las flechas que tocan en su columna (misma regla que la vista):
    // ← si no es la primera ni la última columna; → si no es la última.
    function dibujarFlechas(tarjeta, indice) {
        const id = tarjeta.dataset.ordenId;
        const flechas = tarjeta.querySelector('.flechas');
        const nuevas = [];
        if (indice > 0 && indice < ultimo) nuevas.push(crearFlecha(id, estados[indice - 1], false));
        if (indice < ultimo) nuevas.push(crearFlecha(id, estados[indice + 1], true));
        flechas.replaceChildren(...nuevas);
    }

    // Mueve la tarjeta a la columna nueva, respetando el orden por fecha de ingreso.
    function moverTarjeta(tarjeta, estadoNuevo) {
        const contenedor = tablero.querySelector(`[data-columna="${estadoNuevo}"] .tarjetas`);
        const fecha = tarjeta.dataset.fecha;
        const siguiente = Array.from(contenedor.children).find(t => t.dataset.fecha > fecha);
        contenedor.insertBefore(tarjeta, siguiente || null);
        dibujarFlechas(tarjeta, estados.indexOf(estadoNuevo));
    }

    // Pone los números nuevos que manda el servidor (calculados con GroupBy en SQL Server)
    // y muestra u oculta el texto "Sin órdenes." de cada columna.
    function actualizarConteos(conteo) {
        let total = 0;
        conteo.forEach(c => {
            total += c.cantidad;
            tablero.parentElement.querySelectorAll(
                `[data-conteo-estado="${c.estado}"], [data-conteo-columna="${c.estado}"]`
            ).forEach(el => { el.textContent = c.cantidad; });

            const columna = tablero.querySelector(`[data-columna="${c.estado}"]`);
            if (columna) {
                const vacia = columna.querySelector('.tarjetas').children.length === 0;
                columna.querySelector('.sin-ordenes').classList.toggle('d-none', !vacia);
            }
        });
        const elTotal = document.querySelector('[data-conteo-total]');
        if (elTotal) elTotal.textContent = total;
    }

    // Un solo "escuchador" para todos los formularios de flecha del tablero
    // (también sirve para los que se crean después de mover una tarjeta).
    tablero.addEventListener('submit', async function (e) {
        const form = e.target;
        const tarjeta = form.closest('[data-orden-id]');
        if (!tarjeta) return;
        e.preventDefault();

        const estadoNuevo = form.querySelector('input[name="estado"]').value;
        const botones = tarjeta.querySelectorAll('.flechas button');
        botones.forEach(b => { b.disabled = true; });

        try {
            const respuesta = await fetch(urlCambiar, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json',
                    'RequestVerificationToken': window.tokenAntiforgery()
                },
                body: JSON.stringify({ id: Number(tarjeta.dataset.ordenId), estado: estadoNuevo })
            });

            // Si la sesión expiró, el servidor redirige al login: mandamos el formulario normal.
            const esJson = (respuesta.headers.get('Content-Type') || '').includes('application/json');
            if (respuesta.redirected || !esJson) {
                form.submit();
                return;
            }

            const datos = await respuesta.json();
            if (datos.exito) {
                moverTarjeta(tarjeta, estadoNuevo);
                actualizarConteos(datos.conteo);
            }
            mostrarMensaje(datos.mensaje, datos.exito);
        } catch (error) {
            console.error('Cambio de estado sin recargar falló:', error);
            form.submit(); // respaldo: el formulario de siempre
            return;
        } finally {
            tarjeta.querySelectorAll('.flechas button').forEach(b => { b.disabled = false; });
        }
    });
})();
