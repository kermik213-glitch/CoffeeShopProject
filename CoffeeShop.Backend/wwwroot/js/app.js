let cart = JSON.parse(localStorage.getItem('cart') || '{}');
let appliedStickers = [];

function saveCart() {
    localStorage.setItem('cart', JSON.stringify(cart));
    renderCart();
}

function renderCart() {
    const list = document.getElementById('cart-list');
    if (!list) return;
    const ids = Object.keys(cart).map(Number);
    if (ids.length === 0) {
        list.innerHTML = '<p class="empty-cart">Корзина пуста. Добавьте товары из меню 👈</p>';
        document.getElementById('total-price').textContent = '0';
        return;
    }
    let total = 0;
    list.innerHTML = '';
    ids.forEach(id => {
        const p = (window._products || []).find(x => x.id === id);
        if (!p) return;
        const qty = cart[id];
        total += p.price * qty;
        const row = document.createElement('div');
        row.className = 'cart-row';
        row.innerHTML = `
            <div><strong>${p.title}</strong><br><small>${p.price} ₽</small></div>
            <div class="qty-controls">
                <button class="qty-btn" data-id="${id}" data-delta="-1">−</button>
                <span>${qty}</span>
                <button class="qty-btn" data-id="${id}" data-delta="1">+</button>
            </div>
        `;
        list.appendChild(row);
    });
    document.getElementById('total-price').textContent = total.toFixed(2);
    list.querySelectorAll('.qty-btn').forEach(btn => {
        btn.onclick = () => {
            const id = btn.dataset.id;
            const delta = parseInt(btn.dataset.delta);
            cart[id] = (cart[id] || 0) + delta;
            if (cart[id] <= 0) delete cart[id];
            saveCart();
        };
    });
}

async function loadProducts() {
    const grid = document.getElementById('products-grid');
    try {
        const products = await API.get('/products');
        window._products = products;
        grid.innerHTML = '';
        
        products.forEach(p => {
            const card = document.createElement('div');
            card.className = 'product-card';
            
            card.innerHTML = `
                <img src="${p.imageUrl || 'https://via.placeholder.com/150'}" 
                     alt="${p.title}" 
                     style="width:100%; height:150px; object-fit:cover; border-radius:6px; margin-bottom:10px;">
                <h3>${p.title}</h3>
                <p class="category">${p.category}</p>
                <p>${p.description}</p>
                <div class="card-footer">
                    <strong>${p.price} ₽</strong>
                    <button data-id="${p.id}" class="add-btn">В корзину</button>
                </div>
            `;
            grid.appendChild(card);
        });

        grid.querySelectorAll('.add-btn').forEach(btn => {
            btn.onclick = () => {
                const id = btn.dataset.id;
                cart[id] = (cart[id] || 0) + 1;
                saveCart();
                btn.textContent = '✓ Добавлено';
                setTimeout(() => btn.textContent = 'В корзину', 800);
            };
        });
        renderCart();
    } catch (e) {
        grid.innerHTML = `<p class="error">Не удалось загрузить меню: ${e.message}</p>`;
    }
}

async function checkout() {
    if (!API.user) {
        alert('Войдите, чтобы оформить заказ');
        window.location.href = 'login.html';
        return;
    }
    const items = Object.keys(cart).map(id => ({ productId: Number(id), quantity: cart[id] }));
    if (items.length === 0) { alert('Корзина пуста'); return; }
    try {
        const res = await API.post('/orders', {
            items,
            appliedStickers: appliedStickers.join(', ')
        });
        cart = {}; appliedStickers = [];
        localStorage.removeItem('cart');
        document.getElementById('applied-stickers-container').innerHTML = '';
        renderCart();
        const receipt = await API.get(`/orders/${res.id}/receipt`);
        document.getElementById('receipt-area').innerHTML =
            `<pre class="receipt">${receipt.replace(/</g,'&lt;')}</pre>`;
    } catch (e) {
        alert('Ошибка оформления: ' + e.message);
    }
}

function initDnd() {
    const stickers = document.querySelectorAll('.sticker');
    const dropzone = document.getElementById('coffee-cup-dropzone');
    const appliedContainer = document.getElementById('applied-stickers-container');

    stickers.forEach(sticker => {
        sticker.addEventListener('dragstart', (e) => {
            e.dataTransfer.setData('text/plain', e.target.dataset.sticker);
            e.target.classList.add('dragging');
        });
        sticker.addEventListener('dragend', (e) => e.target.classList.remove('dragging'));
    });

    dropzone.addEventListener('dragover', (e) => {
        e.preventDefault();
        dropzone.classList.add('hovered');
    });
    dropzone.addEventListener('dragleave', () => dropzone.classList.remove('hovered'));
    dropzone.addEventListener('drop', (e) => {
        e.preventDefault();
        dropzone.classList.remove('hovered');
        const name = e.dataTransfer.getData('text/plain');
        if (!appliedStickers.includes(name)) {
            appliedStickers.push(name);
            const badge = document.createElement('div');
            badge.className = 'applied-sticker-tag';
            badge.innerText = name + ' ✕';
            badge.onclick = () => {
                appliedStickers = appliedStickers.filter(s => s !== name);
                badge.remove();
            };
            appliedContainer.appendChild(badge);
        }
    });
}

document.addEventListener('DOMContentLoaded', () => {
    loadProducts();
    initDnd();
    const btn = document.getElementById('checkout-btn');
    if (btn) btn.onclick = checkout;
});
