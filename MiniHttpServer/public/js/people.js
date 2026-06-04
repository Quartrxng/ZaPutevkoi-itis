document.addEventListener('DOMContentLoaded', function () {
    const filter = document.querySelector('.toursist--filter');
    const tooltip = document.querySelector('.tourists--tooltip');
    const countSpan = document.querySelector('.tourists--count');
    const minusBtn = document.querySelector('.tourists--btn-minus');
    const plusBtn = document.querySelector('.tourists--btn-plus');
    const selectBtn = document.querySelector('.tourists--select__btn');
    const addChildBtn = document.querySelector('.tourists--addchild');

    // Уточняем, что ищем .main--select__content только внутри .toursist--filter
    const mainSelectContent = filter.querySelector('.main--select__content');

    // Элементы для выбора возраста
    const ageSelector = tooltip.querySelector('.tourists--select--age__container');
    const ageItems = tooltip.querySelectorAll('.tourists--select--age__item');

    // Состояние
    let adults = 2;
    let children = [];

    // Вспомогательная функция для форматирования возраста ребёнка
    function formatChildAge(age) {
        if (age <= 1) return 'до 2 лет';
        if (age >= 2 && age <= 4) return `${age} года`;
        return `${age} лет`;
    }

    // Обновление текста в основном фильтре и атрибута title
    function updateMainDisplay() {
        // Обновляем только цифру в tourists--count
        countSpan.textContent = adults; // Только цифра, без "взрослых"

        // Формируем текст для отображения
        let text = '';

        // Взрослые
        if (adults === 1) {
            text = '1 взрослый';
        } else if (adults >= 2 && adults <= 4) {
            text = `${adults} взрослых`;
        } else {
            text = `${adults} взрослых`;
        }

        // Дети
        const childCount = children.filter(age => age !== null).length;
        if (childCount > 0) {
            let childText = '';
            if (childCount === 1) {
                childText = '1 ребёнок';
            } else if (childCount >= 2 && childCount <= 4) {
                childText = `${childCount} ребенка`;
            } else {
                childText = `${childCount} детей`;
            }
            text += `, ${childText}`;
        }

        if (mainSelectContent) {
            // Обновляем текстовое содержимое
            mainSelectContent.textContent = text || '2 взрослых';

            // Вычисляем общее количество туристов (взрослые + дети) и записываем только цифру в title
            const totalTourists = adults + childCount;
            mainSelectContent.title = totalTourists; // Только цифра, например "3"
        }
    }

    // Увеличение взрослых
    plusBtn.addEventListener('click', function () {
        if (adults < 6) {
            adults++;
            if (adults > 1) {
                countSpan.classList.remove('touristone');
                countSpan.classList.add('tourists--all');
            }
            updateMainDisplay();
        }
    });

    // Уменьшение взрослых
    minusBtn.addEventListener('click', function () {
        if (adults > 1) {
            adults--;
            if (adults == 1) {
                countSpan.classList.remove('tourists--all');
                countSpan.classList.add('touristone');
            }
            updateMainDisplay();
            
        }
    });

    // Клик по "Добавить ребенка"
    addChildBtn.addEventListener('click', function (e) {
        e.stopPropagation(); // Останавливаем всплытие события, чтобы окно не закрылось

        // Показываем блок выбора возраста
        ageSelector.style.display = 'block';
        ageSelector.style.visibility = 'visible';

        tooltip.querySelector('.tourists--remember').style.display = 'none';
        selectBtn.style.display = 'none';

        tooltip.querySelector('.tourists--addchild').style.display = 'none';
    });

    // Обработка выбора возраста
    ageItems.forEach(item => {
        item.addEventListener('click', function (e) {
            e.stopPropagation(); // Останавливаем всплытие события, чтобы окно не закрылось

            const age = parseInt(this.getAttribute('data-age'), 10);
            children.push(age); // Добавляем ребенка с выбранным возрастом

            // Создаём элемент ребёнка
            const childItem = document.createElement('div');
            childItem.className = 'TVTouristChildItem tourists--controls';

            childItem.innerHTML = `
            <div class="tourists--btn tourists--btn-minus">
            </div>
            <div class="tourist--child">${formatChildAge(age)}</div>
        `;

            // Находим элемент tourists--addchild
            const addChildElement = tooltip.querySelector('.tourists--addchild');
            if (addChildElement) {
                // Вставляем новый элемент перед tourists--addchild
                addChildElement.parentNode.insertBefore(childItem, addChildElement);
            } else {
                // Если tourists--addchild не найден, добавляем в конец какого-то родительского контейнера
                // Предположим, что это tourists--controls или другой контейнер
                const touristsControls = tooltip.querySelector('.tourists--controls');
                if (touristsControls) {
                    touristsControls.appendChild(childItem);
                }
            }

            // Добавляем обработчик клика для кнопки минус внутри TVTouristChildItem
            const childMinusBtn = childItem.querySelector('.tourists--btn-minus');
            childMinusBtn.addEventListener('click', function (e) {
                e.stopPropagation(); // Останавливаем всплытие события, чтобы окно не закрылось

                // Находим индекс этого ребенка в массиве children
                const childIndex = Array.from(tooltip.querySelectorAll('.TVTouristChildItem')).indexOf(childItem);

                if (childIndex !== -1) {
                    // Удаляем ребенка из массива
                    children.splice(childIndex, 1);

                    // Удаляем элемент из DOM
                    childItem.remove();

                    // Обновляем отображение
                    updateMainDisplay();

                    // Проверяем количество детей и возвращаем/скрываем кнопку "Добавить ребёнка"
                    if (children.filter(age => age !== null).length < 3) {
                        addChildBtn.style.display = 'block';
                    } else {
                        addChildBtn.style.display = 'none';
                    }
                }
            });

            // Скрываем выбор возраста
            ageSelector.style.display = 'none';
            ageSelector.style.visibility = 'hidden';

            // Возвращаем основные элементы
            if (children.filter(age => age !== null).length >= 3) {
                addChildBtn.style.display = 'none';
            }
            else { 
                addChildBtn.style.display = 'block';
            }
            tooltip.querySelector('.tourists--remember').style.display = 'flex';
            selectBtn.style.display = 'block';

            // Возвращаем заголовок
            tooltip.querySelector('.tourists--tooltip__header').textContent = 'ТУРИСТЫ';

            // Обновляем отображение
            updateMainDisplay();
        });
    });

    // Клик по фильтру — открываем тултип
    filter.addEventListener('click', function (e) {
        e.stopPropagation();

        // Временно показываем тултип, чтобы получить его размеры
        tooltip.style.display = 'block';
        tooltip.style.opacity = '0'; // Скрываем визуально, но сохраняем в DOM
        tooltip.style.visibility = 'hidden';

        // Ждём, чтобы обновились стили
        setTimeout(() => {
            // Позиционируем тултип под фильтром с отступом 10px
            const rect = filter.getBoundingClientRect();
            const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
            const scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;

            tooltip.style.top = `${rect.bottom + scrollTop + 10}px`;
            tooltip.style.left = `${rect.left + scrollLeft}px`;

            // Снова скрываем, но уже с правильной позицией
            tooltip.style.opacity = '1';
            tooltip.style.visibility = 'visible';
        }, 0);
    });

    // Закрытие при клике вне тултипа
    document.addEventListener('click', function (e) {
        if (!tooltip.contains(e.target) && e.target !== filter) {
            tooltip.style.display = 'none';
        }
    });

    // Кнопка "Выбрать" — закрывает тултип
    selectBtn.addEventListener('click', function (e) {
        e.stopPropagation(); // Останавливаем всплытие события, чтобы окно не закрылось
        tooltip.style.display = 'none';
    });

    // Инициализация
    updateMainDisplay();
});