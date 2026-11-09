let salesChart, topChart;
let _adminProducts = [];

document.addEventListener('DOMContentLoaded', () => {
    if (!API.user || API.user.role !== 'Admin') {
        alert('Доступ только для администратора');
        window.location.href = 'index.html';
        return;
    }

    document.querySelectorAll('.admin-tab').forEach(tab => {
        tab.onclick = () => {
            document.querySelectorAll('.admin-tab').forEach(t => t.classList.remove('active'));
            document.querySelectorAll('.admin-section').forEach(s => s.style.display = 'none');
            tab.classList.add('active');
            document.getElementById('tab-' + tab.dataset.tab).style.display = 'block';
            if (tab.dataset.tab === 'stats') loadStats();
            if (tab.dataset.tab === 'products') loadProducts();
            if (tab.dataset.tab === 'blog') loadBlog();
            if (tab.dataset.tab === 'orders') loadOrders();
            if (tab.dataset.tab === 'users') loadUsers(); 
        };
    });

    const pf = document.getElementById('product-form');
    pf.onsubmit = async (e) => {
        e.preventDefault();
        const fd = new FormData(pf);
        const dto = {
            id: Number(fd.get('id') || 0),
            title: fd.get('title'),
            category: fd.get('category'),
            price: Number(fd.get('price')),
            imageUrl: fd.get('imageUrl') || '',
            description: fd.get('description') || '',
            isActive: fd.get('isActive') === 'on'
        };
        try {
            if (dto.id) await API.put(`/products/${dto.id}`, dto);
            else await API.post('/products', dto);
            pf.reset(); pf.id.value = '';
            loadProducts();
        } catch (err) { alert(err.message); }
    };
    document.getElementById('reset-product').onclick = () => { pf.reset(); pf.id.value = ''; };

    const bf = document.getElementById('blog-form');
    bf.onsubmit = async (e) => {
        e.preventDefault();
        const fd = new FormData(bf);
        const dto = {
            id: Number(fd.get('id') || 0),
            title: fd.get('title'),
            content: fd.get('content'),
            imageUrl: fd.get('imageUrl') || ''
        };
        try {
            if (dto.id) await API.put(`/blog/${dto.id}`, dto);
            else await API.post('/blog', dto);
            bf.reset(); bf.id.value = '';
            loadBlog();
        } catch (err) { alert(err.message); }
    };
    document.getElementById('reset-blog').onclick = () => { bf.reset(); bf.id.value = ''; };

    loadStats();
});

async function loadStats() {
    try {
        const [summary, sales, top] = await Promise.all([
            API.get('/stats/summary'),
            API.get('/stats/sales-by-day?days=14'),
            API.get('/stats/top-products?top=5')
        ]);

        document.getElementById('stats-cards').innerHTML = `
            <div class="stat-card"><div class="stat-value">${summary.totalOrders}</div><div class="stat-label">Заказов</div></div>
            <div class="stat-card"><div class="stat-value">${summary.totalRevenue.toFixed(2)} ₽</div><div class="stat-label">Выручка</div></div>
            <div class="stat-card"><div class="stat-value">${summary.avgOrderValue.toFixed(2)} ₽</div><div class="stat-label">Средний чек</div></div>
            <div class="stat-card"><div class="stat-value">${summary.totalUsers}</div><div class="stat-label">Пользователей</div></div>
            <div class="stat-card"><div class="stat-value">${summary.totalProducts}</div><div class="stat-label">Товаров</div></div>
        `;

        if (salesChart) salesChart.destroy();
        salesChart = new Chart(document.getElementById('sales-chart'), {
            type: 'line',
            data: {
                labels: sales.map(s => s.date),
                datasets: [{
                    label: 'Выручка, ₽',
                    data: sales.map(s => s.revenue),
                    borderColor: '#6f4e37',
                    backgroundColor: 'rgba(111,78,55,0.15)',
                    fill: true, tension: 0.3
                }]
            },
            options: { plugins: { title: { display: true, text: 'Продажи за 14 дней' } } }
        });

        if (topChart) topChart.destroy();
        topChart = new Chart(document.getElementById('top-chart'), {
            type: 'bar',
            data: {
                labels: top.map(t => t.title),
                datasets: [{
                    label: 'Продано, шт',
                    data: top.map(t => t.quantity),
                    backgroundColor: '#8c766b'
                }]
            },
            options: { plugins: { title: { display: true, text: 'Топ-5 товаров' } } }
        });
    } catch (e) { console.error(e); }
}

async function loadProducts() {
    _adminProducts = await API.get('/products?includeInactive=true');
    const tbody = document.querySelector('#products-table tbody');
    tbody.innerHTML = _adminProducts.map((p, i) => `
        <tr>
            <td>${p.id}</td>
            <td>${p.title}</td>
            <td>${p.category}</td>
            <td>${p.price} ₽</td>
            <td>${p.isActive ? '✅' : '❌'}</td>
            <td>
                <button data-edit="${i}">✏️</button>
                <button data-del="${p.id}">🗑</button>
            </td>
        </tr>
    `).join('');
    tbody.querySelectorAll('[data-edit]').forEach(b =>
        b.onclick = () => editProduct(_adminProducts[+b.dataset.edit]));
    tbody.querySelectorAll('[data-del]').forEach(b =>
        b.onclick = () => deleteProduct(+b.dataset.del));
}

function editProduct(p) {
    const f = document.getElementById('product-form');
    f.id.value = p.id;
    f.title.value = p.title;
    f.category.value = p.category;
    f.price.value = p.price;
    f.imageUrl.value = p.imageUrl;
    f.description.value = p.description;
    f.isActive.checked = p.isActive;
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

async function deleteProduct(id) {
    if (!confirm('Деактивировать товар?')) return;
    await API.del(`/products/${id}`);
    loadProducts();
}

async function loadBlog() {
    const posts = await API.get('/blog');
    const tbody = document.querySelector('#blog-table tbody');
    tbody.innerHTML = posts.map((p, i) => `
        <tr>
            <td>${p.id}</td>
            <td>${p.title}</td>
            <td>${p.author}</td>
            <td>${new Date(p.createdAt).toLocaleDateString('ru-RU')}</td>
            <td>
                <button data-edit="${i}">✏️</button>
                <button data-del="${p.id}">🗑</button>
            </td>
        </tr>
    `).join('');
    tbody.querySelectorAll('[data-edit]').forEach(b =>
        b.onclick = () => {
            const p = posts[+b.dataset.edit];
            const f = document.getElementById('blog-form');
            f.id.value = p.id; f.title.value = p.title;
            f.content.value = p.content; f.imageUrl.value = p.imageUrl;
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    tbody.querySelectorAll('[data-del]').forEach(b =>
        b.onclick = async () => {
            if (!confirm('Удалить пост?')) return;
            await API.del(`/blog/${+b.dataset.del}`);
            loadBlog();
        });
}

async function loadOrders() {
    const orders = await API.get('/orders');
    const tbody = document.querySelector('#orders-table tbody');
    const statuses = ['New', 'Preparing', 'Ready', 'Completed', 'Cancelled'];
    tbody.innerHTML = orders.map(o => `
        <tr>
            <td>${o.id}</td>
            <td><code>${o.receiptNumber}</code></td>
            <td>${new Date(o.createdAt).toLocaleString('ru-RU')}</td>
            <td>${o.totalPrice.toFixed(2)} ₽</td>
            <td>
                <select data-id="${o.id}">
                    ${statuses.map(s => `<option value="${s}" ${s === o.status ? 'selected' : ''}>${s}</option>`).join('')}
                </select>
            </td>
            <td>${o.appliedStickers || '—'}</td>
            <td><button data-receipt="${o.id}">🧾 Чек</button></td>
        </tr>
    `).join('');

    tbody.querySelectorAll('select[data-id]').forEach(sel =>
        sel.onchange = () => API.put(`/orders/${sel.dataset.id}/status`, sel.value));
    tbody.querySelectorAll('button[data-receipt]').forEach(btn =>
        btn.onclick = async () => {
            const text = await API.get(`/orders/${btn.dataset.receipt}/receipt`);
            const w = window.open('', '_blank');
            w.document.write(`<pre style="font-family:monospace;font-size:14px;padding:20px">${text.replace(/</g,'&lt;')}</pre>`);
            w.document.title = `Чек #${btn.dataset.receipt}`;
        });
}
async function loadUsers() {
    try {
        const users = await API.get('/users');
        const tbody = document.querySelector('#users-table tbody');
        
        if (!tbody) {
            console.error("Таблица #users-table не найдена в HTML!");
            return;
        }

        tbody.innerHTML = users.map(u => `
            <tr>
                <td>${u.id}</td>
                <td>${u.name}</td>
                <td>${u.email}</td>
                <td>
                    <select data-uid="${u.id}">
                        <option value="User" ${u.role === 'User' ? 'selected' : ''}>User</option>
                        <option value="Admin" ${u.role === 'Admin' ? 'selected' : ''}>Admin</option>
                    </select>
                </td>
                <td>${new Date(u.createdAt).toLocaleDateString('ru-RU')}</td>
            </tr>
        `).join('');

        tbody.querySelectorAll('select[data-uid]').forEach(sel =>
            sel.onchange = async () => {
                await API.put(`/users/${sel.dataset.uid}/role`, sel.value);
                alert('Роль пользователя обновлена!');
            });
    } catch (e) { 
        console.error("Ошибка:", e); 
    }
}