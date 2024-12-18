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
    
    url.search = params.toString();
    window.location.href = url.toString();
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
    const urlParams = new URLSearchParams(window.location.search);

    // capture the current theme
    let themes = urlParams.get('themes') ? urlParams.get('themes').split(',') : [];

    if (checkbox.checked) {
        // if selected add the theme
        if (!themes.includes(checkbox.value)) {
            themes.push(checkbox.value);
        }
    } else {
        themes = themes.filter(theme => theme !== checkbox.value);
    }

    // update url
    if (themes.length > 0) {
        urlParams.set('themes', themes.join(','));
    } else {
        urlParams.delete('themes');
    }

    // new url
    window.location.href = window.location.pathname + '?' + urlParams.toString();
}

function handleCategoryFilter(checkbox) {
    const urlParams = new URLSearchParams(window.location.search);

    // capture the current category
    let category = urlParams.get('category') ? urlParams.get('category').split(',') : [];

    if (checkbox.checked) {
        // if selected add the category
        if (!category.includes(checkbox.value)) {
            category.push(checkbox.value);
        }
    } else {
        category = category.filter(category => category !== checkbox.value);
    }

    // update url
    if (category.length > 0) {
        urlParams.set('category', category.join(','));
    } else {
        urlParams.delete('category');
    }

    // new url
    window.location.href = window.location.pathname + '?' + urlParams.toString();
}