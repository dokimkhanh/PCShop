$(document).ready(function () {
    ShowCount();
    $('body').on('click', '.btnAddToCart', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var quantity = 1;
        var tempQuantity = $('#quantity_value').text()
        if (tempQuantity != '') {
            quantity = parseInt(tempQuantity);
        }
        //alert(id + " - " + quantity);
        
        $.ajax({
            url: '/Cart/AddItem',
            type: 'POST',
            data: {
                id: id,
                quantity: quantity
            },
            success: function (res) {
                if (res.Success) {
                    //alert(res.msg);
                    toastr["success"](res.msg, "Thông báo")
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
                    }
                    ShowCount();
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
                        console.log(res.msg);
                        location.reload();
                        $('#row_' + id).remove();
                    }
                }
            });
        }
    });

    $('body').on('click', '.btnClear', function (e) {
        e.preventDefault();
        var conf = confirm('Làm mới giỏ hàng?');
        if (conf) {
            ClearCart();
            setTimeout(function () { location.reload(); }, 1500);
        }
    });

    $('body').on('change', '.inputUpdate', function (e) {
        var id = $(this).data('id');
        var quantity = $('#quantity_value_' + id).val();
        UpdateCart(id, quantity);
        location.reload();
    });

});

function ShowCount() {
    $.ajax({
        url: '/Cart/ShowCount',
        type: 'GET',
        success: function (res) {
            $('#checkout_items').html(res.Count);
            console.log('Load lại giỏ hàng');
        }
    });
}

function ClearCart() {
    $.ajax({
        url: '/Cart/ClearCart',
        type: 'POST',
        success: function (res) {
            if (res.Success) {
                ShowCount();
                console.log('Xoá hết giỏ hàng');
                toastr["error"]("Xoá hết sản phẩm thành công", "Thông báo")
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
                }
            }
        }
    });
}

function UpdateCart(id1, quantity1) {
    $.ajax({
        url: '/Cart/UpdateQuantityCartItem',
        type: 'POST',
        data: {
            id: id1,
            quantity: quantity1,
        },
        success: function (res) {
            if (res.Success) {
                ShowCount();
                console.log('Update giỏ hàng');
            }
        }
    });
}

function Okchua() {
    alert('Chuyen huong');
}

