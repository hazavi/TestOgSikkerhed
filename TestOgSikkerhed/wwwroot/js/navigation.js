// Navigation toggle functionality
document.addEventListener('DOMContentLoaded', function() {
    const navToggler = document.getElementById('navToggler');
    const navMenu = document.getElementById('navMenu');
    
    if (navToggler && navMenu) {
        navToggler.addEventListener('click', function() {
            navToggler.classList.toggle('active');
            navMenu.classList.toggle('active');
        });
        
        // Close menu when clicking outside
        document.addEventListener('click', function(event) {
            const isClickInside = navToggler.contains(event.target) || navMenu.contains(event.target);
            
            if (!isClickInside && navMenu.classList.contains('active')) {
                navToggler.classList.remove('active');
                navMenu.classList.remove('active');
            }
        });
        
        // Close menu when window is resized to desktop size
        window.addEventListener('resize', function() {
            if (window.innerWidth > 768 && navMenu.classList.contains('active')) {
                navToggler.classList.remove('active');
                navMenu.classList.remove('active');
            }
        });
    }
});