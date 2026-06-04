document.addEventListener('DOMContentLoaded', function () {
    const container = document.querySelector('.placements--search__content');
    if (!container) return;

    // ———— ГЛОБАЛЬНЫЕ ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ (определяются ОДИН РАЗ) ————
    function createItem(item) {
        const wrapper = document.createElement('div');
        wrapper.className = 'search--item';

        const resultItem = document.createElement('div');
        resultItem.className = 'tour--search--input--result--item';

        // —— Иконка ——
        const iconDiv = document.createElement('div');
        iconDiv.className = 'tour--search--input--result--item__icon';

        // Приводим type к нижнему регистру
        const type = (item.type || '').toLowerCase();

        if (type === 'country' && item.image && item.image !== 'none' && item.image.trim() !== '') {
            // ✅ Если тип — страна и есть картинка — вставляем <img>
            const img = document.createElement('img');
            img.src = item.image;
            img.alt = 'flag';
            img.style.width = '20px';
            img.style.height = '20px';
            img.style.objectFit = 'contain';
            iconDiv.appendChild(img);
        } else {
            // Для остальных типов — стандартные SVG-иконки
            const iconHTML =
                type === 'hotel' ? getHotelIcon() :
                    type === 'country' ? getCountryIcon() :
                        getCityIcon();
            iconDiv.innerHTML = iconHTML;
        }

        // —— Текстовая информация ——
        const infoDiv = document.createElement('div');
        infoDiv.className = 'tour--search--input--result--item--info';

        const titleDiv = document.createElement('div');
        titleDiv.className = 'tour--search--input--result--item__title';
        titleDiv.textContent = item.name;

        const descDiv = document.createElement('div');
        descDiv.className = 'tour--search--input--result--item__description';
        if (item.country) {
            descDiv.textContent = item.country;
        } else {
            descDiv.style.display = 'none';
        }

        const typeDiv = document.createElement('div');
        typeDiv.className = 'SearchInputResultItemType';
        typeDiv.style.display = 'none';
        typeDiv.textContent = type;

        infoDiv.appendChild(titleDiv);
        infoDiv.appendChild(descDiv);
        infoDiv.appendChild(typeDiv);

        resultItem.appendChild(iconDiv);
        resultItem.appendChild(infoDiv);
        wrapper.appendChild(resultItem);

        return wrapper;
    }

    function getHotelIcon() {
        return `<svg xmlns="http://www.w3.org/2000/svg" width="20px" height="20px" viewBox="0 0 512 512" fill="#5c6672">
            <path d="M432,230.7a79.44,79.44,0,0,0-32-6.7H112a79.51,79.51,0,0,0-32,6.69h0A80.09,80.09,0,0,0,32,304V416a16,16,0,0,0,32,0v-8a8.1,8.1,0,0,1,8-8H440a8.1,8.1,0,0,1,8,8v8a16,16,0,0,0,32,0V304A80.09,80.09,0,0,0,432,230.7Z"/>
            <path d="M376,80H136a56,56,0,0,0-56,56v72a4,4,0,0,0,5.11,3.84A95.5,95.5,0,0,1,112,208h4.23a4,4,0,0,0,4-3.55A32,32,0,0,1,152,176h56a32,32,0,0,1,31.8,28.45,4,4,0,0,0,4,3.55h24.46a4,4,0,0,0,4-3.55A32,32,0,0,1,304,176h56a32,32,0,0,1,31.8,28.45,4,4,0,0,0,4,3.55H400a95.51,95.51,0,0,1,26.89,3.85A4,4,0,0,0,432,208V136A56,56,0,0,0,376,80Z"/>
        </svg>`;
    }

    function getCityIcon() {
        return `<svg width="20px" height="20px" viewBox="0 0 16 16" fill="#5c6672" xmlns="http://www.w3.org/2000/svg">
            <path fill-rule="evenodd" d="M8 16s6-5.686 6-10A6 6 0 0 0 2 6c0 4.314 6 10 6 10zm0-7a3 3 0 1 0 0-6 3 3 0 0 0 0 6z"/>
        </svg>`;
    }

    function getCountryIcon(imagePath) {
        if (imagePath && imagePath !== 'none') {
            return `<img src="${imagePath}" alt="flag" style="width: 20px; height: 20px; object-fit: contain;" />`;
        }
        return `<svg width="20px" height="20px" viewBox="0 0 16 16" fill="#5c6672" xmlns="http://www.w3.org/2000/svg">
            <path fill-rule="evenodd" d="M8 16s6-5.686 6-10A6 6 0 0 0 2 6c0 4.314 6 10 6 10zm0-7a3 3 0 1 0 0-6 3 3 0 0 0 0 6z"/>
        </svg>`;
    }

    // Функция для установки/снятия атрибута disabled для элементов filter-item
    function setDisabledForFilterItems(isDisabled) {
        console.log('setDisabledForFilterItems вызвана с isDisabled:', isDisabled);

        const filtersContainer = document.querySelector('.filters-container');
        if (!filtersContainer) {
            console.log('filters-container не найден');
            return;
        }

        const filterItems = filtersContainer.querySelectorAll('.filter-item');
        console.log('Найдено filter-item:', filterItems.length);

        filterItems.forEach((item, index) => {
            if (isDisabled) {
                item.classList.add('disabled');
                console.log(`Элемент ${index}: добавлен disabled`);
            } else {
                item.classList.remove('disabled');
                console.log(`Элемент ${index}: убран disabled`);
            }
        });

        // Дополнительная проверка
        const disabledItems = document.querySelectorAll('.filter-item.disabled');
        console.log('Сейчас disabled элементов:', disabledItems.length);
    }

    // ———— ОСТАЛЬНЫЙ КОД ————
    let lastSelectedInfo = { name: null, country: null, type: null };
    let allPossibleCities = [];

    const selectedElement = container.querySelector('.tour--search__content');
    if (selectedElement) {
        setupSelectedHandler(selectedElement);
        const mainContent = selectedElement.querySelector('.tour--search__maincontent');
        const detailContent = selectedElement.querySelector('.tour--search__detailcontent');
        const typeElement = selectedElement.querySelector('.SearchInputResultItemType');
        if (mainContent) {
            lastSelectedInfo.name = mainContent.textContent.trim();
        }
        if (detailContent) {
            const text = detailContent.textContent.trim();
            lastSelectedInfo.country = text.startsWith('(') && text.endsWith(')') ? text.slice(1, -1) : text;
        }
        if (typeElement) {
            lastSelectedInfo.type = typeElement.textContent.trim();
        }

        // Применяем disabled если тип отель
        console.log('Первоначальная настройка, тип:', lastSelectedInfo.type);
        if (lastSelectedInfo.type === 'hotel') {
            setDisabledForFilterItems(true);
        } else {
            setDisabledForFilterItems(false);
        }
    } else {
        initializeInput();
    }

    const specialCities = [
        { name: 'Анталия', country: 'Турция', type: 'city' },
        { name: 'Анапа', country: 'Россия', type: 'city' }
    ];

    const normalCities = [
        { name: 'Дубай', country: 'ОАЭ', type: 'city' },
        { name: 'Пхукет', country: 'Таиланд', type: 'city' },
        { name: 'Сочи', country: 'Россия', type: 'city' },
        { name: 'Фукуок', country: 'Вьетнам', type: 'city' },
        { name: 'Шарм-Эль-Шейх', country: 'Египет', type: 'city' },
        { name: 'Бали', country: 'Индонезия', type: 'city' }
    ];

    allPossibleCities = [...specialCities, ...normalCities];

    function initializeInput() {
        const inputContainer = container.querySelector('.placement--search__input');
        if (!inputContainer) return;

        const input = inputContainer.querySelector('input[placeholder]');
        if (!input) return;

        // ——— ДОБАВИТЬ СКРЫТЫЙ ЭЛЕМЕНТ (если его нет) ———
        if (!inputContainer.querySelector('.SearchInputResultItemType')) {
            const hiddenTypeElement = document.createElement('div');
            hiddenTypeElement.className = 'SearchInputResultItemType';
            hiddenTypeElement.style.display = 'none';
            hiddenTypeElement.textContent = 'none';
            inputContainer.appendChild(hiddenTypeElement);
        }

        const tooltip = document.createElement('div');
        tooltip.id = 'suggestionsTooltip';
        tooltip.className = 'tooltip search--hide';
        tooltip.innerHTML = `
            <div class="TVAutocompleteTooltipContent tour--search--result">
                <div class="TVAutocompleteListControl">
                    <div class="search--auto--complete tour--search--result--last" style="display: none; border-bottom: 1px solid #ddd;"></div>
                    <div class="search--auto--complete tour--search--result--region"></div>
                </div>
                <div class="autocomplete--tooltip__empty TVNewSearchInputEmpty search--hide"></div>
            </div>
        `;
        input.parentNode.appendChild(tooltip);

        const listLast = tooltip.querySelector('.tour--search--result--last');
        const listRegions = tooltip.querySelector('.tour--search--result--region');
        const emptyMessage = tooltip.querySelector('.autocomplete--tooltip__empty');

        let timeoutId = null;
        let userInput = false;
        let selectedByClick = false;
        let isChoosingFromTooltip = false; // ✅ FIX

        tooltip.addEventListener('mousedown', () => { isChoosingFromTooltip = true; });
        tooltip.addEventListener('mouseup', () => { isChoosingFromTooltip = false; });

        function positionTooltip() {
            const filterElement = input.closest('.search--filter') || document.querySelector('.search--filter');
            if (!filterElement) return;

            const filterRect = filterElement.getBoundingClientRect();
            const tooltipWidth = filterRect.width;

            let top = filterRect.bottom + window.scrollY;
            let left = filterRect.left + window.scrollX;

            tooltip.style.width = `${tooltipWidth}px`;

            const viewportWidth = window.innerWidth || document.documentElement.clientWidth;
            const tooltipRight = left + tooltipWidth;
            if (tooltipRight > viewportWidth) {
                left = viewportWidth - tooltipWidth;
            }
            if (left < 0) {
                left = 0;
            }

            tooltip.style.top = `${top + 10}px`;
            tooltip.style.left = `${left}px`;
        }

        input.addEventListener('input', () => {
            const query = input.value.trim();
            clearTimeout(timeoutId);
            userInput = true;

            if (query.length < 2) {
                renderSuggestions(allPossibleCities);
                positionTooltip();
                showTooltip();
                return;
            }

            timeoutId = setTimeout(() => fetchSuggestions(query), 300);
        });

        // ✅ FIX: Исправленный обработчик blur
        input.addEventListener('blur', () => {
            if (isChoosingFromTooltip) return; // Не реагируем, если клик по тултипу

            setTimeout(() => {
                const value = input.value.trim();

                if (userInput && !selectedByClick) {
                    if (lastSelectedInfo.name) {
                        // Используем сохраненные данные
                        replaceWithSelectedElement(lastSelectedInfo.name, lastSelectedInfo.country, lastSelectedInfo.type);
                        // ✅ ОБНОВЛЯЕМ СОСТОЯНИЕ ФИЛЬТРОВ
                        setDisabledForFilterItems(lastSelectedInfo.type === 'hotel');
                    } else {
                        const firstSpecial = specialCities[0];
                        if (firstSpecial) {
                            // Обновляем lastSelectedInfo при восстановлении по умолчанию
                            lastSelectedInfo = {
                                name: firstSpecial.name,
                                country: firstSpecial.country,
                                type: firstSpecial.type
                            };
                            replaceWithSelectedElement(lastSelectedInfo.name, lastSelectedInfo.country, lastSelectedInfo.type);
                            // ✅ ОБНОВЛЯЕМ СОСТОЯНИЕ ФИЛЬТРОВ
                            setDisabledForFilterItems(lastSelectedInfo.type === 'hotel');
                        }
                    }
                }

                userInput = false;
                selectedByClick = false;
                hideTooltip();
            }, 250);
        });

        input.addEventListener('contextmenu', (e) => e.preventDefault());

        input.addEventListener('pointerdown', (e) => {
            if (e.button === 0) {
                if (input.value.trim()) {
                    input.value = '';
                    userInput = false;
                    selectedByClick = false;
                    renderSuggestions(allPossibleCities);
                    positionTooltip();
                    showTooltip();
                }
            }
        });

        input.addEventListener('focus', () => {
            setTimeout(() => {
                const query = input.value.trim();
                if (query.length < 2) {
                    renderSuggestions(allPossibleCities);
                    positionTooltip();
                    showTooltip();
                } else {
                    fetchSuggestions(query);
                }
            }, 0);
        });

        tooltip.addEventListener('click', (e) => {
            const item = e.target.closest('.search--item');
            if (item) {
                const title = item.querySelector('.tour--search--input--result--item__title').textContent;
                const desc = item.querySelector('.tour--search--input--result--item__description').textContent;
                const type = item.querySelector('.SearchInputResultItemType').textContent;

                // Сохраняем полную информацию о выбранном элементе
                lastSelectedInfo = {
                    name: title,
                    country: desc,
                    type: type
                };

                selectedByClick = true;
                replaceWithSelectedElement(lastSelectedInfo.name, lastSelectedInfo.country, lastSelectedInfo.type);

                // ✅ ВЫЗЫВАЕМ ОБНОВЛЕНИЕ СОСТОЯНИЯ ФИЛЬТРОВ
                console.log('Выбран элемент типа:', type);
                setDisabledForFilterItems(type === 'hotel');

                hideTooltip();
            }
        });

        tooltip.addEventListener('contextmenu', (e) => e.preventDefault());

        document.addEventListener('click', (e) => {
            if (!tooltip.contains(e.target) && !input.contains(e.target)) {
                hideTooltip();
            }
        });

        async function fetchSuggestions(query) {
            try {
                console.log('Отправляемый запрос:', query); // Отладка
                const response = await fetch(`/api/placements/${encodeURIComponent(query)}`, {
                    headers: { 'Content-Type': 'application/json' }
                });
                if (!response.ok) throw new Error('Network error');
                const data = await response.json();

                console.log('Полученные данные:', data); // Отладка

                // Преобразуем полученные данные в нужный формат, игнорируя значения 'none'
                const processedData = data.map(item => {
                    // Проверяем, есть ли у объекта нужные свойства
                    const processedItem = {
                        name: item.Name || item.name || '',
                        country: item.Country || item.country || '',
                        image: item.Image || item.image || '',
                        type: item.Type || item.type || 'city'
                    };

                    // Заменяем 'none' на пустую строку
                    processedItem.name = processedItem.name !== 'none' ? processedItem.name : '';
                    processedItem.country = processedItem.country !== 'none' ? processedItem.country : '';
                    processedItem.image = processedItem.image !== 'none' ? processedItem.image : '';
                    processedItem.type = processedItem.type !== 'none' ? processedItem.type : 'city';

                    console.log('Обработанный элемент:', processedItem); // Отладка
                    return processedItem;
                });

                allPossibleCities = [...specialCities, ...normalCities, ...processedData];
                renderSuggestions(processedData);
                positionTooltip();
                showTooltip();
            } catch (err) {
                console.error('Error:', err);
                showEmptyMessage('Ошибка загрузки данных');
            }
        }

        function renderSuggestions(suggestions) {
            listLast.innerHTML = '';
            listRegions.innerHTML = '';

            if (!suggestions.length) {
                showEmptyMessage('Ничего не найдено');
                showTooltip();
                return;
            }

            if (input.value.trim().length >= 2) {
                const listBox = document.createElement('div');
                listBox.className = 'search--list__box search--auto--complete--item tour--search--result--item';
                suggestions.forEach(item => listBox.appendChild(createItem(item)));
                listRegions.appendChild(listBox);

                listLast.style.display = 'none';
                listLast.style.borderBottom = 'none';
            } else {
                if (specialCities.length > 0) {
                    const specialBox = document.createElement('div');
                    specialBox.className = 'search--list__box search--auto--complete--item tour--search--result--item';
                    specialCities.forEach(item => specialBox.appendChild(createItem(item)));
                    listLast.appendChild(specialBox);
                    listLast.style.display = 'block';
                    listLast.style.borderBottom = '1px solid #ddd';
                } else {
                    listLast.style.display = 'none';
                    listLast.style.borderBottom = 'none';
                }

                const otherCities = [...normalCities];
                if (otherCities.length) {
                    const regionsBox = document.createElement('div');
                    regionsBox.className = 'search--list__box search--auto--complete--item tour--search--result--item';
                    otherCities.slice(0, 6).forEach(item => regionsBox.appendChild(createItem(item)));
                    listRegions.appendChild(regionsBox);
                }
            }

            hideEmptyMessage();
        }

        function replaceWithSelectedElement(name, country, type = 'city') {
            const newEl = document.createElement('div');
            newEl.className = 'tour--search__content'; // Используем правильный класс контейнера
            newEl.innerHTML = `
                <div class="tour--search__content">
                    <div class="tour--search__maincontent" title="${name}">${name}</div>
                    <div class="tour--search__detailcontent" ${country ? '' : 'style="display: none;"'}>(${country})</div>
                    <div class="SearchInputResultItemType" style="display: none;">${type}</div>
                </div>
            `;
            inputContainer.parentNode.replaceChild(newEl, inputContainer);
            setupSelectedHandler(newEl);
            selectedByClick = false;
        }

        function showEmptyMessage(text) {
            emptyMessage.textContent = text;
            emptyMessage.classList.remove('search--hide');
            listLast.style.borderBottom = 'none';
        }

        function hideEmptyMessage() {
            emptyMessage.classList.add('search--hide');
        }

        function showTooltip() {
            tooltip.classList.remove('search--hide', 'search--show');
            tooltip.classList.add('search--show');
        }

        function hideTooltip() {
            tooltip.classList.remove('search--show');
            tooltip.classList.add('search--hide');
        }
    }

    function setupSelectedHandler(element) {
        let inputContainer = null;

        element.addEventListener('click', (e) => {
            if (e.button === 0 && (e.target === element ||
                e.target === element.querySelector('.tour--search__maincontent') ||
                e.target === element.querySelector('.tour--search__detailcontent'))) {

                // Сохраняем информацию перед заменой
                const mainContent = element.querySelector('.tour--search__maincontent');
                const detailContent = element.querySelector('.tour--search__detailcontent');
                const typeElement = element.querySelector('.SearchInputResultItemType');

                if (mainContent) {
                    lastSelectedInfo.name = mainContent.textContent.trim();
                }
                if (detailContent) {
                    const text = detailContent.textContent.trim();
                    lastSelectedInfo.country = text.startsWith('(') && text.endsWith(')') ? text.slice(1, -1) : text;
                }
                if (typeElement) {
                    lastSelectedInfo.type = typeElement.textContent.trim();
                }

                inputContainer = document.createElement('div');
                inputContainer.className = 'placement--search__input';
                inputContainer.innerHTML = '<input placeholder="Введите город или название отеля">';

                element.parentNode.replaceChild(inputContainer, element);
                initializeInput();

                setTimeout(() => {
                    const newInput = inputContainer.querySelector('input');
                    if (newInput) {
                        newInput.focus();
                        renderSuggestionsForNewInput(newInput);
                    }
                }, 0);
            }
        });

        element.addEventListener('contextmenu', (e) => e.preventDefault());

        // ✅ Теперь функция принимает сам input, и не зависит от локальных функций
        function renderSuggestionsForNewInput(input) {
            const tooltip = document.getElementById('suggestionsTooltip');
            if (!tooltip || !input) return;

            const listLast = tooltip.querySelector('.tour--search--result--last');
            const listRegions = tooltip.querySelector('.tour--search--result--region');
            const emptyMessage = tooltip.querySelector('.autocomplete--tooltip__empty');

            listLast.innerHTML = '';
            listRegions.innerHTML = '';

            if (specialCities.length > 0) {
                const specialBox = document.createElement('div');
                specialBox.className = 'search--list__box search--auto--complete--item tour--search--result--item';
                specialCities.forEach(item => specialBox.appendChild(createItem(item)));
                listLast.appendChild(specialBox);
                listLast.style.display = 'block';
                listLast.style.borderBottom = '1px solid #ddd';
            } else {
                listLast.style.display = 'none';
                listLast.style.borderBottom = 'none';
            }

            const otherCities = [...normalCities];
            if (otherCities.length) {
                const regionsBox = document.createElement('div');
                regionsBox.className = 'search--list__box search--auto--complete--item tour--search--result--item';
                otherCities.slice(0, 6).forEach(item => regionsBox.appendChild(createItem(item)));
                listRegions.appendChild(regionsBox);
            } else {
                listLast.style.display = 'none';
            }

            emptyMessage.classList.add('search--hide');

            // ✅ Вынесем сюда локальную версию positionTooltip и showTooltip
            const filterElement = input.closest('.search--filter') || document.querySelector('.search--filter');
            if (!filterElement) return;

            const filterRect = filterElement.getBoundingClientRect();
            const tooltipWidth = filterRect.width;
            let top = filterRect.bottom + window.scrollY;
            let left = filterRect.left + window.scrollX;
            tooltip.style.width = `${tooltipWidth}px`;

            const viewportWidth = window.innerWidth || document.documentElement.clientWidth;
            const tooltipRight = left + tooltipWidth;
            if (tooltipRight > viewportWidth) {
                left = viewportWidth - tooltipWidth;
            }
            if (left < 0) left = 0;

            tooltip.style.top = `${top + 10}px`;
            tooltip.style.left = `${left}px`;

            tooltip.classList.remove('search--hide');
            tooltip.classList.add('search--show');
        }
    }
});