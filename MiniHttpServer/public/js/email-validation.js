document.addEventListener('DOMContentLoaded', function() {
    // Находим элементы формы
    const emailInput = document.querySelector('input[name="email"]');
    const subscribeButton = document.querySelector('button[type="submit"]');

    if (!emailInput || !subscribeButton) {
        console.warn('Email input or subscribe button not found');
        return;
    }

    // Функция проверки валидности email (исправленная)
    function isValidEmail(email) {
        const domain = email.split('.').pop().toLowerCase();
        const allowedDomains = ['com', 'ru', 'org', 'uk', 'net'];
        return allowedDomains.includes(domain);
    }

    // Обработчик события ввода
    emailInput.addEventListener('input', function() {
        const emailValue = this.value;
        
        if (isValidEmail(emailValue)) {
            // Активируем кнопку
            subscribeButton.disabled = false;
            subscribeButton.classList.remove('sumbit__disabled');
            subscribeButton.removeAttribute('tabindex');
        } else {
            // Делаем кнопку неактивной
            subscribeButton.disabled = true;
            subscribeButton.classList.add('sumbit__disabled');
            subscribeButton.setAttribute('tabindex', '-1');
        }
    });

    // Проверяем при загрузке страницы, если поле уже заполнено
    if (isValidEmail(emailInput.value)) {
        subscribeButton.disabled = false;
        subscribeButton.classList.remove('sumbit__disabled');
        subscribeButton.removeAttribute('tabindex');
    }
});