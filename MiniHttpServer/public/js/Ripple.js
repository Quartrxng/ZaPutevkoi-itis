// Функция для безопасной валидации цвета
function getRippleColor(attributeValue) {
    const colorMap = {
        white: 'rgba(255, 255, 255, 0.2)',
        gray: 'rgba(77, 77, 77, 0.2)',
        black: 'rgba(0, 0, 0, 0.2)',
        red: 'rgba(255, 0, 0, 0.2)',
    };
    return colorMap[attributeValue] || 'rgba(255, 255, 255, 0.2)'; // по умолчанию
}

function createRipple(event) {
    const button = event.currentTarget;
    const rippleContainer = button._rippleContainer; // используем кэшированный элемент

    if (!rippleContainer) return;

    // Очищаем предыдущий ripple
    while (rippleContainer.firstChild) {
        rippleContainer.removeChild(rippleContainer.firstChild);
    }

    const colorAttribute = button.getAttribute('data-ripple-color');
    const rippleColor = getRippleColor(colorAttribute);

    const rect = button.getBoundingClientRect();
    const size = Math.max(rect.width, rect.height);
    const x = event.clientX - rect.left;
    const y = event.clientY - rect.top;

    const ripple = document.createElement('span');
    ripple.classList.add('mui-ripple-child');
    ripple.style.cssText = `
        position: absolute;
        width: ${size}px;
        height: ${size}px;
        left: ${x - size / 2}px;
        top: ${y - size / 2}px;
        background: ${rippleColor};
        border-radius: 50%;
        transform: scale(0);
        transition: transform 0.6s ease-out;
        pointer-events: none;
    `;

    rippleContainer.appendChild(ripple);

    // Ждём следующего кадра и запускаем анимацию
    requestAnimationFrame(() => {
        if (ripple.parentNode) { // проверяем, не удалён ли элемент
            ripple.style.transform = 'scale(2)';
        }
    });

    button._activeRipple = ripple;
}

function fadeRipple(event) {
    const button = event.currentTarget;
    const ripple = button._activeRipple;

    if (ripple && ripple.parentNode) {
        ripple.style.transform = 'scale(2)'; // фиксируем текущий размер
        ripple.style.transition = 'opacity 0.4s ease-out';
        ripple.style.opacity = '0';

        setTimeout(() => {
            if (ripple.parentNode) {
                ripple.parentNode.removeChild(ripple);
            }
        }, 400);

        button._activeRipple = null;
    }
}

// Инициализация: добавляем обработчики только один раз
function initRippleEffect() {
    document.querySelectorAll('a.button--ripple, button.button--ripple').forEach(element => {
        if (element._rippleInitialized) return; // пропускаем, если уже инициализировано

        const rippleContainer = element.querySelector('.touchripple');
        if (!rippleContainer) return;

        element._rippleContainer = rippleContainer; // кэшируем
        element._rippleInitialized = true;

        element.addEventListener('mousedown', createRipple);
        element.addEventListener('mouseup', fadeRipple);
        element.addEventListener('mouseleave', fadeRipple);
    });
}

document.addEventListener('DOMContentLoaded', initRippleEffect);

// Опционально: при динамическом добавлении кнопок (например, через AJAX)
// вызвать initRippleEffect() снова