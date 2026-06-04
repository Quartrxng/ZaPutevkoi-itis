document.addEventListener('DOMContentLoaded', function() {
    const emailInput = document.querySelector('.Banner--input__container');

    if (!emailInput) {
        console.warn('Element with class .Banner--input__container not found');
        return;
    }

    function addFocusClass() {
        emailInput.classList.add('mui-focused');
    }

    function removeFocusClass() {
        emailInput.classList.remove('mui-focused');
    }

    emailInput.addEventListener('focusin', addFocusClass);
    emailInput.addEventListener('focusout', removeFocusClass);
});