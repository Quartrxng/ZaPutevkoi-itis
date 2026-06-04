document.addEventListener("DOMContentLoaded", function () {
    const starBlocks = document.querySelectorAll(".stars");

    starBlocks.forEach(block => {
        const starCount = parseInt(block.textContent.trim());
        block.innerHTML = "";

        if (isNaN(starCount)) return;

        for (let i = 0; i < starCount; i++) {
            const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
            svg.setAttribute("width", "13");
            svg.setAttribute("height", "13");
            svg.setAttribute("viewBox", "0 0 24 24");
            svg.setAttribute("class", "star-icon");
            svg.setAttribute("fill", "#ff6856");

            svg.innerHTML =
                '<path d="M12 .587l3.668 7.568 8.332 1.151-6.064 5.828 1.48 8.279-7.416-3.967-7.417 3.967 1.481-8.279-6.064-5.828 8.332-1.151z"></path>';

            block.appendChild(svg);
        }
    });
});
