(function () {
    'use strict';

    window.mostrarModal = function (titulo, mensaje, tipo = 'info') {
        // Tipos: success, error, warning, info
        const iconos = {
            success: 'fa-check-circle text-success',
            error: 'fa-exclamation-triangle text-danger',
            warning: 'fa-exclamation-triangle text-warning',
            info: 'fa-info-circle text-primary'
        };

        const colores = {
            success: 'bg-success',
            error: 'bg-danger',
            warning: 'bg-warning',
            info: 'bg-primary'
        };

        const icono = iconos[tipo] || iconos.info;
        const color = colores[tipo] || colores.info;

        const modalHtml = `
            <div class="modal fade" id="modalMensaje" tabindex="-1" role="dialog" data-backdrop="static">
                <div class="modal-dialog modal-dialog-centered" role="document">
                    <div class="modal-content">
                        <div class="modal-header ${color} text-white">
                            <h5 class="modal-title">
                                <i class="fas ${icono}"></i> ${titulo}
                            </h5>
                            <button type="button" class="close text-white" data-dismiss="modal">
                                <span>&times;</span>
                            </button>
                        </div>
                        <div class="modal-body">
                            <p class="mb-0">${mensaje}</p>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-primary" data-dismiss="modal">Aceptar</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Eliminar modal anterior si existe
        $('#modalMensaje').remove();

        // Agregar el nuevo modal al body
        $('body').append(modalHtml);

        // Mostrar el modal
        $('#modalMensaje').modal('show');

        // Limpiar el modal cuando se cierre
        $('#modalMensaje').on('hidden.bs.modal', function () {
            $(this).remove();
        });
    };

    // Función para modal de confirmación
    window.mostrarConfirmacion = function (titulo, mensaje, callbackSi, callbackNo) {
        const modalHtml = `
            <div class="modal fade" id="modalConfirmacion" tabindex="-1" role="dialog" data-backdrop="static">
                <div class="modal-dialog modal-dialog-centered" role="document">
                    <div class="modal-content">
                        <div class="modal-header bg-warning text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-question-circle"></i> ${titulo}
                            </h5>
                            <button type="button" class="close text-white" data-dismiss="modal">
                                <span>&times;</span>
                            </button>
                        </div>
                        <div class="modal-body">
                            <p class="mb-0">${mensaje}</p>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" id="btnNo">No</button>
                            <button type="button" class="btn btn-primary" id="btnSi">Sí</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Eliminar modal anterior si existe
        $('#modalConfirmacion').remove();

        // Agregar el nuevo modal al body
        $('body').append(modalHtml);

        // Configurar eventos
        $('#btnSi').on('click', function () {
            $('#modalConfirmacion').modal('hide');
            if (typeof callbackSi === 'function') {
                callbackSi();
            }
        });

        $('#btnNo').on('click', function () {
            $('#modalConfirmacion').modal('hide');
            if (typeof callbackNo === 'function') {
                callbackNo();
            }
        });

        // Mostrar el modal
        $('#modalConfirmacion').modal('show');

        // Limpiar el modal cuando se cierre
        $('#modalConfirmacion').on('hidden.bs.modal', function () {
            $(this).remove();
        });
    };

    // Función para modal de eliminación (caso específico)
    window.mostrarModalEliminar = function (titulo, mensaje, datos, onConfirm) {
        const modalHtml = `
            <div class="modal fade" id="modalEliminar" tabindex="-1" role="dialog" data-backdrop="static">
                <div class="modal-dialog modal-dialog-centered" role="document">
                    <div class="modal-content">
                        <div class="modal-header bg-danger text-white">
                            <h5 class="modal-title">
                                <i class="fas fa-exclamation-triangle"></i> ${titulo}
                            </h5>
                            <button type="button" class="close text-white" data-dismiss="modal">
                                <span>&times;</span>
                            </button>
                        </div>
                        <div class="modal-body">
                            <p>${mensaje}</p>
                            ${datos ? `<p class="mb-0"><strong>${datos}</strong></p>` : ''}
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-dismiss="modal">No</button>
                            <button type="button" class="btn btn-danger" id="btnConfirmarEliminar">Sí</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Eliminar modal anterior si existe
        $('#modalEliminar').remove();

        // Agregar el nuevo modal al body
        $('body').append(modalHtml);

        // Configurar evento de confirmación
        $('#btnConfirmarEliminar').on('click', function () {
            $('#modalEliminar').modal('hide');
            if (typeof onConfirm === 'function') {
                onConfirm();
            }
        });

        // Mostrar el modal
        $('#modalEliminar').modal('show');

        // Limpiar el modal cuando se cierre
        $('#modalEliminar').on('hidden.bs.modal', function () {
            $(this).remove();
        });
    };

})();