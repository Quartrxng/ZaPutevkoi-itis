// === ГЛОБАЛЬНЫЙ ФЛАГ, ЧТОБЫ НЕ ПЕРЕЗАГРУЖАТЬ ДАННЫЕ ===
let servicesDataLoaded = false;

// === ФУНКЦИЯ ДЛЯ ЗАГРУЗКИ ДАННЫХ С СЕРВЕРА ===
async function fetchServicesData() {
    try {
        const response = await fetch('api/filters', {
            method: 'GET',
            headers: {
                'Accept': 'application/json'
            }
        });

        if (!response.ok) {
            throw new Error(`Ошибка при загрузке фильтров: ${response.status}`);
        }

        const rawData = await response.json();

        const groupedData = Object.values(
            rawData.reduce((acc, item) => {
                const categoryName = item.category_Name;
                if (!acc[categoryName]) {
                    acc[categoryName] = {
                        categoryName: categoryName,
                        categoryOrder: item.category_Order,
                        services: []
                    };
                }
                acc[categoryName].services.push({
                    id: item.id,
                    serviceName: item.service_Name,
                    serviceOrder: item.service_Order
                });
                return acc;
            }, {})
        );

        renderServices(groupedData);

    } catch (error) {
        console.error('Ошибка загрузки данных фильтров:', error);
    }
}

// === ФУНКЦИЯ ДЛЯ ОБНОВЛЕНИЯ ОТОБРАЖЕНИЯ ВЫБРАННЫХ УСЛУГ ===
function updateSelectedDisplay(selectedCount) {
    const serviceFilter = document.querySelector('.service-filter.active, .service-filter.clicked') || document.querySelector('.service-filter');
    if (!serviceFilter) return;

    let addSelectContent = serviceFilter.querySelector('.filter--select__content');
    if (!addSelectContent) {
        console.error('Элемент .filter--select__content не найден в .service-filter');
        return;
    }

    const filterLabel = serviceFilter.querySelector('.filter-label');
    const container = document.getElementById('servicesContent');
    if (!container) return;

    const checkboxes = container.querySelectorAll('input[type="checkbox"]:checked');
    const selectedServices = [];
    const selectedIds = [];

    checkboxes.forEach(checkbox => {
        const label = container.querySelector(`label[for="${checkbox.id}"]`);
        if (label) {
            selectedServices.push(label.textContent.trim());
        }
        selectedIds.push(checkbox.dataset.serviceId);
    });

    const filterContent = serviceFilter.querySelector('.filter-content');

    if (selectedServices.length === 0) {
        addSelectContent.title = '0';
        addSelectContent.textContent = 'Любой';
        addSelectContent.style.display = 'none';
        if (filterLabel) filterLabel.className = 'filter-label';

        const selectButton = container.querySelector('.tourists--select__btn, .tourists--select--button__outlined');
        if (selectButton) {
            selectButton.className = 'tourists--select--button__outlined';
            selectButton.textContent = 'Выбрать';
        }

        const selectedTab = container.querySelector('.svc-tab-item .svc-tab-selected');
        if (selectedTab) {
            selectedTab.style.display = 'none';
        }
        const resetTab = container.querySelector('.svc-tab-item:nth-child(3)');
        if (resetTab) {
            resetTab.style.display = 'none';
        }

        const allTab = container.querySelector('.svc-tab-item:nth-child(1)');
        const selectedTabElement = container.querySelector('.svc-tab-item:nth-child(2)');
        if (allTab) {
            allTab.classList.add('svc-tab-active');
        }
        if (selectedTabElement) {
            selectedTabElement.classList.remove('svc-tab-active');
        }

    } else {
        addSelectContent.title = selectedIds.join(',');
        addSelectContent.textContent = selectedServices.length === 1 ? selectedServices[0] : `Выбрано (${selectedServices.length})`;
        addSelectContent.style.display = 'block';
        if (filterLabel) filterLabel.className = 'filter-label selected';

        const selectButton = container.querySelector('.tourists--select__btn, .tourists--select--button__outlined');
        if (selectButton) {
            selectButton.className = 'tourists--select__btn';
            selectButton.textContent = 'Выбрать';
        }

        const selectedTab = container.querySelector('.svc-tab-item .svc-tab-selected');
        if (selectedTab) {
            selectedTab.style.display = 'inline-flex';
        }
        const resetTab = container.querySelector('.svc-tab-item:nth-child(3)');
        if (resetTab) {
            resetTab.style.display = 'block';
        }
    }

    if (selectedServices.length > 0 && filterContent) {
        filterContent.style.display = 'none';
    } else if (filterContent) {
        filterContent.style.display = '';
    }
}

// === ФУНКЦИЯ ДЛЯ ОТОБРАЖЕНИЯ УСЛУГ ===
function renderServices(data) {
    let container = document.getElementById('servicesContent');
    if (!container) {
        container = document.createElement('div');
        container.id = 'servicesContent';
        container.style.display = 'none';
        document.body.appendChild(container);
    }

    container.innerHTML = '';

    const headerDiv = document.createElement('div');
    headerDiv.className = 'dropdown-header';
    const headerSpan = document.createElement('span');
    headerSpan.textContent = 'Услуги в отеле';
    headerDiv.appendChild(headerSpan);
    container.appendChild(headerDiv);

    const tabControl = document.createElement('div');
    tabControl.className = 'svc-tab-control';

    const allTab = document.createElement('div');
    allTab.className = 'svc-tab-item svc-tab-active';
    allTab.textContent = 'Все';

    const selectedTab = document.createElement('div');
    selectedTab.className = 'svc-tab-item';
    const selectedSpan = document.createElement('span');
    selectedSpan.className = 'svc-tab-selected';
    const selectedText = document.createElement('span');
    selectedText.textContent = 'Выбрано';
    const selectedCount = document.createElement('span');
    selectedCount.className = 'svc-tab-selected-count';
    selectedCount.textContent = '0';
    selectedSpan.appendChild(selectedText);
    selectedSpan.appendChild(selectedCount);
    selectedTab.appendChild(selectedSpan);

    const resetTab = document.createElement('div');
    resetTab.className = 'svc-tab-item';
    resetTab.textContent = 'Сброс';

    tabControl.appendChild(allTab);
    tabControl.appendChild(selectedTab);
    tabControl.appendChild(resetTab);
    container.appendChild(tabControl);

    const categoriesContainer = document.createElement('div');
    categoriesContainer.className = 'services-categories';
    data.sort((a, b) => a.categoryOrder - b.categoryOrder);

    data.forEach(category => {
        const sortedServices = [...category.services].sort((a, b) => a.serviceOrder - b.serviceOrder);
        const categoryEl = document.createElement('div');
        categoryEl.className = 'svc-category';
        categoryEl.dataset.category = category.categoryName.toLowerCase();

        const titleEl = document.createElement('div');
        titleEl.className = 'svc-category-title';
        titleEl.textContent = category.categoryName;
        categoryEl.appendChild(titleEl);

        sortedServices.forEach(service => {
            const item = document.createElement('div');
            item.className = 'svc-service-item';

            const checkbox = document.createElement('input');
            checkbox.type = 'checkbox';
            checkbox.id = `service_${service.id}`;
            checkbox.dataset.serviceId = service.id;

            checkbox.addEventListener('change', function () {
                updateSelectedCount(selectedCount);
                updateSelectedDisplay(selectedCount.textContent);

                const currentSelectedTab = container.querySelector('.svc-tab-item:nth-child(2).svc-tab-active');
                if (currentSelectedTab) {
                    updateSelectedTabView();
                } else {
                    const currentAllTab = container.querySelector('.svc-tab-item:nth-child(1).svc-tab-active');
                    if (currentAllTab) {
                        document.querySelectorAll('.svc-category').forEach(cat => {
                            cat.style.display = 'block';
                            const title = cat.querySelector('.svc-category-title');
                            if (title) title.style.display = 'block';
                            const items = cat.querySelectorAll('.svc-service-item');
                            items.forEach(item => {
                                item.style.display = '';
                            });
                        });
                    }
                }
            });

            const label = document.createElement('label');
            label.htmlFor = `service_${service.id}`;
            label.textContent = service.serviceName;

            item.appendChild(checkbox);
            item.appendChild(label);
            categoryEl.appendChild(item);
        });

        categoriesContainer.appendChild(categoryEl);
    });

    container.appendChild(categoriesContainer);

    const selectButton = document.createElement('button');
    selectButton.className = 'tourists--select--button__outlined';
    selectButton.textContent = 'Выбрать';

    selectButton.addEventListener('click', function () {
        container.style.display = 'none';
        updateSelectedDisplay(0);
    });

    container.appendChild(selectButton);

    allTab.addEventListener('click', function () {
        document.querySelectorAll('.svc-tab-item').forEach(tab => tab.classList.remove('svc-tab-active'));
        this.classList.add('svc-tab-active');

        document.querySelectorAll('.svc-category').forEach(cat => {
            cat.style.display = 'block';
            const title = cat.querySelector('.svc-category-title');
            if (title) title.style.display = 'block';
            const items = cat.querySelectorAll('.svc-service-item');
            items.forEach(item => {
                item.style.display = '';
            });
        });
    });

    selectedTab.addEventListener('click', function () {
        document.querySelectorAll('.svc-tab-item').forEach(tab => tab.classList.remove('svc-tab-active'));
        this.classList.add('svc-tab-active');
        updateSelectedTabView();
    });

    resetTab.addEventListener('click', function () {
        const checkboxes = container.querySelectorAll('input[type="checkbox"]');
        checkboxes.forEach(ch => (ch.checked = false));
        updateSelectedCount(selectedCount);
        updateSelectedDisplay(selectedCount.textContent);

        const allTab = container.querySelector('.svc-tab-item:nth-child(1)');
        const selectedTabElement = container.querySelector('.svc-tab-item:nth-child(2)');
        if (allTab) {
            allTab.classList.add('svc-tab-active');
        }
        if (selectedTabElement) {
            selectedTabElement.classList.remove('svc-tab-active');
        }

        document.querySelectorAll('.svc-category').forEach(cat => {
            cat.style.display = 'block';
            const title = cat.querySelector('.svc-category-title');
            if (title) title.style.display = 'block';
            const items = cat.querySelectorAll('.svc-service-item');
            items.forEach(item => {
                item.style.display = '';
            });
        });
    });
}

// === ОБНОВЛЕНИЕ ОТОБРАЖЕНИЯ В ТАБЕ "ВЫБРАНО" ===
function updateSelectedTabView() {
    const container = document.getElementById('servicesContent');
    if (!container) return;

    const checkboxes = container.querySelectorAll('input[type="checkbox"]:checked');
    const checkedServiceIds = Array.from(checkboxes).map(cb => parseInt(cb.dataset.serviceId));

    document.querySelectorAll('.svc-category').forEach(cat => {
        cat.style.display = 'none';
    });

    document.querySelectorAll('.svc-category').forEach(cat => {
        const items = cat.querySelectorAll('.svc-service-item');
        let hasChecked = false;

        items.forEach(item => {
            const checkbox = item.querySelector('input[type="checkbox"]');
            if (checkbox && checkedServiceIds.includes(parseInt(checkbox.dataset.serviceId))) {
                item.style.display = '';
                hasChecked = true;
            } else {
                item.style.display = 'none';
            }
        });

        if (hasChecked) {
            cat.style.display = 'block';
            const title = cat.querySelector('.svc-category-title');
            if (title) title.style.display = 'block';
        }
    });

    if (checkedServiceIds.length === 0) {
        document.querySelectorAll('.svc-category').forEach(cat => {
            cat.style.display = 'none';
        });
    }
}

// === ОБНОВЛЕНИЕ СЧЁТЧИКА ===
function updateSelectedCount(countElement) {
    const container = document.getElementById('servicesContent');
    if (!container) return;
    const checked = container.querySelectorAll('input[type="checkbox"]:checked');
    countElement.textContent = checked.length.toString();
}

// === ПОКАЗ/СКРЫТИЕ ВЫПАДАЮЩЕГО СПИСКА ===
function toggleServicesDropdown(filterButton) {
    const container = document.getElementById('servicesContent');
    if (!filterButton) return;

    if (container.style.display === 'flex') {
        container.style.display = 'none';
    } else {
        const rect = filterButton.getBoundingClientRect();
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        const scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;

        container.style.top = `${rect.bottom + scrollTop + 10}px`;
        container.style.left = `${rect.left + scrollLeft}px`;
        container.style.display = 'flex';

        setTimeout(() => {
            const selectedCount = container.querySelector('.svc-tab-selected-count');
            if (selectedCount) {
                updateSelectedCount(selectedCount);
                updateSelectedDisplay(selectedCount.textContent);
            }
        }, 0);
    }
}

// === ИНИЦИАЛИЗАЦИЯ ===
document.addEventListener('DOMContentLoaded', function () {
    if (typeof fetchServicesData === 'undefined') {
        console.error('Функция fetchServicesData не найдена!');
        return;
    }

    const filterButton = document.querySelector('.service-filter');

    if (filterButton) {
        filterButton.addEventListener('click', async function (e) {
            e.stopPropagation();
            document.querySelectorAll('.service-filter').forEach(f => f.classList.remove('active'));
            this.classList.add('active');

            try {
                if (!servicesDataLoaded) {
                    await fetchServicesData();
                    servicesDataLoaded = true;
                }

                toggleServicesDropdown(this);

            } catch (error) {
                console.error('Ошибка при обработке фильтров:', error);
            }
        });

        document.addEventListener('click', function (e) {
            const container = document.getElementById('servicesContent');
            if (container && container.style.display === 'flex') {
                if (!e.target.closest('.service-filter') && !e.target.closest('#servicesContent')) {
                    container.style.display = 'none';
                    updateSelectedDisplay(0);
                }
            }
        });
    } else {
        console.error('Элемент с классом service-filter не найден');
    }
});
