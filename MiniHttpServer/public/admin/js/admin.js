console.log('Admin JS loaded');

// =================== State ===================
let deletedImages = [];
let deletedRooms = [];
let hotelImagesFiles = [];
let roomsData = [];
let editingHotelId = null;
const availableMeals = ['BB', 'HB', 'FB', 'AI', 'UAI'];
let selectedMeal = [...availableMeals];
let servicesList = [];
let selectedServices = [];

// =================== Form Show/Hide ===================
function showHotelForm() {
    const form = document.getElementById('hotelForm');
    if (!form) return;
    form.style.display = 'block';

    if (!editingHotelId) {
        form.reset();

        const descText = document.getElementById('hotelDescriptionText');
        if (descText) descText.value = '';

        const desc = document.getElementById('hotelDescription');
        if (desc) desc.innerHTML = '';

        const hotelCard = document.getElementById('hotelCard');
        if (hotelCard) hotelCard.innerHTML = '';

        const hotelImages = document.getElementById('hotelImages');
        if (hotelImages) hotelImages.innerHTML = '';

        const roomsContainer = document.getElementById('roomsContainer');
        if (roomsContainer) roomsContainer.innerHTML = '';

        const mealSelect = document.getElementById('mealSelect');
        if (mealSelect) mealSelect.innerHTML = '';

        const servicesContainer = document.getElementById('servicesContainer');
        if (servicesContainer) servicesContainer.innerHTML = '';

        deletedImages = [];
        deletedRooms = [];
        hotelImagesFiles = [];
        roomsData = [];
        selectedMeal = [];
        selectedServices = [];

        loadMeal(selectedMeal);
        loadServices([]);
    } else {
        loadMeal(selectedMeal);
    }
}

function logout() {
    fetch('/adminapi/logout', {
        method: 'POST'
    })
        .then(async res => {
            const text = await res.text(); // читаем как текст (не ломается никогда)
            let data = {};

            // пытаемся распарсить JSON, если он есть
            try {
                data = JSON.parse(text);
            } catch (e) {
                // Если сервер вернул пусто или HTML — оставляем data как {}
            }

            if (data.success || res.ok) {
                window.location.href = '/admin/login';
            } else {
                alert('Ошибка выхода: ' + (data.message || 'Неизвестный ответ сервера'));
            }
        })
        .catch(err => {
            console.error('Ошибка logout:', err);
            alert('Ошибка сети при выходе');
        });
}


function hideHotelForm() {
    const form = document.getElementById('hotelForm');
    if (form) form.style.display = 'none';

    const formElement = document.getElementById('hotelFormElement');
    if (formElement && formElement.reset) formElement.reset();

    const desc = document.getElementById('hotelDescription');
    if (desc) desc.innerHTML = '';

    const hotelCard = document.getElementById('hotelCard');
    if (hotelCard) hotelCard.innerHTML = '';

    const hotelImages = document.getElementById('hotelImages');
    if (hotelImages) hotelImages.innerHTML = '';

    const roomsContainer = document.getElementById('roomsContainer');
    if (roomsContainer) roomsContainer.innerHTML = '';

    const mealSelect = document.getElementById('mealSelect');
    if (mealSelect) mealSelect.innerHTML = '';

    const servicesContainer = document.getElementById('servicesContainer');
    if (servicesContainer) servicesContainer.innerHTML = '';

    deletedImages = [];
    deletedRooms = [];
    hotelImagesFiles = [];
    roomsData = [];
    selectedServices = [];
    editingHotelId = null;
    selectedMeal = [];
}

// =================== WYSIWYG for Card ===================
function formatCard(cmd, value = null) {
    const hotelCard = document.getElementById('hotelCard');
    if (!hotelCard) return;

    hotelCard.focus();

    if (cmd === 'createLink') {
        value = prompt('Введите ссылку:', 'https://');
        if (!value) return;
        document.execCommand(cmd, false, value);
    }
    else if (cmd === 'insertTable') {
        insertCardTable();
    }
    else if (cmd === 'insertRow') {
        insertCardRow();
    }
    else {
        document.execCommand(cmd, false, value);
    }
}

function insertCardTable() {
    const hotelCard = document.getElementById('hotelCard');
    if (!hotelCard) return;

    const tableHTML = `
        <table class="hotel-card-table">
            <tr>
                <td><strong>Пример</strong></td>
                <td>Пример значения</td>
            </tr>
        </table>
    `;

    document.execCommand('insertHTML', false, tableHTML);
}

function insertCardRow() {
    const selection = window.getSelection();
    if (!selection.rangeCount) return;

    const range = selection.getRangeAt(0);
    let node = range.startContainer;

    while (node && node.nodeType !== Node.ELEMENT_NODE) {
        node = node.parentNode;
    }

    const table = node ? node.closest('table') : null;

    if (table) {
        const newRow = table.insertRow();
        const cellCount = table.rows[0].cells.length;

        for (let i = 0; i < cellCount; i++) {
            const cell = newRow.insertCell();
            cell.innerHTML = i === 0 ? '<strong>Новый заголовок</strong>' : 'Новое значение';
        }
    } else {
        alert('Пожалуйста, выберите таблицу для добавления строки');
    }
}

function deleteCardRow() {
    const selection = window.getSelection();
    if (!selection.rangeCount) return;

    const range = selection.getRangeAt(0);
    let node = range.startContainer;

    while (node && node.nodeType !== Node.ELEMENT_NODE) {
        node = node.parentNode;
    }

    const row = node ? node.closest('tr') : null;

    if (row) {
        const table = row.closest('table');
        if (table && table.rows.length > 1) {
            if (confirm('Удалить эту строку?')) {
                row.remove();
            }
        } else {
            alert('Нельзя удалить последнюю строку таблицы');
        }
    } else {
        alert('Пожалуйста, выберите строку таблицы для удаления');
    }
}

function deleteCardTable() {
    const selection = window.getSelection();
    if (!selection.rangeCount) return;

    const range = selection.getRangeAt(0);
    let node = range.startContainer;

    while (node && node.nodeType !== Node.ELEMENT_NODE) {
        node = node.parentNode;
    }

    const table = node ? node.closest('table') : null;

    if (table && confirm('Удалить таблицу?')) {
        table.remove();
    } else {
        alert('Пожалуйста, выберите таблицу для удаления');
    }
}

// =================== WYSIWYG ===================
function format(cmd, value = null) {
    if (cmd === 'createLink') {
        value = prompt('Введите ссылку:', 'https://');
        if (!value) return;
    }
    document.execCommand(cmd, false, value);
}

// =================== Helper: Convert File to Base64 ===================
function fileToBase64(file) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(file);
        reader.onload = () => resolve(reader.result);
        reader.onerror = error => reject(error);
    });
}

// =================== Drag & Drop Images (Base64) ===================
function dragOver(e) { e.preventDefault(); }

async function dropRoomImage(e, idx) {
    e.preventDefault();
    const files = e.dataTransfer.files;
    if (files.length === 0) return;

    const file = files[0];

    if (!file.type.includes('jpeg') && !file.type.includes('jpg')) {
        alert('Разрешены только JPG файлы');
        return;
    }

    try {
        const dataUrl = await fileToBase64(file);

        console.log("roomsData length:", roomsData.length, "idx:", idx);
        if (idx < 0 || idx >= roomsData.length) {
            console.error("Комната с индексом", idx, "не найдена в roomsData");
            alert("Комната не найдена. Попробуйте снова.");
            return;
        }

        const room = roomsData[idx];
        if (!room) {
            console.error("roomsData[" + idx + "] is undefined or null");
            return;
        }

        room.file = { name: file.name, dataUrl: dataUrl };

        const container = document.getElementById('roomsContainer');
        const roomDivs = container.querySelectorAll('.room-container');
        if (!roomDivs[idx]) {
            console.error("DOM-элемент комнаты с idx", idx, "не найден");
            return;
        }

        const roomDiv = roomDivs[idx];
        const imagesDiv = roomDiv.querySelector('.room-images');
        if (!imagesDiv) {
            console.error("Элемент .room-images не найден в комнате с idx", idx);
            return;
        }

        imagesDiv.innerHTML = `<div class="image-preview"><img src="${dataUrl}"><button class="delete-img" onclick="deleteRoomImage(this,${idx})">x</button></div>`;
    } catch (error) {
        console.error('Ошибка при кодировании файла комнаты:', file.name, error);
        alert(`Ошибка при обработке файла комнаты: ${file.name}`);
    }
}

function deleteImage(btn, name) {
    if (btn && btn.parentElement) btn.parentElement.remove();
    hotelImagesFiles = hotelImagesFiles.filter(f => f.name !== name);
}

// =================== Rooms (Base64) ===================
async function addRoom(room = {}) {
    const container = document.getElementById('roomsContainer');
    if (!container) return;
    const idx = roomsData.length;

    console.log("Добавляем комнату с idx:", idx, "в roomsData");
    roomsData.push({
        id: room.id || null,
        name: room.name || '',
        price: room.price || '',
        description: room.description || '',
        peopleCount: room.peopleCount || 1,
        oldPhotoName: room.oldPhotoName || null,
        hotelId: room.hotelId || null,
        city: room.city || null,
        country: room.country || null,
        file: null
    });

    console.log("roomsData после push:", roomsData);

    const div = document.createElement('div');
    div.className = 'room-container';
    div.dataset.index = idx;

    div.innerHTML = `
        <input type="hidden" id="room_${idx}_id" value="${roomsData[idx].id || ''}">
        <input type="text" placeholder="Название" value="${roomsData[idx].name}" onchange="roomsData[${idx}].name=this.value">
        <input type="number" placeholder="Цена" value="${roomsData[idx].price}" onchange="roomsData[${idx}].price=parseFloat(this.value) || 0">
        <input type="number" placeholder="Макс. количество людей" value="${roomsData[idx].peopleCount}" onchange="roomsData[${idx}].peopleCount=parseInt(this.value) || 1">
        <input type="text" placeholder="Описание" value="${roomsData[idx].description}" onchange="roomsData[${idx}].description=this.value">
        <div class="room-images" ondrop="dropRoomImage(event,${idx})" ondragover="dragOver(event)">
            ${roomsData[idx].oldPhotoName ? `<div class="image-preview"><img src="/hotels/data/${roomsData[idx].country || ''}/${roomsData[idx].city || ''}/${roomsData[idx].hotelId || ''}/Rooms/${roomsData[idx].oldPhotoName}"><button class="delete-img" onclick="deleteRoomImage(this,${idx})">x</button></div>` : ''}
        </div>
        <button type="button" class="btn" onclick="deleteRoom(${idx})">Удалить комнату</button>
    `;
    container.appendChild(div);
}

async function dropImage(e) {
    e.preventDefault();
    const files = e.dataTransfer.files;
    for (const file of files) {
        if (!file.type.includes('jpeg') && !file.type.includes('jpg')) {
            alert('Разрешены только JPG файлы');
            continue;
        }

        try {
            const dataUrl = await fileToBase64(file);

            const existingIndex = hotelImagesFiles.findIndex(f => f.name === file.name);
            if (existingIndex !== -1) {
                const imgDivs = document.querySelectorAll('#hotelImages .image-preview');
                if (imgDivs[existingIndex] && imgDivs[existingIndex].parentElement) {
                    imgDivs[existingIndex].parentElement.removeChild(imgDivs[existingIndex]);
                }
                hotelImagesFiles.splice(existingIndex, 1);
            }

            hotelImagesFiles.push({ name: file.name, dataUrl: dataUrl });

            const imgDiv = document.createElement('div');
            imgDiv.className = 'image-preview';
            imgDiv.innerHTML = `<img src="${dataUrl}"><button class="delete-img" onclick="deleteImage(this,'${file.name}')">x</button>`;
            const container = document.getElementById('hotelImages');
            if (container) container.appendChild(imgDiv);
        } catch (error) {
            console.error('Ошибка при кодировании файла:', file.name, error);
            alert(`Ошибка при обработке файла: ${file.name}`);
        }
    }
}

function deleteRoomImage(btn, idx) {
    if (btn && btn.parentElement) btn.parentElement.remove();

    if (idx < 0 || idx >= roomsData.length) {
        console.error("Комната с индексом", idx, "не найдена в roomsData");
        return;
    }

    const room = roomsData[idx];
    if (room) {
        room.file = null;
    }
}

function deleteRoom(uniqueId) {
    const index = roomsData.findIndex(r => r.uniqueId === uniqueId);
    if (index === -1) return;

    const room = roomsData[index];
    if (room.id) {
        deletedRooms.push(room.id);
    }

    roomsData.splice(index, 1);
    redrawRooms();
}

function redrawRooms() {
    const container = document.getElementById('roomsContainer');
    if (!container) return;
    container.innerHTML = '';
    roomsData.forEach(r => addRoom(r));
}

// =================== Meal ===================
function loadMeal(selectedMeals = []) {
    const select = document.getElementById('mealSelect');
    if (!select) return;
    select.innerHTML = '';
    availableMeals.forEach(meal => {
        const checked = selectedMeals.includes(meal) ? 'checked' : '';
        const label = document.createElement('label');
        label.innerHTML = `<input type="checkbox" value="${meal}" ${checked} onchange="toggleMeal(this)"> ${meal}`;
        select.appendChild(label);
    });
}

function toggleMeal(cb) {
    const val = cb.value;
    const index = availableMeals.indexOf(val);
    if (cb.checked) {
        for (let i = 0; i <= index; i++) {
            if (!selectedMeal.includes(availableMeals[i])) {
                selectedMeal.push(availableMeals[i]);
            }
        }
    } else {
        for (let i = index; i < availableMeals.length; i++) {
            selectedMeal = selectedMeal.filter(x => x !== availableMeals[i]);
        }
    }
    loadMeal(selectedMeal);
}

// =================== Services ===================
function loadServices(selectedIds = []) {
    const container = document.getElementById('servicesContainer');
    if (!container) return;
    container.innerHTML = '';

    fetch('/api/filters')
        .then(res => res.json())
        .then(data => {
            servicesList = data;
            const addedIds = new Set();
            data.forEach(s => {
                if (isNaN(s.id) || addedIds.has(s.id)) return;
                addedIds.add(s.id);

                const checked = selectedIds.includes(s.id) ? 'checked' : '';
                const label = document.createElement('label');
                label.innerHTML = `<input type="checkbox" value="${s.id}" ${checked} onchange="toggleService(this)"> ${s.service_Name} (${s.category_Name})`;
                container.appendChild(label);
                container.appendChild(document.createElement('br'));
            });

            selectedServices = [...selectedIds];
        })
        .catch(err => console.error(err));
}

function toggleService(cb) {
    const id = parseInt(cb.value);
    if (isNaN(id)) return;
    if (cb.checked) {
        if (!selectedServices.includes(id)) selectedServices.push(id);
    } else {
        selectedServices = selectedServices.filter(x => x !== id);
    }
}

// =================== Submit Hotel ===================
async function submitHotel() {
    const form = document.getElementById('hotelFormElement') || document.getElementById('hotelForm');
    if (!form) {
        alert('Форма не найдена');
        return;
    }

    const hotelCard = document.getElementById('hotelCard');
    let hotelCardHtml = hotelCard ? hotelCard.innerHTML : '';

    hotelCardHtml = cleanInlineStyles(hotelCardHtml);

    const name = document.getElementById('hotelName').value?.trim();
    const country = document.getElementById('hotelCountry').value?.trim();
    const city = document.getElementById('hotelCity').value?.trim();

    if (!name) {
        alert('Пожалуйста, введите название отеля');
        return;
    }

    if (!country) {
        alert('Пожалуйста, введите страну');
        return;
    }

    if (!city) {
        alert('Пожалуйста, введите город');
        return;
    }

    const requestData = {
        name: name,
        stars: parseInt(document.getElementById('hotelStars').value) || 0,
        rating: parseFloat(document.getElementById('hotelRating').value) || 0,
        city: city,
        country: country,
        description: document.getElementById('hotelDescriptionText').value?.trim() || '',
        about_Hotel: document.getElementById('hotelDescription').innerHTML || '',
        card: hotelCardHtml,
        meal: selectedMeal,
        servicesIds: selectedServices,
        deletedImages: deletedImages,
        deletedRooms: deletedRooms,
        photoFiles: hotelImagesFiles,
        rooms: roomsData.map(r => ({
            id: r.id || null,
            name: r.name?.trim() || '',
            price: parseFloat(r.price) || 0,
            description: r.description?.trim() || '',
            peopleCount: parseInt(r.peopleCount) || 1,
            photo: r.oldPhotoName || null,
            photoFile: r.file
        }))
    };

    if (!editingHotelId) {
        requestData.rooms = requestData.rooms.filter(room =>
            room.name || room.description || room.photoFile
        );
    }

    console.log('Отправляемые данные:', requestData);

    const submitBtn = form.querySelector('button[type="submit"]');
    const originalText = submitBtn.textContent;
    submitBtn.textContent = 'Сохранение...';
    submitBtn.disabled = true;

    try {
        const response = await fetch(form.dataset.url, {
            method: form.dataset.method || 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(requestData)
        });

        const data = await response.json();

        if (data.success) {
            alert('Отель успешно сохранен!');
            hideHotelForm();
            refreshHotelsTable();
        } else {
            const errorMessage = data.message || data.error || 'Неизвестная ошибка сервера';
            alert('Ошибка: ' + errorMessage);
            console.error('Ошибка сервера:', data);
        }
    } catch (err) {
        console.error('Ошибка при отправке:', err);
        alert('Ошибка сети при отправке данных. Проверьте подключение к интернету.');
    } finally {
        submitBtn.textContent = originalText;
        submitBtn.disabled = false;
    }
}

// =================== Clean Inline Styles ===================
function cleanInlineStyles(html) {
    if (!html) return '';

    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = html;

    const elementsWithStyle = tempDiv.querySelectorAll('[style]');
    elementsWithStyle.forEach(element => {
        element.removeAttribute('style');
    });

    return tempDiv.innerHTML;
}

// =================== Delete Hotel ===================
function deleteHotel(id) {
    if (!confirm('Удалить отель ' + id + '?')) return;
    fetch(`/adminapi/hotels/${id}`, { method: 'DELETE' })
        .then(res => res.json())
        .then(data => {
            if (data.success) refreshHotelsTable();
            else alert('Ошибка');
        });
}

// =================== Edit ===================
function editHotel(id) {
    fetch(`/adminapi/hotels/${id}`)
        .then(res => res.json())
        .then(data => {
            editingHotelId = data.id;

            // Настройка формы на PUT
            const form = document.getElementById('hotelFormElement') || document.getElementById('hotelForm');
            if (!form) {
                console.error('Форма hotelFormElement или hotelForm не найдена');
                return;
            }

            form.dataset.url = `/adminapi/hotels/${data.id}`;
            form.dataset.method = 'PUT';

            // Показ формы
            showHotelForm();

            // Заполняем основные поля
            const nameEl = document.getElementById('hotelName'); if (nameEl) nameEl.value = data.name || '';
            const starsEl = document.getElementById('hotelStars'); if (starsEl) starsEl.value = data.stars || '';
            const ratingEl = document.getElementById('hotelRating'); if (ratingEl) ratingEl.value = data.rating || '';
            const cityEl = document.getElementById('hotelCity'); if (cityEl) cityEl.value = data.city || '';
            const countryEl = document.getElementById('hotelCountry'); if (countryEl) countryEl.value = data.country || '';
            const descText = document.getElementById('hotelDescriptionText'); if (descText) descText.value = data.description || '';
            const desc = document.getElementById('hotelDescription'); if (desc) desc.innerHTML = data.about_Hotel || '';

            // --- Карточка отеля ---
            const hotelCard = document.getElementById('hotelCard');
            if (hotelCard) {
                if (data.card) {
                    hotelCard.innerHTML = data.card;
                }
                else if (data.hotel_card) {
                    hotelCard.innerHTML = data.hotel_card;
                }
                else {
                    hotelCard.innerHTML = '';
                }
            }

            // Фотографии (старые)
            hotelImagesFiles = [];
            deletedImages = [];
            const imagesDiv = document.getElementById('hotelImages');
            if (imagesDiv) imagesDiv.innerHTML = '';
            if (data.image_Names) {
                data.image_Names.forEach(name => {
                    const cleanName = name.replace(/^{|}$/g, '');
                    const div = document.createElement('div');
                    div.className = 'image-preview';
                    div.innerHTML = `<img src="/hotels/data/${data.country}/${data.city}/${data.id}/${cleanName}"><button class="delete-img" onclick="markImageForDeletion(this,'${cleanName}')">x</button>`;
                    imagesDiv.appendChild(div);
                });
            }

            // Комнаты
            roomsData = [];
            deletedRooms = [];
            const roomsContainer = document.getElementById('roomsContainer');
            if (roomsContainer) roomsContainer.innerHTML = '';
            if (data.hotel_Rooms) {
                data.hotel_Rooms.forEach((r, idx) => addRoom({
                    id: r.id,
                    name: r.name,
                    price: r.price,
                    description: r.description,
                    peopleCount: r.peopleCount || 1,
                    oldPhotoName: r.photo,
                    hotelId: r.hotel_Id,
                    country: data.country,
                    city: data.city
                }));
            }

            // Meal
            let meals = [];
            if (Array.isArray(data.meal)) meals = data.meal;
            else if (typeof data.meal === 'string') meals = data.meal.split(',').map(x => x.trim());
            selectedMeal = meals.filter(m => availableMeals.includes(m));
            loadMeal(selectedMeal);

            // Services
            selectedServices = (data.servicesIds || []).filter(id => !isNaN(id));
            loadServices(selectedServices);
        })
        .catch(error => {
            console.error('Ошибка при загрузке данных отеля:', error);
        });
}

function markImageForDeletion(btn, name) {
    if (btn && btn.parentElement) btn.parentElement.remove();
    deletedImages.push(name);
    hotelImagesFiles = hotelImagesFiles.filter(f => f.name !== name);
}

// =================== Refresh Table ===================
function refreshHotelsTable() {
    fetch('/adminapi/hotels')
        .then(res => res.text())
        .then(html => {
            const container = document.getElementById('hotelsTableContainer');
            if (container) container.innerHTML = html;
        });
}

// =================== On Load ===================
window.onload = function () {
    refreshHotelsTable();
};