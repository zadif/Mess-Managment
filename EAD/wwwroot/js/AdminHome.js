document.addEventListener("DOMContentLoaded", () => {
    const cards = document.querySelectorAll('.card');

    cards.forEach((card, index) => {
        // Set initial state via JS to ensure it works even if CSS differs
        card.style.opacity = "0";
        card.style.transform = "translateY(20px)";

        // Apply transition
        setTimeout(() => {
            card.style.transition = "opacity 0.6s cubic-bezier(0.25, 1, 0.5, 1), transform 0.6s cubic-bezier(0.25, 1, 0.5, 1)";
            card.style.opacity = "1";
            card.style.transform = "translateY(0)";
        }, index * 100); // 100ms delay per card
    });
});