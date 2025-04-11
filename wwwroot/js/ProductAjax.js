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
    var currentPath = window.location.pathname.toLowerCase();

    if (currentPath.includes("/productmanagement/product/search")) {
        $('#searchBox').on('input', function () {
            var searchString = $(this).val();
            loadProducts(searchString);
        });
    }

    if (currentPath.includes("/productmanagement/product/add")) {
        $('#addProductForm').submit(function (e) {
            e.preventDefault();

            $("#spinner").fadeIn();

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
                        loadProducts("");
                    } else {
                        alert(response.message);
                    }
                    $("#spinner").fadeOut();
                },
                error: function (xhr, status, error) {
                    $("#spinner").fadeOut();
                    alert("Error adding product: " + xhr.responseText);
                }
            });
        });
    }
});