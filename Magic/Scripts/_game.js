$(function () {
    var isReady = false,
        $gameId = $('#gameId'),
        $playerReadyButton = $('#player-ready'),
        $playerName = $('.player-name');

    // ---------------------------- HUB ---------------------------- BEGIN
    // Reference the auto-generated proxy for the hub.
    var chat = window.chat = $.connection.chatHub;
    var game = window.game = $.connection.gameHub;

    // Initialize game handling.
    window.game.initialize = function initializeGame() {
        chat.server.toggleGameChatSubscription($gameId.val(), true);

        $playerReadyButton.click(function () {
            isReady = !isReady;
            if (isReady) {
                game.server.togglePlayerReady($gameId.val());
                $playerReadyButton.val("Not ready!");
            }
            else {
                game.server.togglePlayerReady("");
                $playerReadyButton.val("Ready to start");
            }
        });
    }
    // ---------------------------- HUB ---------------------------- END
    

    // ------------------------ GAME DISPLAY ----------------------- START
    // Toggle player ready handler on server.
    game.client.togglePlayerReady = function (playerName, playerColor, resetPlayerReadyButton) {
        var $existingPlayer = $playerName.filter(function (element) {
            return $(this).text() == playerName;
        });
        // Display the player is ready.
        if (playerColor) {
            $existingPlayer.css('color', playerColor);
        }
            // Display the player is not ready.
        else {
            $existingPlayer.css('color', '#808080');
            // Set player to 'not ready' if other player left before game start.
            if (resetPlayerReadyButton) {
                isReady = false;
                $playerReadyButton.val("Ready to start");
            }
        }
    };

    // Add player who has joined to player list.
    game.client.playerJoined = function (playerName) {
        var $existingPlayer = $playerName.filter(function (element) {
            return $(this).text() == playerName;
        });
        if ($existingPlayer.length == 0) {
            $('#players-list').append('<h2 class="player-name" style="color:#808080">' + playerName + '</h2>');
        }
        // Refresh the variable content.
        $playerName = $($playerName.selector);
    };

    // Add observer who has joined to observer list.
    game.client.observerJoined = function (observerName) {
        var $existingObserver = $observerName.filter(function (element) {
            return $(this).text() == observerName;
        });
        if ($existingObserver.length == 0) {
            $('#observers-list').append('<h2 class="observer-name" style="color:#808080">' + observerName + '</h2>');
        }
        // Refresh the variable content.
        $observerName = $($observerName.selector);
    };

    // Remove absent user from list.
    game.client.userLeft = function (playerName) {
        var $existingPlayer = $playerName.filter(function (element) {
            return $(this).text() == playerName;
        });
        $existingPlayer.remove();
        $playerName = $($playerName.selector);

        $playerName.each(function () {
            $(this).css('color', '#808080');
        });
    };
    // ------------------------ GAME DISPLAY ----------------------- START
});