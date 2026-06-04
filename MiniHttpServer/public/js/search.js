document.addEventListener('DOMContentLoaded', function () {
    const searchButton = document.querySelector('.main--searchButton');
    if (!searchButton) return console.error('Кнопка main--searchButton не найдена');

    searchButton.addEventListener('click', function () {
        const searchContent = document.querySelector('.tour--search__content');
        const contentType = searchContent?.querySelector('.SearchInputResultItemType')?.textContent?.trim() || '';
        const MainContent = searchContent?.querySelector('.tour--search__maincontent');

        if (!searchContent || !MainContent || !contentType || !['hotel', 'city', 'country'].includes(contentType)) {
            const searchFilter = document.querySelector('.search--filter');
            if (searchFilter) {
                const originalOutline = searchFilter.style.outline;
                const originalOutlineOffset = searchFilter.style.outlineOffset;
                searchFilter.style.outline = '1px solid red';
                searchFilter.style.outlineOffset = '-1px';
                setTimeout(() => {
                    searchFilter.style.outline = originalOutline;
                    searchFilter.style.outlineOffset = originalOutlineOffset;
                }, 3000);
            }
            return;
        }

        const name = MainContent.textContent || 'none';
        const type = contentType;
        const duration = document.querySelector('.duration--filter__select .main--select__content')?.title || 'none';
        const tourists = document.querySelector('.TVTouristsSelect .main--select__content')?.title || 'none';
        const meal = document.querySelector('.filter-meal .filter--select__content')?.title || 'none';
        const stars = document.querySelector('.filter-stars .filter--select__content')?.textContent || '0';
        const rating = document.querySelector('.filter-rating .filter--select__content')?.title || 'none';
        const detail = searchContent?.querySelector('.tour--search__detailcontent')?.textContent || 'none';
        const service = document.querySelector('.service-filter .filter--select__content')?.title || 'none';

        const requestData = { name, type, duration, tourists };
        if (type === 'city' || type === 'country') {
            requestData.stars = stars;
            requestData.meal = meal;
            requestData.service = service;
            requestData.rating = rating;
        }
        if (type !== 'country') {
            requestData.detail = detail;
        }


        fetch('api/searchHotel', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(requestData)
        })
            .then(response => response.text())
            .then(html => {
                function formatPrice(priceText) {
                    return priceText.replace(/\d+/g, function (match) {
                        return match.replace(/\B(?=(\d{3})+(?!\d))/g, " ");
                    });
                }

                const tempDiv = document.createElement('div');
                tempDiv.innerHTML = html;

                tempDiv.querySelectorAll('.price').forEach(el => {
                    el.textContent = formatPrice(el.textContent);
                });
                tempDiv.querySelectorAll('.star-badge').forEach(badge => {
                    let starCount = parseInt(badge.dataset.stars || badge.textContent || '0');
                    if (isNaN(starCount)) starCount = 0;
                    badge.innerHTML = '';
                    for (let i = 0; i < starCount; i++) {
                        const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
                        svg.setAttribute("width", "14");
                        svg.setAttribute("height", "14");
                        svg.setAttribute("viewBox", "0 0 24 24");
                        svg.setAttribute("class", "star-icon");
                        svg.innerHTML = '<path d="M12 .587l3.668 7.568 8.332 1.151-6.064 5.828 1.48 8.279-7.416-3.967-7.417 3.967 1.481-8.279-6.064-5.828 8.332-1.151z"></path>';
                        badge.appendChild(svg);
                    }
                });
                const tourSearchContainer = document.getElementById('toursearch');
                if (tourSearchContainer) {
                    let cardContainer = tourSearchContainer.querySelector('.card-container');
                    if (!cardContainer) {
                        cardContainer = document.createElement('div');
                        cardContainer.className = 'card-container';
                        tourSearchContainer.appendChild(cardContainer);
                    }
                    cardContainer.innerHTML = tempDiv.innerHTML;
                    addLoadMoreButton(requestData);
                } else {
                    console.error('Элемент с id="toursearch" не найден');
                }
            })
            .catch(error => console.error('Ошибка:', error));
    });
    function addLoadMoreButton(requestData) {
        const tourSearchContainer = document.getElementById('toursearch');
        if (!tourSearchContainer) return;

        const oldBtn = tourSearchContainer.querySelector('.morebutton--container');
        if (oldBtn) oldBtn.remove();

        const btnContainer = document.createElement('div');
        btnContainer.className = "morebutton--container";

        const btn = document.createElement('div');
        btn.className = "morebutton";
        btn.textContent = "Найти больше предложений";

        btn.dataset.startValue = "10";

        btnContainer.appendChild(btn);
        tourSearchContainer.appendChild(btnContainer);

        btn.addEventListener('click', function () {

            let startValue = parseInt(btn.dataset.startValue) || 10;

            const sendData = {
                name: requestData.name,
                type: requestData.type || "Hotel",
                duration: parseInt(requestData.duration) || 1,
                tourists: parseInt(requestData.tourists) || 3,
                stars: parseInt(requestData.stars) || 1,
                meal: requestData.meal || "Любой",
                service: requestData.service || "0",
                detail: requestData.detail || "",
                startValue: startValue,
                rating: requestData.rating || "0.0"
            };

            fetch('api/searchHotelMore', {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(sendData)
            })
                .then(response => response.text())
                .then(html => {

                    const tempDiv = document.createElement('div');
                    tempDiv.innerHTML = html;

                    tempDiv.querySelectorAll('.price').forEach(el => {
                        el.textContent = el.textContent.replace(/\d+/g, m =>
                            m.replace(/\B(?=(\d{3})+(?!\d))/g, " ")
                        );
                    });

                    tempDiv.querySelectorAll('.star-badge').forEach(badge => {
                        let starCount = parseInt(badge.dataset.stars || badge.textContent || '0');
                        if (isNaN(starCount)) starCount = 0;
                        badge.innerHTML = '';
                        for (let i = 0; i < starCount; i++) {
                            const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
                            svg.setAttribute("width", "14");
                            svg.setAttribute("height", "14");
                            svg.setAttribute("viewBox", "0 0 24 24");
                            svg.setAttribute("class", "star-icon");
                            svg.innerHTML = '<path d="M12 .587l3.668 7.568 8.332 1.151-6.064 5.828 1.48 8.279-7.416-3.967-7.417 3.967 1.481-8.279-6.064-5.828 8.332-1.151z"></path>';
                            badge.appendChild(svg);
                        }
                    });

                    const cardContainer = tourSearchContainer.querySelector('.card-container');
                    if (cardContainer) {
                        cardContainer.insertAdjacentHTML('beforeend', tempDiv.innerHTML);
                    }

                    startValue += 10;
                    btn.dataset.startValue = startValue.toString();

                    tourSearchContainer.appendChild(btnContainer);
                })
                .catch(err => console.error("Ошибка догрузки:", err));
        });
    }


});
