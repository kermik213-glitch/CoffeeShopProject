document.addEventListener('DOMContentLoaded', () => {
    const tabs = document.querySelectorAll('.tab');
    const loginForm = document.getElementById('login-form');
    const registerForm = document.getElementById('register-form');
    const errorEl = document.getElementById('auth-error');

    tabs.forEach(tab => {
        tab.onclick = () => {
            tabs.forEach(t => t.classList.remove('active'));
            tab.classList.add('active');
            const isLogin = tab.dataset.tab === 'login';
            loginForm.style.display = isLogin ? 'block' : 'none';
            registerForm.style.display = isLogin ? 'none' : 'block';
            errorEl.textContent = '';
        };
    });

    loginForm.onsubmit = async (e) => {
        e.preventDefault();
        const fd = new FormData(loginForm);
        try {
            const auth = await API.post('/auth/login', {
                email: fd.get('email'), password: fd.get('password')
            });
            API.setAuth(auth);
            window.location.href = 'index.html';
        } catch (err) { errorEl.textContent = err.message; }
    };

    registerForm.onsubmit = async (e) => {
        e.preventDefault();
        const fd = new FormData(registerForm);
        try {
            const auth = await API.post('/auth/register', {
                email: fd.get('email'), password: fd.get('password'), name: fd.get('name')
            });
            API.setAuth(auth);
            window.location.href = 'index.html';
        } catch (err) { errorEl.textContent = err.message; }
    };
});