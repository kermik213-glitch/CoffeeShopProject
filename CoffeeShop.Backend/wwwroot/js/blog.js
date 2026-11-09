document.addEventListener('DOMContentLoaded', async () => {
    const list = document.getElementById('blog-list');
    try {
        const posts = await API.get('/blog');
        if (posts.length === 0) { list.innerHTML = '<p>Пока нет публикаций</p>'; return; }
        list.innerHTML = posts.map(p => `
            <article class="blog-post">
                <h3>${p.title}</h3>
                <p class="meta">${p.author} · ${new Date(p.createdAt).toLocaleDateString('ru-RU')}</p>
                <p>${p.content}</p>
            </article>
        `).join('');
    } catch (e) { list.innerHTML = `<p class="error">Ошибка: ${e.message}</p>`; }
});