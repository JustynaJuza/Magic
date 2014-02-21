$(function () {
    // Start the hub connection if it's not running already.
    if ($.connection.hub && $.connection.hub.state == 4) {
        $.connection.hub.start().done(function () {
            // If hub handling provided, call initialization functions.
            if (typeof window.chat != 'undefined') {
                window.chat.initialize();
            }
            if (typeof window.game != 'undefined') {
                alert('start')
                window.game.initialize();
            }
        });
    }
});