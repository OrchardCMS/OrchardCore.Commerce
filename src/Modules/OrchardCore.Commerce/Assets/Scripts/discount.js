const prices = Array.from(document.querySelectorAll('.tax-part-gross-price-value, .price-part-price-field-value'));

function strikeOutPrice(price) {
    price.innerHTML = "<del class=\"text-danger\">" + price.textContent + "</del>";
}

prices.forEach(price => strikeOutPrice(price));
