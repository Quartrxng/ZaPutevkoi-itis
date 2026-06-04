let interval = 25000;
let intervalId;

document.addEventListener('DOMContentLoaded', function () {
    const slides = document.querySelectorAll('.slide');
    const prevButton = document.querySelector('.slider--nav__button--left .button--ripple');
    const nextButton = document.querySelector('.slider--nav__button--right .button--ripple');
    const indicatorButtons = document.querySelectorAll('.slider--indicator');

    let currentIndex = 0;
    const totalSlides = slides.length;

    // Инициализация слайдов
    slides.forEach((slide, index) => {
        const slideDiv = slide.querySelector('div[style*="transform-origin"] > div');
        if (!slideDiv) return;
        slideDiv.style.position = 'absolute';
        slideDiv.style.top = '0';
        slideDiv.style.left = '0';
        slideDiv.style.width = '100%';
        slideDiv.style.height = '100%';
        slideDiv.style.transition = 'transform 0.8s ease, opacity 0.8s ease';
        slideDiv.style.transform = index === currentIndex ? 'translateX(0)' : 'translateX(100%)';
        slideDiv.style.opacity = index === currentIndex ? '1' : '0';
        slideDiv.style.zIndex = index === currentIndex ? '1' : '0';
        slideDiv.style.display = 'block';
    });

    function goToSlide(nextIndex) {
        if (nextIndex === currentIndex) return;

        const direction = nextIndex > currentIndex || (currentIndex === totalSlides - 1 && nextIndex === 0) ? 1 : -1;

        slides.forEach((slide, index) => {
            const slideDiv = slide.querySelector('div[style*="transform-origin"] > div');
            if (!slideDiv) return;

            if (index === nextIndex) {
                // Ставим новый слайд за пределы видимости
                slideDiv.style.transition = 'none';
                slideDiv.style.transform = `translateX(${100 * direction}%)`;
                slideDiv.style.opacity = '1';
                slideDiv.style.zIndex = '1';
                slideDiv.style.display = 'block';

                // Применяем анимацию в следующем кадре с более быстрой скоростью
                requestAnimationFrame(() => {
                    slideDiv.style.transition = 'transform 0.4s ease, opacity 0.4s ease'; // ускорение
                    slideDiv.style.transform = 'translateX(0)';
                });
            }

            if (index === currentIndex) {
                slideDiv.style.transition = 'transform 0.4s ease, opacity 0.4s ease'; // ускорение
                slideDiv.style.transform = `translateX(${-100 * direction}%)`;
                slideDiv.style.opacity = '0';
                slideDiv.style.zIndex = '0';
            }
        });


        // Обновляем индикаторы
        indicatorButtons.forEach((indicator, index) => {
            indicator.classList.toggle('slider--indicator__active', index === nextIndex);
        });

        currentIndex = nextIndex;
        resetInterval();
    }


    function goToNextSlide() {
        let nextIndex = (currentIndex + 1) % totalSlides;
        goToSlide(nextIndex);
    }

    function goToPrevSlide() {
        let nextIndex = (currentIndex - 1 + totalSlides) % totalSlides;
        goToSlide(nextIndex);
    }

    function resetInterval() {
        clearInterval(intervalId);
        intervalId = setInterval(goToNextSlide, interval);
    }

    // Кнопки
    nextButton?.addEventListener('click', goToNextSlide);
    prevButton?.addEventListener('click', goToPrevSlide);

    // Индикаторы
    indicatorButtons.forEach((indicator, index) => {
        indicator.addEventListener('click', () => goToSlide(index));
    });

    // Автоплей
    intervalId = setInterval(goToNextSlide, interval);
});
