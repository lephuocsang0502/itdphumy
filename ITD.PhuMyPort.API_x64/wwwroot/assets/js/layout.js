$(document).ready(function () {
    $.ajax({
        url: "/Transection/Same",
        dataType: "json",
        type: "GET",
        success: function (response) {
            var html = '';
            var template = $('#data-result').html();

            html += Mustache.render(template, {
                Same: response.dataSame,
                NotSame: response.dataNotSame,
                NoImg: response.dataNoImg,
                Unapprove: response.dataUnapprove
            });
            $('#getResult').html(html);
        }
    });
});