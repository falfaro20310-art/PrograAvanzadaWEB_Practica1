// Loader global: se muestra al navegar o enviar un formulario y bloquea la pantalla

(function () {
    var FALLBACK_MS = 15000;
    var fallbackTimer = null;

    function getLoader() {
        return document.getElementById('globalLoader');
    }

    function showLoader() {
        var loader = getLoader();
        if (!loader || loader.classList.contains('is-visible')) return;

        loader.classList.add('is-visible');

        // Seguro: si la navegacion nunca ocurre, se oculta solo
        fallbackTimer = window.setTimeout(hideLoader, FALLBACK_MS);
    }

    function hideLoader() {
        var loader = getLoader();
        if (loader) loader.classList.remove('is-visible');
        if (fallbackTimer) {
            window.clearTimeout(fallbackTimer);
            fallbackTimer = null;
        }
    }

    // Determina si un enlace realmente navega a otra pagina
    function esEnlaceNavegable(link, event) {
        var href = link.getAttribute('href');

        if (!href) return false;
        if (href.startsWith('#')) return false;
        if (href.startsWith('javascript:')) return false;
        if (href.startsWith('mailto:')) return false;
        if (href.startsWith('tel:')) return false;
        if (link.target === '_blank') return false;
        if (link.hasAttribute('download')) return false;
        if (link.hasAttribute('data-no-loader')) return false;
        if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return false;

        return true;
    }

    document.addEventListener('DOMContentLoaded', function () {

        // Clic en enlaces
        document.addEventListener('click', function (event) {
            var link = event.target.closest('a');
            if (!link) return;
            if (!esEnlaceNavegable(link, event)) return;

            showLoader();
        });

        // Envio de formularios
        document.addEventListener('submit', function (event) {
            var form = event.target;
            if (form.hasAttribute('data-no-loader')) return;

            // Si el formulario es invalido no navega, no se muestra el loader
            if (typeof form.checkValidity === 'function' && !form.checkValidity()) return;

            showLoader();
        });
    });

    // Ocultar al cargar y al regresar con el boton atras (bfcache)
    window.addEventListener('pageshow', hideLoader);
})();
