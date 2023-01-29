function strikeOutPrices(prices) {
    Array.from(prices)
        .forEach((price) => price.innerHTML = `<del class="text-danger">${price.innerHTML.trim()}</del>`);
}
