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