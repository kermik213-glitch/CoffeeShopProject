const API_BASE = '/api';

const API = {
    get token() { return localStorage.getItem('token'); },
    get user() {
        const raw = localStorage.getItem('user');
        return raw ? JSON.parse(raw) : null;
    },
    setAuth(auth) {
        localStorage.setItem('token', auth.token);
        localStorage.setItem('user', JSON.stringify(auth));
    },
    logout() {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        window.location.href = 'index.html';
    },
    async request(path, options = {}) {
        const headers = options.headers || {};
        headers['Content-Type'] = 'application/json';
        if (this.token) headers['Authorization'] = `Bearer ${this.token}`;
        const res = await fetch(API_BASE + path, { ...options, headers });
        if (res.status === 401) { this.logout(); throw new Error('Необходима авторизация'); }
        if (!res.ok) {
            const text = await res.text();
            throw new Error(text || `Ошибка ${res.status}`);
        }
        const ct = res.headers.get('content-type') || '';
        return ct.includes('application/json') ? res.json() : res.text();
    },
    get(path) { return this.request(path); },
    post(path, body) { return this.request(path, { method: 'POST', body: JSON.stringify(body) }); },
    put(path, body) { return this.request(path, { method: 'PUT', body: JSON.stringify(body) }); },
    del(path) { return this.request(path, { method: 'DELETE' }); }
};

function updateNav() {
    const user = API.user;
    const navUser = document.getElementById('nav-user');
    const navLogin = document.getElementById('nav-login');
    const navLogout = document.getElementById('nav-logout');
    const navAdmin = document.getElementById('nav-admin');

    if (user) {
        if (navUser) navUser.textContent = `👤 ${user.name} (${user.role})`;
        if (navLogin) navLogin.style.display = 'none';
        if (navLogout) navLogout.style.display = 'inline';
        if (navAdmin) navAdmin.style.display = user.role === 'Admin' ? 'inline' : 'none';
    } else {
        if (navUser) navUser.textContent = '';
        if (navLogin) navLogin.style.display = 'inline';
        if (navLogout) navLogout.style.display = 'none';
        if (navAdmin) navAdmin.style.display = 'none';
    }
    const logout = document.getElementById('nav-logout');
    if (logout) logout.onclick = (e) => { e.preventDefault(); API.logout(); };
}

document.addEventListener('DOMContentLoaded', updateNav);