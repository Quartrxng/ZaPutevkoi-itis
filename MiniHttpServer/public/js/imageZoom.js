function initPreview() {
    const aboutCards = document.querySelectorAll('.aboutcard');

    aboutCards.forEach((card) => {
        if (card.dataset.previewInitialized) return;
        card.dataset.previewInitialized = true;

        // Ищем overlay именно внутри этого card-wrapper
        const wrapper = card.closest('.card-wrapper');
        if (!wrapper) return;

        const overlay = wrapper.querySelector('.preview-overlay');
        if (!overlay) return;

        const previewImage = overlay.querySelector('.preview-image');
        const galleryImages = card.querySelectorAll('.gallery-item img');

        galleryImages.forEach((img) => {
            img.addEventListener('mouseenter', () => {
                previewImage.src = img.src;
                overlay.classList.add('visible');
            });
            img.addEventListener('mouseleave', () => {
                overlay.classList.remove('visible');
            });
        });

        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) {
                overlay.classList.remove('visible');
                previewImage.src = '';
            }
        });
    });
}

// Инициализация сразу и для динамически добавляемых элементов
const observer = new MutationObserver(() => {
    initPreview();
});

observer.observe(document.body, { childList: true, subtree: true });

document.addEventListener('DOMContentLoaded', () => {
    initPreview();
});
