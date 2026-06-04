// ===================================================================
// ФИЛЬТР ПИТАНИЕ (filter-meal)
// ===================================================================

// Находим кнопку "Питание" (теперь это filter-meal)
const filterMeal = document.querySelector('.filter-meal');
const filterContent = filterMeal ? filterMeal.querySelector('.filter-content') : null;
const filterLabel = filterMeal ? filterMeal.querySelector('.filter-label') : null;
const selectArrow = filterMeal ? filterMeal.querySelector('.select-arrow') : null;

if (filterMeal) {
    // Добавляем обработчик клика на filter-meal
    filterMeal.addEventListener('click', function (e) {
        // Проверяем, что клик был именно на filter-meal или его вложенных элементах,
        // но НЕ на элементе с классом other-filter (кроме случая, когда он также является filter-meal)
        if (e.target.closest('.other-filter') && !e.target.closest('.filter-meal.other-filter')) {
            // Если клик был по .other-filter, который НЕ является .filter-meal.other-filter,
            // то не обрабатываем его
            return;
        }

        // Проверяем, что клик был внутри текущего filter-meal
        if (!this.contains(e.target) || this !== e.target.closest('.filter-meal')) {
            return;
        }

        // Проверяем, есть ли атрибут disabled у родительского элемента или самого элемента
        if (this.hasAttribute('disabled') || this.closest('[disabled]')) {
            return; // Если disabled - ничего не делаем
        }

        // Определяем, был ли клик по select-arrow
        const isClickOnArrow = e.target.closest('.select-arrow') !== null;

        // Останавливаем всплытие события, чтобы не срабатывали другие обработчики
        e.stopPropagation();

        // Находим выпадающее меню, если оно уже существует
        let dropdown = document.getElementById('nutrition-dropdown');

        // Проверяем, открыто ли уже выпадающее меню
        if (dropdown && dropdown.style.display !== 'none' && dropdown.style.display !== '') {
            // Если меню открыто, просто закрываем его
            closeDropdown();
            return;
        }

        if (!dropdown) {
            // Создаем выпадающее меню
            dropdown = document.createElement('div');
            dropdown.id = 'nutrition-dropdown';
            dropdown.className = 'nutrition-dropdown';
            dropdown.innerHTML = `
                <div class="dropdown-header"><span>ПИТАНИЕ</span></div>
                <div class="dropdown-options">
                    <label class="option-label">
                        <input type="radio" name="nutrition" value="any" checked>
                        <span class="default">Любой</span>
                    </label>
                    <label class="option-label">
                        <input type="radio" name="nutrition" value="bb">
                        <span class="code">BB</span><span class="description"> - Только завтрак</span>
                    </label>
                    <label class="option-label">
                        <input type="radio" name="nutrition" value="hb">
                        <span class="code">HB</span><span class="description"> - Завтрак, ужин</span>
                    </label>
                    <label class="option-label">
                        <input type="radio" name="nutrition" value="fb">
                        <span class="code">FB</span><span class="description"> - Полный пансион</span>
                    </label>
                    <label class="option-label">
                        <input type="radio" name="nutrition" value="ai">
                        <span class="code">AI</span><span class="description"> - Все включено</span>
                    </label>
                    <label class="option-label">
                        <input type="radio" name="nutrition" value="uai">
                        <span class="code">UAI</span><span class="description"> - Ультра все включено</span>
                    </label>
                </div>
            `;

            // Добавляем в документ
            document.body.appendChild(dropdown);

            // Стили для радио-кнопок — уже в CSS
            const radios = dropdown.querySelectorAll('input[type="radio"]');
            radios.forEach(radio => {
                radio.classList.add('nutrition-radio');
            });

            // Обработчики кликов по опциям
            const options = dropdown.querySelectorAll('.option-label');
            options.forEach(option => {
                option.addEventListener('click', function () {
                    const radio = this.querySelector('input[type="radio"]');
                    const value = radio.value;

                    if (value === 'any') {
                        updateTVAddSelectContent(value, 'Любой', 'Любой');
                    } else {
                        const code = this.querySelector('.code').textContent.trim();
                        const displayText = code + ' и лучше'; // "BB и лучше", "HB и лучше" и т.д.

                        // Создаем или обновляем filter--select__content
                        updateTVAddSelectContent(value, code, displayText);
                    }

                    setTimeout(() => {
                        closeDropdown();
                    }, 0);
                });
            });
        }

        // Рассчитываем позицию: начало filter-meal + 10px вниз
        const rect = this.getBoundingClientRect();
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        const scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;

        // Позиционируем выпадающее меню
        dropdown.style.top = `${rect.bottom + scrollTop + 10}px`; // 10px отступ вниз
        dropdown.style.left = `${rect.left + scrollLeft}px`; // Начало от левого края элемента

        // Показываем выпадающее меню
        dropdown.style.display = 'block';

        // Меняем класс у стрелки
        if (selectArrow) {
            selectArrow.classList.add('arrow-top');
        }

        // Закрываем при клике вне меню
        document.addEventListener('click', closeDropdownOnClickOutside);

        // Закрывает при нажатии Escape
        document.addEventListener('keydown', closeDropdownOnEscape);
    });

    // Функция для обновления filter--select__content
    function updateTVAddSelectContent(value, titleText, displayText) {
        // Удаляем старый filter--select__content, если есть
        const oldSelectContent = filterMeal.querySelector('.filter--select__content');
        if (oldSelectContent) {
            oldSelectContent.remove();
        }

        // Создаем новый filter--select__content
        const newSelectContent = document.createElement('div');
        newSelectContent.className = 'filter--select__content';
        newSelectContent.title = titleText; // Только код (например, "BB", "HB")
        newSelectContent.textContent = displayText; // Код + "и лучше" (например, "BB и лучше")

        // Добавляем его в filter-meal
        filterMeal.appendChild(newSelectContent);

        if (value === 'any') {
            // Если выбрано "Любой", скрываем filter--select__content и не скрываем filter-content
            newSelectContent.style.display = 'none';
            if (filterContent) {
                filterContent.style.display = '';
            }
            // Убираем класс selected у filter-label
            if (filterLabel) {
                filterLabel.classList.remove('selected');
            }
        } else {
            // Если выбрано не "Любой", показываем filter--select__content и скрываем filter-content
            newSelectContent.style.display = 'block';
            if (filterContent) {
                filterContent.style.display = 'none';
            }
            // Добавляем класс selected у filter-label
            if (filterLabel) {
                filterLabel.classList.add('selected');
            }
        }
    }

    // Функция закрытия выпадающего меню при клике вне его
    function closeDropdownOnClickOutside(e) {
        if (!e.target.closest('#nutrition-dropdown') && !e.target.closest('.filter-meal')) {
            closeDropdown();
        }
    }

    // Функция закрытия выпадающего меню при нажатии Escape
    function closeDropdownOnEscape(e) {
        if (e.key === 'Escape') {
            closeDropdown();
        }
    }

    // Функция закрытия выпадающего меню
    function closeDropdown() {
        const dropdown = document.getElementById('nutrition-dropdown');
        if (dropdown) {
            dropdown.style.display = 'none';
            document.removeEventListener('click', closeDropdownOnClickOutside);
            document.removeEventListener('keydown', closeDropdownOnEscape);

            // Убираем класс у стрелки
            if (selectArrow) {
                selectArrow.classList.remove('arrow-top');
            }
        }
    }
}

// ===================================================================
// ФИЛЬТР РЕЙТИНГ (filter-rating)
// ===================================================================

// Находим кнопку "Рейтинг"
const filterRating = document.querySelector('.filter-rating');
const filterContentRating = filterRating ? filterRating.querySelector('.filter-content') : null;
const filterLabelRating = filterRating ? filterRating.querySelector('.filter-label') : null;
const selectArrowRating = filterRating ? filterRating.querySelector('.select-arrow') : null;

if (filterRating) {
    // Добавляем обработчик клика на filter-rating
    filterRating.addEventListener('click', function (e) {
        // Проверяем, что клик был именно на filter-rating или его вложенных элементах,
        // но НЕ на элементе с классом other-filter (кроме случая, когда он также является filter-rating)
        if (e.target.closest('.other-filter') && !e.target.closest('.filter-rating.other-filter')) {
            return;
        }

        // Проверяем, что клик был внутри текущего filter-rating
        if (!this.contains(e.target) || this !== e.target.closest('.filter-rating')) {
            return;
        }

        // Проверяем, есть ли атрибут disabled у родительского элемента или самого элемента
        if (this.hasAttribute('disabled') || this.closest('[disabled]')) {
            return; // Если disabled - ничего не делаем
        }

        // Останавливаем всплытие события
        e.stopPropagation();

        // Находим выпадающее меню, если оно уже существует
        let dropdown = document.getElementById('rating-dropdown');

        // Проверяем, открыто ли уже выпадающее меню
        if (dropdown && dropdown.style.display !== 'none' && dropdown.style.display !== '') {
            closeDropdown();
            return;
        }

        if (!dropdown) {
            // Создаем выпадающее меню
            dropdown = document.createElement('div');
            dropdown.id = 'rating-dropdown';
            dropdown.className = 'rating-dropdown';
            dropdown.innerHTML = `
                <div class="dropdown-header"><span>РЕЙТИНГ</span></div>
                <div class="dropdown-options">
                    <label class="option-label">
                        <input type="radio" name="rating" value="any" checked>
                        <span class="default">Любой</span>
                    </label>
                    <label class="option-label">
                        <input type="radio" name="rating" value="3.0">
                        <span class="code">3,0</span><span class="description"> и более</span>
                    </label>
                    <label class="option-label">
                        <input type="radio" name="rating" value="3.5">
                        <span class="code">3,5</span><span class="description"> и более</span>
                    </label>
                    <label class="option-label">
                        <input type="radio" name="rating" value="4.0">
                        <span class="code">4,0</span><span class="description"> и более</span>
                    </label>
                    <label class="option-label">
                        <input type="radio" name="rating" value="4.5">
                        <span class="code">4,5</span><span class="description"> и более</span>
                    </label>
                </div>
            `;

            // Добавляем в документ
            document.body.appendChild(dropdown);

            // Стили для радио-кнопок — уже в CSS
            const radios = dropdown.querySelectorAll('input[type="radio"]');
            radios.forEach(radio => {
                radio.classList.add('rating-radio');
            });

            // Обработчики кликов по опциям
            const options = dropdown.querySelectorAll('.option-label');
            options.forEach(option => {
                option.addEventListener('click', function () {
                    const radio = this.querySelector('input[type="radio"]');
                    const value = radio.value;

                    if (value === 'any') {
                        updateTVAddSelectContentRating(value, 'Любой', 'Любой');
                    } else {
                        const prefixSpan = this.querySelector('.TVRadioGroupSelectItemPrefix');
                        const prefixText = prefixSpan ? prefixSpan.textContent : value.replace('.', ',');
                        const displayText = prefixText + ' и более';
                        updateTVAddSelectContentRating(value, value, displayText);
                    }

                    setTimeout(() => {
                        closeDropdown();
                    }, 0);
                });
            });
        }

        // Рассчитываем позицию
        const rect = this.getBoundingClientRect();
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        const scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;

        dropdown.style.top = `${rect.bottom + scrollTop + 10}px`;
        dropdown.style.left = `${rect.left + scrollLeft}px`;

        dropdown.style.display = 'block';

        if (selectArrowRating) {
            selectArrowRating.classList.add('arrow-top');
        }

        // Закрываем при клике вне меню
        document.addEventListener('click', closeDropdownOnClickOutsideRating);
        // Закрывает при нажатии Escape
        document.addEventListener('keydown', closeDropdownOnEscapeRating);
    });

    // Функция для обновления filter--select__content для рейтинга
    function updateTVAddSelectContentRating(value, titleText, displayText) {
        // Удаляем старый filter--select__content, если есть
        const oldSelectContent = filterRating.querySelector('.filter--select__content');
        if (oldSelectContent) {
            oldSelectContent.remove();
        }

        // Создаем новый filter--select__content
        const newSelectContent = document.createElement('div');
        newSelectContent.className = 'filter--select__content';
        newSelectContent.title = titleText;
        newSelectContent.textContent = displayText;

        // Добавляем его в filter-rating
        filterRating.appendChild(newSelectContent);

        if (value === 'any') {
            newSelectContent.style.display = 'none';
            if (filterContentRating) {
                filterContentRating.style.display = '';
            }
            if (filterLabelRating) {
                filterLabelRating.classList.remove('selected');
            }
        } else {
            newSelectContent.style.display = 'block';
            if (filterContentRating) {
                filterContentRating.style.display = 'none';
            }
            if (filterLabelRating) {
                filterLabelRating.classList.add('selected');
            }
        }
    }

    // Функция закрытия выпадающего меню при клике вне его
    function closeDropdownOnClickOutsideRating(e) {
        if (!e.target.closest('#rating-dropdown') && !e.target.closest('.filter-rating')) {
            closeDropdown();
        }
    }

    // Функция закрытия выпадающего меню при нажатии Escape
    function closeDropdownOnEscapeRating(e) {
        if (e.key === 'Escape') {
            closeDropdown();
        }
    }

    // Функция закрытия выпадающего меню
    function closeDropdown() {
        const dropdown = document.getElementById('rating-dropdown');
        if (dropdown) {
            dropdown.style.display = 'none';
            document.removeEventListener('click', closeDropdownOnClickOutsideRating);
            document.removeEventListener('keydown', closeDropdownOnEscapeRating);

            if (selectArrowRating) {
                selectArrowRating.classList.remove('arrow-top');
            }
        }
    }
}