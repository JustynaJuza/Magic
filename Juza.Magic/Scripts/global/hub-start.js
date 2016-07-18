// Start the hub connection if it's not running already.
if ($.connection.hub && $.connection.hub.state === 4) {
    var hasChat = typeof chat != 'undefined';
    var isGame = typeof game != 'undefined';

    if (hasChat) {
        chat.hub = $.connection.chatHub;
        chat.registerClientCallbacks(chat.hub.client);
    }

    $.connection.hub.start().done(function () {
        if (hasChat) {
            // If hub handling provided by previously loaded hub script, call initialization functions.
            chat.initialize();
        }
        if (isGame) {
            game.hub = $.connection.gameHub;
            game.initialized = true;
            game.initialize();
        }
    });
}
