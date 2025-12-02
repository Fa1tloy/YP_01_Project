// modern.js
(() => {
    // toast-уведомления
    window.showToast = (msg, type = 'success') => {
        const toast = new bootstrap.Toast(document.getElementById('liveToast'));
        const body = document.getElementById('toastMessage');
        body.textContent = msg;
        body.className = `toast-body text-${type === 'error' ? 'danger' : 'success'}`;
        toast.show();
    };

    // спиннер на кнопки
    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', () => {
            const btn = form.querySelector('button[type="submit"]');
            if (!btn) return;
            btn.disabled = true;
            btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Обработка...';
        });
    });

    // range-slider с баблом
    document.querySelectorAll('input[type=range]').forEach(slider => {
        const bubble = document.createElement('div');
        bubble.className = 'range-value';
        slider.parentElement.classList.add('range-wrap');
        slider.parentElement.appendChild(bubble);

        const setBubble = () => {
            const val = slider.value;
            const min = slider.min || 0;
            const max = slider.max || 100;
            const percent = (val - min) / (max - min);
            const width = slider.offsetWidth;
            bubble.textContent = (+val).toLocaleString('ru-RU') + ' ₽';
            bubble.style.left = `${percent * width}px`;
            bubble.style.transform = 'translateX(-50%)';
        };
        slider.addEventListener('input', setBubble);
        setBubble();
    });

    // переключатель темы (если нужно)
    const toggle = document.createElement('button');
    toggle.className = 'btn btn-sm btn-outline-secondary position-fixed top-0 end-0 m-3';
    toggle.innerHTML = '🌓';
    toggle.onclick = () => {
        const html = document.documentElement;
        const current = html.getAttribute('data-theme');
        const next = current === 'dark' ? 'light' : 'dark';   
        html.setAttribute('data-theme', next);
        localStorage.setItem('theme', next);
    };
    document.body.appendChild(toggle);
    document.documentElement.setAttribute('data-theme', localStorage.getItem('theme') || 'light');
})(); 