﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
showInPopup = (url, title) => {
    $.ajax({
        type: "GET",
        url: url,
        success: function (res) {
            $("#form-modal .modal-body").html(res);
            $("#form-modal .modal-title").html(title);
            $("#form-modal").modal('show');
        }
    })
}

jQueryAjaxPost = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $("#view-all").html(res.html);
                    $("#form-modal .modal-body").html('');
                    $("#form-modal .modal-title").html('');
                    $("#form-modal").modal('hide');
                    $.notify('Submitted successfully', { globalPosition: 'top-center', className : 'success' });
                }
                else {
                    $("#form-modal .modal-body").html(res.html);
                }
            },
            error: function (err) {
                console.log(e);
            }
        })

    } catch (e) {
        console.log(e);
    }
    return false;
}

jQueryAjaxDelete = form => {
    if (confirm('Are you sure to delete this record?')) {
        try {
            $.ajax({
                type: 'POST',
                url: form.action,
                data: new FormData(form),
                contentType: false,
                processData: false,
                success: function (res) {
                    $("#view-all").html(res.html);
                    $.notify('Deleted successfully', { globalPosition: 'top-center', className: 'success' });
                },
                error: function (err) {
                    console.log(e);
                }
            })
        } catch (e) {
            console.log(e);
        }
    }
    return false;
}

    edit = form => {
        try {
    
        
            console.log("aaaa");
            var btn = $(this);
            var id = btn.data('id');
            $.ajax({
                url: "/Transection/Edit",
                data: { id: id },
                dataType: "json",
                type: "POST",
                success: function (response) {
                    console.log(response);
                    if (response.status != null) {
                        console.log(response.status)
                    }
                    else {
                        console.log("Ko co")
                    }
                }
            });
        }
    }

user.init();