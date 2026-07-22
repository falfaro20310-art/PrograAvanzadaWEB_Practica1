// Carrusel de recomendaciones del dashboard

document.addEventListener('DOMContentLoaded', function () {

    if (!document.getElementById('swiper-1')) return;

    new Swiper('#swiper-1', {
        loop: true,
        pagination: { el: '.swiper-pagination', clickable: true },
        autoplay: { delay: 3000 }
    });

});
