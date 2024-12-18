const rangeInput = document.querySelectorAll(".range-input input"),
    priceInput = document.querySelectorAll(".price-input input"),
    range = document.querySelector(".slider .progress");
let priceGap = 10; // Adjusted gap for smaller range

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