// Load products dynamically
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

    if (currentPath.includes("/ProductManagement/Product/Search")) {
        $('#searchBox').on('input', function () {
            var searchString = $(this).val();
            loadProducts(searchString);
        });
    }
});