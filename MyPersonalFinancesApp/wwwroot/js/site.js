// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Example: A function to handle the "Make it regular" checkbox
function setupRegularPaymentToggle() {
    const checkbox = document.getElementById('make-regular-checkbox');
    const regularFields = document.getElementById('regular-payment-fields');

    if (checkbox && regularFields) {
        checkbox.addEventListener('change', function () {
            if (this.checked) {
                regularFields.style.display = 'block';
            } else {
                regularFields.style.display = 'none';
            }
        });
    }
}
