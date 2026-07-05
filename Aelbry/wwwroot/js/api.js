window.Aelbry = window.Aelbry || {};

Aelbry.api = (function () {
    const ACCESS_KEY = 'aelbry_access_token';
    const REFRESH_KEY = 'aelbry_refresh_token';

    function getAccessToken() {
        return sessionStorage.getItem(ACCESS_KEY);
    }

    // El JWT no esta cifrado, solo firmado: se puede leer su payload en el cliente sin
    // problema (la validacion real de permisos siempre ocurre en el servidor via
    // [Authorize(Policy = "Permission:X")]). Esto solo sirve para decidir que mostrar en la UI.
    function parseJwt(token) {
        try {
            const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
            const json = decodeURIComponent(atob(base64).split('').map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join(''));
            return JSON.parse(json);
        } catch {
            return null;
        }
    }

    function getPermissions() {
        const token = getAccessToken();
        if (!token) return [];

        const payload = parseJwt(token);
        const claim = payload?.permission;
        if (!claim) return [];

        return Array.isArray(claim) ? claim : [claim];
    }

    function hasPermission(code) {
        return getPermissions().includes(code);
    }

    // Los claims estandar (NameIdentifier, Email, GivenName, Surname, Role) los serializa
    // JwtSecurityTokenHandler con sus URIs largas de siempre (no los nombres cortos); los
    // custom (company_id, permission) sí se quedan tal cual como los definimos en el backend.
    const CLAIM_NAME_ID = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
    const CLAIM_EMAIL = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
    const CLAIM_GIVEN_NAME = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname';
    const CLAIM_SURNAME = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname';
    const CLAIM_ROLE = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

    function getCurrentUser() {
        const token = getAccessToken();
        if (!token) return null;

        const payload = parseJwt(token);
        if (!payload) return null;

        const role = payload[CLAIM_ROLE];

        return {
            userId: parseInt(payload[CLAIM_NAME_ID], 10),
            companyId: parseInt(payload.company_id, 10),
            fullName: `${payload[CLAIM_GIVEN_NAME] ?? ''} ${payload[CLAIM_SURNAME] ?? ''}`.trim(),
            email: payload[CLAIM_EMAIL],
            roles: Array.isArray(role) ? role : (role ? [role] : []),
        };
    }

    function getRefreshToken() {
        return localStorage.getItem(REFRESH_KEY);
    }

    function setSession(auth) {
        sessionStorage.setItem(ACCESS_KEY, auth.accessToken);
        localStorage.setItem(REFRESH_KEY, auth.refreshToken);
    }

    function clearSession() {
        sessionStorage.removeItem(ACCESS_KEY);
        localStorage.removeItem(REFRESH_KEY);
    }

    async function refreshAccessToken() {
        const refreshToken = getRefreshToken();
        if (!refreshToken) return false;

        const response = await fetch('/Auth/Refresh', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refreshToken }),
        });

        const payload = await response.json();
        if (payload.result !== 'OK') {
            clearSession();
            return false;
        }

        setSession(payload.data);
        return true;
    }

    async function request(url, options = {}, retry = true) {
        const isFormData = options.body instanceof FormData;
        const headers = Object.assign(isFormData ? {} : { 'Content-Type': 'application/json' }, options.headers || {});
        const token = getAccessToken();
        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        const response = await fetch(url, Object.assign({}, options, { headers }));

        if (response.status === 401 && retry) {
            const refreshed = await refreshAccessToken();
            if (refreshed) {
                return request(url, options, false);
            }
            window.location.href = '/Auth/Login';
            return null;
        }

        return response.json();
    }

    async function login(email, password) {
        const response = await fetch('/Auth/Login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password }),
        });

        const payload = await response.json();
        if (payload.result === 'OK') {
            setSession(payload.data);
        }

        return payload;
    }

    async function logout() {
        const refreshToken = getRefreshToken();
        clearSession();

        if (!refreshToken) return;

        await request('/Auth/Logout', {
            method: 'POST',
            body: JSON.stringify({ refreshToken }),
        });
    }

    return {
        get: (url) => request(url, { method: 'GET' }),
        post: (url, body) => request(url, { method: 'POST', body: JSON.stringify(body) }),
        postForm: (url, formData) => request(url, { method: 'POST', body: formData }),
        login,
        logout,
        isAuthenticated: () => !!getAccessToken(),
        getAccessToken,
        getPermissions,
        hasPermission,
        getCurrentUser,
    };
})();
