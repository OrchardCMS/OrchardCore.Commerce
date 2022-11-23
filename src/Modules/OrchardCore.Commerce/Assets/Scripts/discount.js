const grossPrice = document.querySelector('span.tax-part-gross-price-value');
const defaultPrice = document.querySelector('span.price-part-price-field-value');

function strikeOutPrice(price) {
    price.style.color = 'red';
    price.innerHTML = "<del>" + price.textContent + "</del>";
}

if (grossPrice) {
    strikeOutPrice(grossPrice);
}
else if (defaultPrice) {
    strikeOutPrice(defaultPrice);
}
