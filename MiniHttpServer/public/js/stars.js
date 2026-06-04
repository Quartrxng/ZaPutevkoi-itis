document.addEventListener('DOMContentLoaded', function () {
    const filterStars = document.querySelector('.filter-stars');
    if (!filterStars) return;

    const starSvgs = filterStars.querySelectorAll('.star-svg');
    const hiddenSpan = filterStars.querySelector('.filter--select__content');

    if (!starSvgs || !hiddenSpan) return;

    const initialRating = parseInt(hiddenSpan.textContent) || 0;
    updateStars(initialRating);

    starSvgs.forEach((star, index) => {
        star.addEventListener('mouseenter', function () {
            starSvgs.forEach(s => s.classList.remove('filled'));
            for (let i = 0; i <= index; i++) {
                starSvgs[i].classList.add('filled');
            }
        });

        star.addEventListener('click', function () {
            const selectedRating = index + 1;
            updateStars(selectedRating);
        });
    });

    filterStars.addEventListener('mouseleave', function () {
        const currentRating = parseInt(hiddenSpan.textContent) || 0;
        updateStars(currentRating);
    });

    function updateStars(rating) {
        starSvgs.forEach((star, index) => {
            if (index < rating) {
                star.classList.add('filled');
            } else {
                star.classList.remove('filled');
            }
        });
        hiddenSpan.textContent = rating.toString();
    }
});