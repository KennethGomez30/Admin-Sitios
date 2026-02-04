/**
 * Sistema de manejo automático de alertas
 * - Auto-oculta alertas después de 4 segundos
 * - Permite cierre manual
 * - Animaciones suaves
 */

(function () {
    'use strict';

    // Configuración
    const CONFIG = {
        AUTO_HIDE_DELAY: 3000, // 4 segundos
        FADE_OUT_DURATION: 500 // 0.5 segundos
    };

    // Inicializar cuando el DOM esté listo
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initAlertas);
    } else {
        initAlertas();
    }

    function initAlertas() {
        // Seleccionar todas las alertas con Bootstrap
        const alertas = document.querySelectorAll('.alert:not(.alert-light)');

        alertas.forEach(function (alerta) {
            // Solo procesar alertas que NO sean informativas permanentes
            if (!alerta.classList.contains('alert-light')) {
                configurarAlerta(alerta);
            }
        });
    }

    function configurarAlerta(alerta) {
        // Agregar evento de cierre manual si tiene botón close
        const btnCerrar = alerta.querySelector('.close');
        if (btnCerrar) {
            btnCerrar.addEventListener('click', function (e) {
                e.preventDefault();
                cerrarAlerta(alerta, true);
            });
        }

        // Auto-ocultar después del delay configurado
        setTimeout(function () {
            cerrarAlerta(alerta, false);
        }, CONFIG.AUTO_HIDE_DELAY);
    }

    function cerrarAlerta(alerta, esManual) {
        // Verificar que la alerta aún exista en el DOM
        if (!alerta || !alerta.parentNode) {
            return;
        }

        // Agregar clase de fade-out
        alerta.style.transition = `opacity ${CONFIG.FADE_OUT_DURATION}ms ease-out`;
        alerta.style.opacity = '0';

        // Remover del DOM después de la animación
        setTimeout(function () {
            if (alerta && alerta.parentNode) {
                alerta.remove();
            }
        }, CONFIG.FADE_OUT_DURATION);
    }

    // Exponer función para cerrar alertas programáticamente
    window.cerrarAlerta = function (alertaId) {
        const alerta = document.getElementById(alertaId);
        if (alerta) {
            cerrarAlerta(alerta, true);
        }
    };

})();
