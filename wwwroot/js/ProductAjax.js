// Load products dynamically based on search
function loadProducts(searchString) {
    $("#spinner").fadeIn();

    $.ajax({
        url: '/ProductManagement/Product/Search?searchString=' + searchString,
        method: "GET",
        success: function (data) {
            $('#productsList').html(data); // Update products
            $("#spinner").fadeOut();
        },
        error: function (xhr, status, error) {
            $("#spinner").fadeOut();
            alert("Error loading products: " + xhr.responseText);
        }
    });
}

$(document).ready(function () {
    // Remove global ajaxStart and ajaxStop
    // Manual control instead

    // Trigger search when user types
    $('#searchBox').on('input', function () {
        var searchString = $(this).val();
        loadProducts(searchString);
    });

    // AJAX Add Product form submit
    $('#addProductForm').submit(function (e) {
        e.preventDefault();

        $("#spinner").fadeIn(); // Show spinner when submitting

        var formData = {
            Name: $('#addProductForm input[name="Name"]').val(),
            Description: $('#addProductForm input[name="Description"]').val(),
            Price: parseFloat($('#addProductForm input[name="Price"]').val()),
            Quantity: parseInt($('#addProductForm input[name="Quantity"]').val()),
            LowStockThreshold: parseInt($('#addProductForm input[name="LowStockThreshold"]').val()),
            CategoryId: parseInt($('#addProductForm select[name="CategoryId"]').val())
        };

        $.ajax({
            url: '/ProductManagement/Product/Add',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.success) {
                    alert("Product added successfully!");
                    $('#addProductForm')[0].reset();
                    loadProducts(""); // Reload product list
                } else {
                    alert(response.message);
                }
                $("#spinner").fadeOut(); // Hide spinner after add
            },
            error: function (xhr, status, error) {
                $("#spinner").fadeOut(); // Hide spinner if error
                alert("Error adding product: " + xhr.responseText);
            }
        });
    });
});