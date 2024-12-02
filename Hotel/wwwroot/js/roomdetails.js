// Initialize Flatpickr
const dateInput = document.getElementById("date-picker");
const bookBtn = document.getElementById("book-btn");
const errorMsg = document.getElementById("error-msg");
let selectedDates = [];

flatpickr("#date-picker", {
    mode: "range",
    dateFormat: "F j, Y",
    minDate: "today",
    onChange: function (dates) {
        selectedDates = dates;
        errorMsg.textContent = "";
    },
});


// Food Menu Section Toggle
const navLinks = document.querySelectorAll('.menu-nav a');
const sections = document.querySelectorAll('.menu-section');

navLinks.forEach(link => {
    link.addEventListener('click', (e) => {
        e.preventDefault();
        sections.forEach(section => section.classList.remove('active'));
        const sectionID = link.getAttribute('data-section');
        document.getElementById(sectionID).classList.add('active');
    });
});

// Show Breakfast by default
document.getElementById('breakfast').classList.add('active');