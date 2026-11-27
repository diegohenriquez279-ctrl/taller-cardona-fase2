// Búsqueda en vivo del módulo Vehículos, con envío GET normal como alternativa.
(function () {
    'use strict';

    const input = document.getElementById('txtBusquedaVehiculos');
    const formulario = document.getElementById('formBusquedaVehiculos');
    const cuerpo = document.getElementById('cuerpoVehiculos');
    const tabla = document.getElementById('contenedorTablaVehiculos');
    const sinResultados = document.getElementById('sinResultadosVehiculos');
    if (!input || !formulario || !cuerpo || !tabla || !sinResultados) return;

    const puedeEliminar = input.dataset.puedeEliminar === 'true';
    let controlador = null;
    let temporizador = null;

    function agregarCelda(fila, texto) {
        const celda = document.createElement('td');
        celda.textContent = texto ?? '—';
        fila.appendChild(celda);
    }

    function agregarAccion(fila, url, texto, clase) {
        const enlace = document.createElement('a');
        enlace.href = url.replace('__id__', encodeURIComponent(fila.dataset.id));
        enlace.className = `btn btn-sm ${clase} me-1`;
        enlace.textContent = texto;
        fila.lastElementChild.appendChild(enlace);
    }

    function dibujar(vehiculos) {
        cuerpo.replaceChildren();
        for (const vehiculo of vehiculos) {
            const fila = document.createElement('tr');
            fila.dataset.id = vehiculo.id;
            agregarCelda(fila, vehiculo.placa);
            agregarCelda(fila, `${vehiculo.marca} ${vehiculo.modelo}`);
            agregarCelda(fila, vehiculo.anio);
            agregarCelda(fila, vehiculo.clienteNombre);
            agregarCelda(fila, `${Number(vehiculo.kilometraje).toLocaleString()} km`);

            const acciones = document.createElement('td');
            acciones.className = 'text-end text-nowrap';
            fila.appendChild(acciones);
            agregarAccion(fila, input.dataset.urlDetails, 'Ver historial', 'btn-outline-secondary');
            agregarAccion(fila, input.dataset.urlEdit, 'Editar', 'btn-outline-primary');
            if (puedeEliminar) {
                agregarAccion(fila, input.dataset.urlDelete, 'Eliminar', 'btn-outline-danger');
            }
            cuerpo.appendChild(fila);
        }

        const hayResultados = vehiculos.length > 0;
        tabla.classList.toggle('d-none', !hayResultados);
        sinResultados.classList.toggle('d-none', hayResultados);
    }

    async function buscar() {
        if (controlador) controlador.abort();
        controlador = new AbortController();
        const texto = input.value.trim();

        try {
            const url = new URL(input.dataset.urlBuscar, window.location.origin);
            if (texto) url.searchParams.set('texto', texto);
            const respuesta = await fetch(url, {
                headers: { 'Accept': 'application/json' },
                signal: controlador.signal
            });
            if (respuesta.redirected || !respuesta.ok) {
                formulario.submit();
                return;
            }

            dibujar(await respuesta.json());
            const actual = new URL(window.location.href);
            if (texto) actual.searchParams.set('busqueda', texto);
            else actual.searchParams.delete('busqueda');
            history.replaceState(null, '', actual);
        } catch (error) {
            if (error.name === 'AbortError') return;
            console.error('La búsqueda en vivo de vehículos falló:', error);
            formulario.submit();
        }
    }

    input.addEventListener('input', function () {
        clearTimeout(temporizador);
        temporizador = setTimeout(buscar, 300);
    });

    formulario.addEventListener('submit', function (evento) {
        evento.preventDefault();
        clearTimeout(temporizador);
        buscar();
    });
})();
