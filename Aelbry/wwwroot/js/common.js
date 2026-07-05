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

    /* Descarga endpoints que devuelven un archivo binario (no JSON): como el JWT vive en
       sessionStorage y no en una cookie, un <a href> normal no lo manda, asi que se hace
       fetch manual con el header Authorization y se fuerza la descarga via Blob. */
    async function downloadBlob(url, suggestedName) {
        const token = Aelbry.api.getAccessToken();
        const response = await fetch(url, { headers: token ? { Authorization: `Bearer ${token}` } : {} });
        if (!response.ok) { alert('No se pudo descargar el archivo'); return; }

        const blob = await response.blob();
        const objectUrl = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = objectUrl;
        a.download = suggestedName || 'archivo';
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(objectUrl);
    }

    document.addEventListener('DOMContentLoaded', () => {
        applyTheme(getTheme());

        const toggleBtn = document.getElementById('themeToggle');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', toggleTheme);
        }

        initSelect2(document);
    });

    return { getTheme, applyTheme, toggleTheme, initSelect2, downloadBlob };
})();

/* Proyecto activo (elegido desde una fila del feed de Inicio): fuente unica de verdad para
   que el sidebar sepa cuando mostrarse (ver _Sidebar.cshtml) y para que cada vista de
   proyecto (Kanban, Gantt, Calendario, Actividades, Documentos) se autocargue con el mismo
   proyecto sin importar por cual link del sidebar se haya navegado. */
Aelbry.projectContext = (function () {
    const ID_KEY = 'aelbry_active_project_id';
    const NAME_KEY = 'aelbry_active_project_name';

    function getId() {
        return localStorage.getItem(ID_KEY);
    }

    function getName() {
        return localStorage.getItem(NAME_KEY);
    }

    function set(id, name) {
        localStorage.setItem(ID_KEY, id);
        localStorage.setItem(NAME_KEY, name);
    }

    function clear() {
        localStorage.removeItem(ID_KEY);
        localStorage.removeItem(NAME_KEY);
    }

    /* Para usar en cada vista de proyecto: si el filtro de proyecto viene vacio, lo rellena
       con el proyecto activo y dispara loadFn. Se puede llamar directo al final del script de
       la pagina (sin envolver en su propio DOMContentLoaded): si el documento ya termino de
       parsear para cuando este script se ejecuta (ej. si tardo mas en llegar por cache/red),
       corre de inmediato en vez de esperar un evento que ya paso y nunca se volvera a disparar. */
    function autoLoad(loadFn) {
        function run() {
            const filterInput = document.getElementById('filterProjectId');
            if (!filterInput || filterInput.value) return;

            const activeId = getId();
            if (activeId) {
                filterInput.value = activeId;
                loadFn();
            }
        }

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', run);
        } else {
            run();
        }
    }

    return { getId, getName, set, clear, autoLoad };
})();
