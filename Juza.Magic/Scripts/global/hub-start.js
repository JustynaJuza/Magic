// Start the hub connection if it's not running already.
if ($.connection.hub && $.connection.hub.state === 4) {
    var hasChat = typeof chat != 'undefined';
    // If hub handling provided by previously loaded hub script, call initialization functions.
    if (hasChat) {
        chat.hub = $.connection.chatHub;
        chat.registerClientCallbacks(chat.hub.client);
    }

    $.connection.hub.start().done(function () {
        if (hasChat) {
            chat.initialize();
        }
        if (typeof game != 'undefined') {
            game.hub = $.connection.gameHub;
            game.initialized = true;
            game.initialize();
        }
    });
}
