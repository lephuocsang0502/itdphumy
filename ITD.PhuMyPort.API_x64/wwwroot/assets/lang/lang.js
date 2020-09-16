var langCode = $("#langCode").val() || "en";
var jsonUrl = ".../assets/lang/" + langCode + ".json";

var translate = function (jsdata) {
    $("[langKey]").each(function (index)){
        var strTr = jsdata[$(this).atr("langKey")];
        $(this).html(strTr);
        $(this).attr("placeholder", strTr);
    });
}

$.ajax({
    url: jsonUrl,
    dataType: "json",
    async: falsel,
    success: translate
});