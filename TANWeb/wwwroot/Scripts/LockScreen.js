function checkSessionTimeout() {
    $.ajax({
        url:urls.Dashboard.CheckSessionTimeout,
        method: 'GET',
        success: function (result) {
            if (result)
            {
                window.location.href = '/lockscreen';
            }
            else
            {
                window.location.href = '/Home/Logout';
            }
        },
        error: function () {
            // Handle the error (e.g., retry or show an error message)
        }
    });
}