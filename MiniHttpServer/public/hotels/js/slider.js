document.addEventListener('DOMContentLoaded', function () {
    const sliders = document.querySelectorAll('.slider-images');

    sliders.forEach(slider => {
        const hotelId = slider.id.replace('hotel-slider-', '');
        const prevBtn = document.querySelector(`[data-slider="hotel-slider-${hotelId}"][data-direction="prev"]`);
        const nextBtn = document.querySelector(`[data-slider="hotel-slider-${hotelId}"][data-direction="next"]`);
        const dotsContainer = document.getElementById(`hotel-dots-${hotelId}`);

        const images = slider.querySelectorAll('.slider-image');
        const totalImages = images.length;

        if (totalImages === 0) return;

        let currentIndex = 0;
        for (let i = 0; i < totalImages; i++) {
            const dot = document.createElement('div');
            dot.classList.add('slider-dot');
            if (i === 0) dot.classList.add('active');
            dot.addEventListener('click', () => goToSlide(i));
            dotsContainer.appendChild(dot);
        }

        const dots = dotsContainer.querySelectorAll('.slider-dot');

        function updateSlider() {
            slider.style.transform = `translateX(-${currentIndex * 100}%)`;

            dots.forEach((dot, index) => {
                dot.classList.toggle('active', index === currentIndex);
            });
        }

        function goToSlide(index) {
            currentIndex = index;
            updateSlider();
        }

        function nextSlide() {
            currentIndex = (currentIndex + 1) % totalImages;
            updateSlider();
        }

        function prevSlide() {
            currentIndex = (currentIndex - 1 + totalImages) % totalImages;
            updateSlider();
        }
        if (prevBtn) prevBtn.addEventListener('click', prevSlide);
        if (nextBtn) nextBtn.addEventListener('click', nextSlide);

        setInterval(nextSlide, 15000);

        document.addEventListener('keydown', function (e) {
            if (e.key === 'ArrowRight') {
                nextSlide();
            } else if (e.key === 'ArrowLeft') {
                prevSlide();
            }
        });
    });
});