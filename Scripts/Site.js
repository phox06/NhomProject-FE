document.addEventListener("DOMContentLoaded", function () {
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
    const hamburger = document.querySelector(".hamburger-menu");
    const navBar = document.querySelector(".header-nav-bar");

    if (hamburger && navBar) {
        hamburger.addEventListener("click", function () {
            navBar.classList.toggle("is-active");
        });
    }
});