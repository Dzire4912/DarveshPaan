const SessionTimeout = {

    init: function (customOptions) {

        const config = window.sessionTimeoutConfig || {};

        const defaultOptions = {
            title: "Session Timeout Notification",
            message: "Your session is expiring soon.",
            redirUrl: "/lockscreen",
            logoutUrl: "/Home/Logout",
            warnAfter: config.warnAfter,
            redirAfter: config.redirAfter,
            ignoreUserActivity: config.ignoreUserActivity,
            countdownMessage: "Redirecting in {timer} seconds.",
            countdownBar: true,
            keepAliveUrl: window.location.href,
        };

        const mergedOptions = $.extend(defaultOptions, customOptions);
        $.sessionTimeout(mergedOptions);
    },

};

jQuery(function () {
    // Customize session timeout options
    SessionTimeout.init({});
});

