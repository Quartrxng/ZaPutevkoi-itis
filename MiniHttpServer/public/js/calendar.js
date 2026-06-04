// === ГЛОБАЛЬНЫЕ ПЕРЕМЕННЫЕ ===
let startDate = null;
let endDate = null;
let currentOffset = 0; // текущее смещение месяцев от текущего

// Находим элементы
const dateSelect = document.querySelector('.main--select');
const calendarPopup = document.getElementById('calendarPopup');

// === ФОРМАТЫ ===
function formatDate(date) {
    const months = ['янв', 'фев', 'мар', 'апр', 'май', 'июн', 'июл', 'авг', 'сен', 'окт', 'ноя', 'дек'];
    return `${date.getDate()} ${months[date.getMonth()]}`;
}

function formatRange() {
    if (!startDate || !endDate) return '';
    const daysDiff = Math.ceil((endDate - startDate) / (1000 * 60 * 60 * 24));
    return `${formatDate(startDate)} - ${formatDate(endDate)} (${daysDiff} нч)`;
}

function updateInputValue() {
    const contentElement = document.querySelector('.main--select__content');
    if (!contentElement) return;

    const rangeText = formatRange(); // Полный текст: "15 ноя - 20 ноя (5 нч)"
    contentElement.textContent = rangeText;

    // Извлекаем только количество ночей как число
    let nights = '';
    if (startDate && endDate) {
        const diffTime = endDate - startDate;
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        nights = String(diffDays); // Просто цифра, например "5"
    }

    // Записываем только цифру в title
    contentElement.title = nights;
}

function initDefaultRange() {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const tomorrow = new Date(today);
    tomorrow.setDate(today.getDate() + 1);

    startDate = today;
    endDate = tomorrow;

    updateInputValue();
}

// === РЕНДЕР КАЛЕНДАРЯ ===
function renderCalendar(monthOffset = 0) {
    currentOffset = monthOffset;

    const now = new Date();
    const currentMonth = new Date(now.getFullYear(), now.getMonth() + monthOffset, 1);
    const nextMonth = new Date(currentMonth.getFullYear(), currentMonth.getMonth() + 1, 1);

    const months = ['ЯНВАРЬ', 'ФЕВРАЛЬ', 'МАРТ', 'АПРЕЛЬ', 'МАЙ', 'ИЮНЬ',
        'ИЮЛЬ', 'АВГУСТ', 'СЕНТЯБРЬ', 'ОКТЯБРЬ', 'НОЯБРЬ', 'ДЕКАБРЬ'];

    calendarPopup.innerHTML = '';

    // === ЗАГОЛОВОК С КНОПКАМИ ===
    const header = document.createElement('div');
    header.className = 'calendar-header';

    const prevButton = document.createElement('button');
    prevButton.className = 'nav-button prev';
    prevButton.textContent = '';
    prevButton.disabled = currentOffset <= 0;
    prevButton.style.opacity = currentOffset <= 0 ? '0.4' : '1';
    prevButton.style.cursor = currentOffset <= 0 ? 'not-allowed' : 'pointer';
    prevButton.addEventListener('click', (e) => {
        e.stopPropagation();
        if (currentOffset > 0) renderCalendar(currentOffset - 1);
    });

    const month1Div = document.createElement('div');
    month1Div.className = 'calendar-month';
    month1Div.innerHTML = `<h3>${months[currentMonth.getMonth()]} <span>${currentMonth.getFullYear()}</span></h3>`;

    const month2Div = document.createElement('div');
    month2Div.className = 'calendar-month';
    month2Div.innerHTML = `<h3>${months[nextMonth.getMonth()]} <span>${nextMonth.getFullYear()}</span></h3>`;

    const nextButton = document.createElement('button');
    nextButton.className = 'nav-button next';
    nextButton.textContent = '';
    nextButton.disabled = currentOffset >= 12;
    nextButton.style.opacity = currentOffset >= 12 ? '0.4' : '1';
    nextButton.style.cursor = currentOffset >= 12 ? 'not-allowed' : 'pointer';
    nextButton.addEventListener('click', (e) => {
        e.stopPropagation();
        if (currentOffset < 12) renderCalendar(currentOffset + 1);
    });

    header.appendChild(prevButton);
    header.appendChild(month1Div);
    header.appendChild(month2Div);
    header.appendChild(nextButton);
    calendarPopup.appendChild(header);

    // === ОСНОВНОЙ БЛОК ДВУХ МЕСЯЦЕВ ===
    const monthsContainer = document.createElement('div');
    monthsContainer.className = 'calendar-months-container';

    // --- РЕНДЕРИМ ПЕРВЫЙ МЕСЯЦ ---
    const month1Block = renderSingleMonth(currentMonth, true); // true = первый месяц
    monthsContainer.appendChild(month1Block);

    // --- РЕНДЕРИМ ВТОРОЙ МЕСЯЦ ---
    const month2Block = renderSingleMonth(nextMonth, false); // false = второй месяц
    monthsContainer.appendChild(month2Block);

    calendarPopup.appendChild(monthsContainer);
}

// Вспомогательная функция для рендеринга одного месяца
function renderSingleMonth(monthDate, isFirstMonth) {
    const container = document.createElement('div');
    container.className = 'single-month-block';

    // Дни недели
    const weekdays = ['ПН', 'ВТ', 'СР', 'ЧТ', 'ПТ', 'СБ', 'ВС'];
    const weekRow = document.createElement('div');
    weekRow.className = 'calendar-weekdays';
    weekdays.forEach(day => {
        const d = document.createElement('div');
        d.textContent = day;
        weekRow.appendChild(d);
    });
    container.appendChild(weekRow);

    // Дни месяца
    const daysGrid = document.createElement('div');
    daysGrid.className = 'calendar-days';

    const firstDayOfMonth = monthDate.getDay(); // 0=вс, 1=пн, ..., 6=сб
    const adjustedFirstDay = (firstDayOfMonth === 0) ? 6 : firstDayOfMonth - 1; // Понедельник = 0

    // Пустые ячейки до начала месяца
    for (let i = 0; i < adjustedFirstDay; i++) {
        const empty = document.createElement('div');
        empty.className = 'calendar-day disabled';
        daysGrid.appendChild(empty);
    }

    const daysInMonth = new Date(monthDate.getFullYear(), monthDate.getMonth() + 1, 0).getDate();
    for (let day = 1; day <= daysInMonth; day++) {
        const date = new Date(monthDate.getFullYear(), monthDate.getMonth(), day);
        const today = new Date();
        today.setHours(0, 0, 0, 0); // сбрасываем время

        const dayEl = document.createElement('div');
        dayEl.className = 'calendar-day';
        dayEl.textContent = day;

        // Проверка: дата в прошлом?
        const isPast = date < today;

        if (isPast) {
            dayEl.classList.add('disabled');
            dayEl.style.pointerEvents = 'none'; // отключаем клик
            dayEl.style.color = '#ccc';         // приглушаем цвет
        } else {
            // Только для будущих (и сегодняшних!) дней — добавляем обработчик клика
            dayEl.addEventListener('click', (e) => {
                e.stopPropagation();
                if (!startDate) {
                    startDate = date;
                    endDate = null;
                } else if (!endDate && date >= startDate) {
                    endDate = date;
                } else {
                    startDate = date;
                    endDate = null;
                }
                renderCalendar(currentOffset);
                updateInputValue();

                // Если выбраны обе даты, закрываем календарь
                if (startDate && endDate) {
                    setTimeout(() => {
                        calendarPopup.classList.remove('open');
                    }, 300);
                }
            });
        }

        // Подсветка выбранных дат (даже если они сегодня — разрешено)
        if (startDate && date.getTime() === startDate.getTime()) {
            dayEl.classList.add('selected', 'start');
            if (isPast) {
                // Если старт — прошлая дата (редко, но возможно при инициализации),
                // всё равно делаем её активной
                dayEl.style.color = '#fff';
            }
        }
        if (endDate && date.getTime() === endDate.getTime()) {
            dayEl.classList.add('selected', 'end');
            if (isPast) {
                dayEl.style.color = '#fff';
            }
        }
        if (startDate && endDate && date > startDate && date < endDate) {
            dayEl.classList.add('range');
            if (isPast) {
                dayEl.style.backgroundColor = '#cce5ff';
            }
        }

        daysGrid.appendChild(dayEl);
    }

    // Заполняем оставшиеся строки до 6 недель (42 ячейки)
    const totalDays = adjustedFirstDay + daysInMonth;
    const rowsNeeded = Math.ceil(totalDays / 7);

    // Если меньше 6 строк — не добавляем лишние
    if (rowsNeeded < 6) {
        // Добавляем пустые ячейки только до конца последней строки
        const remainingInLastRow = 7 - (totalDays % 7);
        if (remainingInLastRow < 7) {
            for (let i = 0; i < remainingInLastRow; i++) {
                const empty = document.createElement('div');
                empty.className = 'calendar-day disabled';
                daysGrid.appendChild(empty);
            }
        }
    } else {
        // Если нужно 6 строк — заполняем до 42
        while (daysGrid.children.length < 42) {
            const empty = document.createElement('div');
            empty.className = 'calendar-day disabled';
            daysGrid.appendChild(empty);
        }
    }

    container.appendChild(daysGrid);
    return container;
}

// === ОБРАБОТКА КЛИКА ПО СЕЛЕКТУ ===
dateSelect.addEventListener('click', (e) => {
    e.stopPropagation();

    // Рассчитываем позицию календаря относительно клика
    const rect = dateSelect.getBoundingClientRect();
    const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
    const scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;

    // Устанавливаем позицию календаря
    calendarPopup.style.top = (rect.bottom + scrollTop + 10) + 'px';
    calendarPopup.style.left = (rect.left + scrollLeft + rect.width / 2 - 345) + 'px'; // 345 = половина ширины календаря (690/2)

    renderCalendar(currentOffset);
    calendarPopup.classList.add('open');
});

// === ЗАКРЫТИЕ КАЛЕНДАРЯ ПРИ КЛИКЕ ВНЕ ===
document.addEventListener('click', (e) => {
    if (
        !dateSelect.contains(e.target) &&
        !calendarPopup.contains(e.target) &&
        calendarPopup.classList.contains('open')
    ) {
        // Если выбрано начало, но не выбран конец — установить конец на следующий день
        if (startDate && !endDate) {
            endDate = new Date(startDate);
            endDate.setDate(startDate.getDate() + 1);
            updateInputValue();
        }
        calendarPopup.classList.remove('open');
    }
});

// === ИНИЦИАЛИЗАЦИЯ ===
initDefaultRange(); // начальное состояние