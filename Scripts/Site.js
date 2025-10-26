document.addEventListener("DOMContentLoaded", function () {

    // --- CAROUSEL INITIALIZATION ---
    // Check if an element with the class '.promo-carousel' exists on the page
    const promoCarousel = document.querySelector(".promo-carousel");

    if (promoCarousel) {
        const swiper = new Swiper('.promo-carousel', {
            // Optional parameters
            direction: 'horizontal',
            loop: true, // Makes it loop continuously
            autoplay: {
                delay: 5000, // Autoplays every 5 seconds
                disableOnInteraction: false,
            },

            // Pagination (dots)
            pagination: {
                el: '.swiper-pagination',
                clickable: true,
            },

            // Navigation arrows
            navigation: {
                nextEl: '.swiper-button-next',
                prevEl: '.swiper-button-prev',
            },
        });
    }
    // --- END CAROUSEL ---


    // --- Your existing hamburger menu code ---
    const hamburger = document.querySelector(".hamburger-menu");
    const navBar = document.querySelector(".header-nav-bar");

    if (hamburger && navBar) {
        hamburger.addEventListener("click", function () {
            // This will add or remove the 'is-active' class on the nav bar
            navBar.classList.toggle("is-active");
        });
    }
});