window.Aelbry = window.Aelbry || {};

/* Tema claro/oscuro (persistido en localStorage) + inicializacion de Select2 en todos los
   <select> de la pagina, incluyendo los que se pueblan dinamicamente via fetch. */
Aelbry.ui = (function () {
    const THEME_KEY = 'aelbry_theme';

    function getTheme() {
        return localStorage.getItem(THEME_KEY) || 'light';
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem(THEME_KEY, theme);

        const icon = document.getElementById('themeToggleIcon');
        if (icon) {
            icon.textContent = theme === 'dark' ? '☀️' : '🌙';
        }
    }

    function toggleTheme() {
        applyTheme(getTheme() === 'dark' ? 'light' : 'dark');
    }

    function initSelect2(root) {
        if (!window.jQuery || !window.jQuery.fn.select2) return;

        jQuery(root || document).find('select').addBack('select').each(function () {
            const $select = jQuery(this);
            if ($select.data('select2')) {
                $select.select2('destroy');
            }
            $select.select2({ theme: 'bootstrap-5', width: 'resolve', dropdownAutoWidth: true });
        });
    }

    document.addEventListener('DOMContentLoaded', () => {
        applyTheme(getTheme());

        const toggleBtn = document.getElementById('themeToggle');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', toggleTheme);
        }

        initSelect2(document);
    });

    return { getTheme, applyTheme, toggleTheme, initSelect2 };
})();
