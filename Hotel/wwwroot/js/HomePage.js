document.addEventListener("DOMContentLoaded", () => {
    const navLinks = document.querySelectorAll(".menu-nav a");
    const sections = document.querySelectorAll(".menu-section");

    navLinks.forEach(link => {
        link.addEventListener("click", (e) => {
            e.preventDefault();

            // Get the target section ID from data-section attribute
            const targetSection = link.getAttribute("data-section");

            // Hide all sections
            sections.forEach(section => section.classList.remove("active"));

            // Show the selected section
            document.getElementById(targetSection).classList.add("active");
        });
    });

    // Show the first section (MAINS) by default
    document.getElementById("breakfast").classList.add("active");
});



const roomGrid = document.querySelector('.room-grid');
const scrollSpeed = 5;

roomGrid.addEventListener('wheel', (event) => {
    event.preventDefault();
    roomGrid.scrollLeft += event.deltaY * scrollSpeed;
});