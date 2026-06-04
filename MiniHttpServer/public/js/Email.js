    document.addEventListener('DOMContentLoaded', function() {
    const emailInput = document.querySelector('input[name="email"]');
    const subscribeButton = document.querySelector('button[type="submit"]');
    const form = document.querySelector('form.Banner_content');

    if (!emailInput || !subscribeButton || !form) {
        console.warn('Email input, button or form not found');
    return;
    }

    // Валидация email
    function isValidEmail(email) {
        const domain = email.split('.').pop().toLowerCase();
    const allowedDomains = ['com', 'ru', 'org', 'uk', 'net'];
    return allowedDomains.includes(domain);
    }

    // Обработчик input
    emailInput.addEventListener('input', function() {
        const emailValue = this.value;
    if (isValidEmail(emailValue)) {
        subscribeButton.disabled = false;
    subscribeButton.classList.remove('sumbit__disabled');
    subscribeButton.removeAttribute('tabindex');
        } else {
        subscribeButton.disabled = true;
    subscribeButton.classList.add('sumbit__disabled');
    subscribeButton.setAttribute('tabindex', '-1');
        }
    });

    // Проверка при загрузке
    if (isValidEmail(emailInput.value)) {
        subscribeButton.disabled = false;
    subscribeButton.classList.remove('sumbit__disabled');
    subscribeButton.removeAttribute('tabindex');
    }

    // Обработчик отправки формы
    form.addEventListener('submit', function(e) {
        e.preventDefault(); // <- блокируем стандартное поведение
    const email = emailInput.value.trim();
    if (!email) return;

    fetch('/Login/', {
        method: 'POST',
    headers: {'Content-Type': 'application/json' },
    body: JSON.stringify({email})
        });

    emailInput.value = '';
    subscribeButton.disabled = true;
    subscribeButton.classList.add('sumbit__disabled');
    subscribeButton.setAttribute('tabindex', '-1');
    });
});
