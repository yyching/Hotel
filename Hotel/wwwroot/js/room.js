const rangeInput = document.querySelectorAll(".range-input input"),
    priceInput = document.querySelectorAll(".price-input input"),
    range = document.querySelector(".slider .progress");
let priceGap = 10; // Adjusted gap for smaller range

// set the slider range
function initializeSlider() {
    const minVal = parseInt(rangeInput[0].value);
    const maxVal = parseInt(rangeInput[1].value);

    range.style.left = ((minVal - 100) / (rangeInput[0].max - 100)) * 100 + "%";
    range.style.right = 100 - ((maxVal - 100) / (rangeInput[1].max - 100)) * 100 + "%";
}

// load the slider when page loading
window.addEventListener('load', () => {
    const urlParams = new URLSearchParams(window.location.search);
    const minPrice = urlParams.get('minPrice');
    const maxPrice = urlParams.get('maxPrice');

    if (minPrice) {
        priceInput[0].value = minPrice;
        rangeInput[0].value = minPrice;
    }
    if (maxPrice) {
        priceInput[1].value = maxPrice;
        rangeInput[1].value = maxPrice;
    }

    initializeSlider();
});

// set a time to avoid the page keep loading
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// pass the min price and max price
const updatePriceFilter = debounce(() => {
    const minPrice = priceInput[0].value;
    const maxPrice = priceInput[1].value;

    const url = new URL(window.location.href);
    const params = new URLSearchParams(url.search);

    params.set('minPrice', minPrice);
    params.set('maxPrice', maxPrice);

    // send the ajax request
    fetch(`/Home/RoomPage?${params.toString()}`, {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => response.text())
        .then(html => {
            document.querySelector('.hotel-table').innerHTML = html;
            // update the url but not refresh the page
            window.history.pushState({}, '', `${url.pathname}?${params.toString()}`);
        })
        .catch(error => console.error('Error:', error));
}, 1000);

priceInput.forEach((input) => {
    input.addEventListener("input", (e) => {
        let minPrice = parseInt(priceInput[0].value),
            maxPrice = parseInt(priceInput[1].value);

        if (maxPrice - minPrice >= priceGap && maxPrice <= rangeInput[1].max) {
            if (e.target.className === "input-min") {
                rangeInput[0].value = minPrice;
                range.style.left = ((minPrice - 100) / (rangeInput[0].max - 100)) * 100 + "%";
            } else {
                rangeInput[1].value = maxPrice;
                range.style.right = 100 - ((maxPrice - 100) / (rangeInput[1].max - 100)) * 100 + "%";
            }
            updatePriceFilter();
        }
    });
});

rangeInput.forEach((input) => {
    input.addEventListener("input", (e) => {
        let minVal = parseInt(rangeInput[0].value),
            maxVal = parseInt(rangeInput[1].value);

        if (maxVal - minVal < priceGap) {
            if (e.target.className === "range-min") {
                rangeInput[0].value = maxVal - priceGap;
            } else {
                rangeInput[1].value = minVal + priceGap;
            }
        } else {
            priceInput[0].value = minVal;
            priceInput[1].value = maxVal;
            range.style.left = ((minVal - 100) / (rangeInput[0].max - 100)) * 100 + "%";
            range.style.right = 100 - ((maxVal - 100) / (rangeInput[1].max - 100)) * 100 + "%";
            updatePriceFilter();
        }
    });
});

function handleThemeFilter(checkbox) {
    // get the selected theme
    const selectedThemes = Array.from(document.querySelectorAll('.theme-checkbox:checked'))
        .map(checkbox => checkbox.value)
        .join(',');

    // get the current filter
    const url = new URL(window.location.href);
    const currentParams = new URLSearchParams(url.search);
    
    // update the theme
    if (selectedThemes) {
        currentParams.set('themes', selectedThemes);
    } else {
        currentParams.delete('themes');
    }

    // send ajax request
    fetch(`/Home/RoomPage?${currentParams.toString()}`, {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.text())
    .then(html => {
        document.querySelector('.hotel-table').innerHTML = html;
        // update thee url but no page refresh
        window.history.pushState({}, '', `${url.pathname}?${currentParams.toString()}`);
    })
    .catch(error => console.error('Error:', error));
}

function handleCategoryFilter(checkbox) {
    // get the selected category
    const selectedCategories = Array.from(document.querySelectorAll('.category-checkbox:checked'))
        .map(checkbox => checkbox.value)
        .join(',');

    // get the current filter
    const url = new URL(window.location.href);
    const currentParams = new URLSearchParams(url.search);
    
    // update the category
    if (selectedCategories) {
        currentParams.set('category', selectedCategories);
    } else {
        currentParams.delete('category');
    }

    // send ajax request
    fetch(`/Home/RoomPage?${currentParams.toString()}`, {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.text())
    .then(html => {
        document.querySelector('.hotel-table').innerHTML = html;
        // update thee url but no page refresh
        window.history.pushState({}, '', `${url.pathname}?${currentParams.toString()}`);
    })
    .catch(error => console.error('Error:', error));
}

// clear all the filter
function clearAllFilters() {
    const url = new URL(window.location.href);
    const params = new URLSearchParams(url.search);

    // only delete the filter
    params.delete('themes');
    params.delete('category');
    params.delete('minPrice');
    params.delete('maxPrice');

    // reset the price
    priceInput[0].value = 100;
    priceInput[1].value = rangeInput[1].max;
    rangeInput[0].value = 100;
    rangeInput[1].value = rangeInput[1].max;
    initializeSlider();

    // reset theme checkboxes
    document.querySelectorAll('.theme-checkbox').forEach(checkbox => {
        checkbox.checked = false;
    });

    // reset room category checkboxes
    document.querySelectorAll('.category-checkbox').forEach(checkbox => {
        checkbox.checked = false;
    });

    // send ajax request
    fetch(`/Home/RoomPage?${params.toString()}`, {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(response => response.text())
        .then(html => {
            document.querySelector('.hotel-table').innerHTML = html;
            // update the url but no oage reload
            window.history.pushState({}, '', `${url.pathname}?${params.toString()}`);
        })
        .catch(error => console.error('Error:', error));
}

document.getElementById('clear-all').addEventListener('click', clearAllFilters);
