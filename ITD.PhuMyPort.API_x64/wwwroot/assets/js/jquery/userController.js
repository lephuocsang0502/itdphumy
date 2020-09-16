var user = {
    init: function () {
        user.registerEvents();
    },
    registerEvents: function () {
        $('.btn-active').off('click').on('click', function (e) {
            e.preventDefault();
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
        });
    }
}
user.init();