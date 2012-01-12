$(".lang-picker a").click(
    function () {
        $.cookie("_culture", $(this).attr("class"), { expires: 365, path: '/' });
        window.location.reload();
    }
);