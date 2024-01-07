var SessionTimeout = (function () {
    var i = function () {
        $.sessionTimeout({
            title: "Session Timeout Notification",
            message: "Your session is expiring soon.",
            redirUrl: "/lockscreen",
            logoutUrl: "/Home/Logout",
            //warnAfter: 10e3,
            warnAfter: 6e5,
            redirAfter: 6.6e5,
            ignoreUserActivity: !1,
            countdownMessage: "Redirecting in {timer} seconds.",
            countdownBar: !0,
        });
    };
    return {
        init: function () {
            i();
        },
    };
})();
jQuery(function () {
    SessionTimeout.init();
});
