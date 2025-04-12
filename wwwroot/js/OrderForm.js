$(document).ready(function () {
    $("#orderForm").on("submit", function(e) {
        e.preventDefault();

        // Record the start time
        var startTime = new Date().getTime();
        var minDisplayTime = 1000; // Minimum spinner display time in milliseconds (1 second)

        $("#spinner").fadeIn();

        $.ajax({
            type: "POST",
            url: $(this).attr("action"),
            data: $(this).serialize(),
            success: function(response) {
                $("#orderConfirmationContainer").html(response);
            },
            error: function(xhr, status, error) {
                alert("An error occurred while placing your order. Please try again.");
            },
            complete: function() {
                var elapsedTime = new Date().getTime() - startTime;
                var delay = Math.max(minDisplayTime - elapsedTime, 0); // Ensure non-negative delay
                setTimeout(function() {
                    $("#spinner").fadeOut();
                }, delay);
            }
        });
    });

    window.addOrderItem = function () {
        let div = document.createElement("div");
        div.className = "orderItem";
        div.innerHTML = `<label class="control-label">Product:</label>
            <select name="productIds" class="form-control">
                ${window.orderProductsOptionsHTML}
            </select>
            <label class="control-label">Quantity:</label>
            <input type="number" name="quantities" class="form-control" value="1" min="1" />`;
        document.getElementById("orderItems").appendChild(div);
    };
});