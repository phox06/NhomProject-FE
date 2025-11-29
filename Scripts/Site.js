document.addEventListener("DOMContentLoaded", function () {
    // --- 1. CAROUSEL CODE (KEEP THIS AS IS) ---
    const promoCarousel = document.querySelector(".promo-carousel");

    if (promoCarousel) {
        const swiper = new Swiper('.promo-carousel', {
            direction: 'horizontal',
            loop: true,
            autoplay: {
                delay: 5000,
                disableOnInteraction: false,
            },
            pagination: {
                el: '.swiper-pagination',
                clickable: true,
            },
            navigation: {
                nextEl: '.swiper-button-next',
                prevEl: '.swiper-button-prev',
            },
        });
    }

    // --- 2. REPLACE THE OLD MENU CODE WITH THIS NEW SECTION ---
    const hamburger = document.querySelector(".hamburger-menu");
    const navBar = document.querySelector(".header-nav-bar");
    // Select the new elements we added to HTML
    const overlay = document.getElementById("menuOverlay");
    const closeBtn = document.getElementById("closeMenuBtn");

    function toggleMenu() {
        if (navBar) navBar.classList.toggle("is-active");
        if (overlay) overlay.classList.toggle("is-active");

        // Lock body scroll when menu is open
        if (navBar && navBar.classList.contains("is-active")) {
            document.body.style.overflow = "hidden";
        } else {
            document.body.style.overflow = "";
        }
    }

    if (hamburger) {
        hamburger.addEventListener("click", toggleMenu);
    }

    if (closeBtn) {
        closeBtn.addEventListener("click", toggleMenu);
    }

    if (overlay) {
        overlay.addEventListener("click", toggleMenu);
    }
});