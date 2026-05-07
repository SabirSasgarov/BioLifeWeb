$(document).ready(function () {
    // Add to cart AJAX
    $('.add-to-cart-form').on('submit', function (e) {
        e.preventDefault();

        var $form = $(this);
        var url = $form.attr('action');
        var data = $form.serialize();

        $.ajax({
            type: 'POST',
            url: url,
            data: data,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (response) {
                if (response.success) {
                    // Update mini cart count if it exists
                    $('.minicart-block .qty').text(response.count);

                    // Try an unobtrusive toast message instead of alert
                    var $toast = $('<div class="cart-toast" style="position:fixed; bottom:20px; right:20px; background-color:#73b22a; color:white; padding:15px 25px; border-radius:5px; z-index:9999; box-shadow: 0px 4px 6px rgba(0,0,0,0.2); font-weight: bold;">Item added to cart!</div>');
                    $('body').append($toast);
                    $toast.hide().fadeIn().delay(2500).fadeOut(function() { $(this).remove(); });
                }
            },
            error: function () {
                // If unauthorized or failed, you might want to redirect to login
                window.location.href = '/Account/Login';
            }
        });
    });

    // Update quantity AJAX
    $('.quantity-form .input-qty').on('change', function () {
        var $input = $(this);
        var $form = $input.closest('form');
        var url = $form.attr('action');
        var data = $form.serialize();

        // Find parent row to update subtotal and total locally without refresh
        var $row = $input.closest('tr');

        $.ajax({
            type: 'POST',
            url: url,
            data: data,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (response) {
                if (response.success) {
                    // Update mini cart count
                    $('.minicart-block .qty').text(response.count);
                    // Update item subtotal
                    $row.find('.product-subtotal').text('?' + response.itemTotal);
                    // Update basket total
                    $('.cart-total strong').text('?' + response.cartTotal);
                }
            }
        });
    });
});