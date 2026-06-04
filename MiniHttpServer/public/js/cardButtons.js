document.addEventListener('DOMContentLoaded', () => {
    // Делегирование кликов на кнопки внутри card-wrapper
    document.addEventListener('click', (e) => {
        const wrapper = e.target.closest('.card-wrapper');
        if (!wrapper) return;

        const button = e.target.closest('.btn', wrapper);
        if (!button) return;

        // Игнорируем лайки
        if (button.classList.contains('heart-btn')) return;

        const aboutCard = wrapper.querySelector('.aboutcard');
        const roomList = wrapper.querySelector('.roomlist');

        const target = button.dataset.target;

        if (target === 'aboutcard') {
            aboutCard?.classList.toggle('hide');
            roomList?.classList.add('hide');
        } else if (target === 'hotelrooms') {
            roomList?.classList.toggle('hide');
            aboutCard?.classList.add('hide');
        } else {
            // все остальные кнопки просто закрывают оба блока
            aboutCard?.classList.add('hide');
            roomList?.classList.add('hide');
        }
    });

    // Галерея и превью картинок
    function initGallery() {
        const wrappers = document.querySelectorAll('.card-wrapper');
        if (!wrappers.length) return false;

        let initialized = false;

        wrappers.forEach(wrapper => {
            const previewOverlay = wrapper.querySelector('.preview-overlay');
            const previewImage = wrapper.querySelector('.preview-image');
            const galleryImages = wrapper.querySelectorAll('.gallery-item img');

            if (!previewOverlay || !previewImage || galleryImages.length === 0) return;

            galleryImages.forEach(img => {
                if (!img.dataset.listenerAttached) {
                    img.addEventListener('click', (e) => {
                        previewImage.src = e.target.src;
                        previewOverlay.style.display = 'flex';
                    });
                    img.dataset.listenerAttached = 'true';
                }
            });

            previewOverlay.addEventListener('click', (e) => {
                if (e.target === previewOverlay) {
                    previewOverlay.style.display = 'none';
                    previewImage.src = '';
                }
            });

            initialized = true;
        });

        return initialized;
    }

    const galleryInterval = setInterval(() => {
        if (initGallery()) {
            console.log('✅ Галерея и превью инициализированы');
            clearInterval(galleryInterval);
        }
    }, 300);
});
