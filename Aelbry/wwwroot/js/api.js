window.Aelbry = window.Aelbry || {};

Aelbry.api = (function () {
    const ACCESS_KEY = 'aelbry_access_token';
    const REFRESH_KEY = 'aelbry_refresh_token';

    function getAccessToken() {
        return sessionStorage.getItem(ACCESS_KEY);
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
        const headers = Object.assign({ 'Content-Type': 'application/json' }, options.headers || {});
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
        login,
        logout,
        isAuthenticated: () => !!getAccessToken(),
    };
})();
