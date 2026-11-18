$(document).ready(function () {
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": false,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };

    ShowCount();

    $('body').on('click', '.btnAddToCart', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var quantity = 1;
        var tempQuantity = $('#quantity_value').text();
        if (tempQuantity != '') {
            quantity = parseInt(tempQuantity);
        }
        if (isNaN(quantity) || quantity < 1) {
            quantity = 1;
        }

        $.ajax({
            url: '/Cart/AddItem',
            type: 'POST',
            data: {
                id: id,
                quantity: quantity
            },
            success: function (res) {
                if (res.Success) {
                    toastr["success"](res.msg, "Thông báo");
                    ShowCount();
                } else {
                    toastr["error"](res.msg || "Không thể thêm sản phẩm", "Thông báo");
                }
            }
        });
    });

    $('body').on('click', '.btnDelete', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var conf = confirm('Xoá sản phẩm này khỏi giỏ hàng?');
        if (conf) {
            $.ajax({
                url: '/Cart/DeleteItem',
                type: 'POST',
                data: {
                    id: id
                },
                success: function (res) {
                    if (res.Success) {
                        location.reload();
                    } else {
                        toastr["error"](res.msg || "Không thể xoá sản phẩm", "Thông báo");
                    }
                }
            });
        }
    });

    $('body').on('click', '.btnClear', function (e) {
        e.preventDefault();
        var conf = confirm('Làm mới giỏ hàng?');
        if (conf) {
            ClearCart(function () {
                location.reload();
            });
        }
    });

    $('body').on('change', '.inputUpdate', function (e) {
        var id = $(this).data('id');
        var quantity = parseInt($(this).val());
        if (isNaN(quantity) || quantity < 1) {
            toastr["error"]("Số lượng sản phẩm không hợp lệ", "Thông báo");
            return;
        }
        UpdateCart(id, quantity, function () {
            location.reload();
        });
    });

});

function ShowCount() {
    $.ajax({
        url: '/Cart/ShowCount',
        type: 'GET',
        success: function (res) {
            var count = parseInt(res.Count) || 0;
            if (count > 0) {
                $('#checkout_items').text(count).show();
            } else {
                $('#checkout_items').text('0').hide();
            }
        }
    });
}

function ClearCart(callback) {
    $.ajax({
        url: '/Cart/ClearCart',
        type: 'POST',
        success: function (res) {
            if (res.Success) {
                ShowCount();
                toastr["success"]("Xoá hết sản phẩm thành công", "Thông báo");
                if (typeof callback === 'function') {
                    callback();
                }
            } else {
                toastr["error"]("Không thể làm mới giỏ hàng", "Thông báo");
            }
        }
    });
}

function UpdateCart(id1, quantity1, callback) {
    $.ajax({
        url: '/Cart/UpdateQuantityCartItem',
        type: 'POST',
        data: {
            id: id1,
            quantity: quantity1
        },
        success: function (res) {
            if (res.Success) {
                ShowCount();
                if (typeof callback === 'function') {
                    callback();
                }
            } else {
                toastr["error"](res.msg || "Không thể cập nhật số lượng sản phẩm", "Thông báo");
            }
        }
    });
}