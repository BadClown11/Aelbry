// Inicializar iconos Lucide en toda la pantalla
lucide.createIcons();

// 1. Manejo del Tema Claro/Oscuro dinámico
const themeToggle = document.getElementById('themeToggle');
const sunIcon = document.getElementById('theme-icon-dark');
const moonIcon = document.getElementById('theme-icon-light');

function updateThemeUI(theme) {
    if (theme === 'dark') {
        document.documentElement.classList.add('dark');
        document.documentElement.setAttribute('data-theme', 'dark');
        sunIcon.classList.remove('d-none');
        moonIcon.classList.add('d-none');
    } else {
        document.documentElement.classList.remove('dark');
        document.documentElement.setAttribute('data-theme', 'light');
        sunIcon.classList.add('d-none');
        moonIcon.classList.remove('d-none');
    }
}

const initialTheme = localStorage.getItem('aelbry_theme') || 'light';
updateThemeUI(initialTheme);

themeToggle.addEventListener('click', () => {
    const currentTheme = document.documentElement.classList.contains('dark') ? 'dark' : 'light';
    const newTheme = currentTheme === 'light' ? 'dark' : 'light';
    localStorage.setItem('aelbry_theme', newTheme);
    updateThemeUI(newTheme);
});

// 2. Control interactivo para revelar la Contraseña
const passwordInput = document.getElementById('password');
const togglePasswordBtn = document.getElementById('togglePasswordBtn');
const eyeIcon = document.getElementById('eyeIcon');
let showPassword = false;

togglePasswordBtn.addEventListener('click', () => {
    showPassword = !showPassword;
    if (showPassword) {
        passwordInput.type = 'text';
        eyeIcon.setAttribute('data-lucide', 'eye-off');
    } else {
        passwordInput.type = 'password';
        eyeIcon.setAttribute('data-lucide', 'eye');
    }
    lucide.createIcons();
});

// 3. Envío seguro AJAX y manejo de spinners para el JWT
document.getElementById('loginForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    const errorBox = document.getElementById('loginError');
    const errorMessage = document.getElementById('errorMessage');
    const submitBtn = document.getElementById('submitBtn');
    const submitText = document.getElementById('submitText');
    const submitIcon = document.getElementById('submitIcon');

    errorBox.classList.add('d-none');

    // Feedback visual: Bloquear botón de envío y cambiar ícono a Spinner
    submitBtn.disabled = true;
    submitText.textContent = "Autenticando...";
    submitIcon.setAttribute('data-lucide', 'loader');
    submitIcon.classList.add('animate-spin');
    lucide.createIcons();

    try {
        // Envío de credenciales a tu controlador de .NET 8 (ADO.NET + JWT)
        const result = await Aelbry.api.login(email, password);

        if (result.result === 'OK') {
            window.location.href = '/Home/Index';
        } else {
            errorMessage.textContent = result.result;
            errorBox.classList.remove('d-none');
            resetSubmitButton();
        }
    } catch (err) {
        errorMessage.textContent = "Falla de comunicación. Intenta nuevamente o contacta a soporte.";
        errorBox.classList.remove('d-none');
        resetSubmitButton();
    }
});

function resetSubmitButton() {
    const submitBtn = document.getElementById('submitBtn');
    const submitText = document.getElementById('submitText');
    const submitIcon = document.getElementById('submitIcon');

    submitBtn.disabled = false;
    submitText.textContent = "Ingresar al workspace";
    submitIcon.setAttribute('data-lucide', 'arrow-right');
    submitIcon.classList.remove('animate-spin');
    lucide.createIcons();
}