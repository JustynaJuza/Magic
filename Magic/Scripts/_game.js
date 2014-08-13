$(function () {
    var isReady = false,
        isPlayer = ($('#player').length ? true : false),
        userName = ($('#player').length ? $('#player').val() : $('#observer').val()),
        $gameId = $('#gameId'),
        $playerReadyButton = $('#player-ready'),
        $playerName = $('.player'),
        $observerName = $('.observer'),
        $gameUsers = $('.game-user');

    // ---------------------------- HUB ---------------------------- BEGIN
    // Reference the auto-generated proxy for the hub.
    window.game = $.connection.gameHub;

    // Initialize game handling.
    window.game.initialize = function initializeGame() {
        window.chat.server.subscribeGameChat($gameId.val());
        window.game.server.joinGame($gameId.val(), userName, isPlayer);

        $playerReadyButton.click(function () {
            isReady = !isReady;
            $playerReadyButton.val((isReady ? "Not ready!" : "Ready to start"));
            game.server.togglePlayerReady($gameId.val(), isReady);
        });
    }
    // ---------------------------- HUB ---------------------------- END


    // ------------------------ GAME DISPLAY ----------------------- START
    // Toggle player ready handler on server.
    window.game.client.togglePlayerReady = function (playerName, playerColor, resetPlayerReadyButton) {
        var $existingPlayer = $playerName.filter(function (i, element) {
            return $(element).text() == playerName;
        });

        // Display the player is ready.
        if (playerColor) {
            $existingPlayer.css('color', playerColor);
        }
        // Display the player is not ready.    
        //else {
        //    $existingPlayer.css('color', '#808080');
        //    // Set player to 'not ready' if other player left before game start.
        //    if (resetPlayerReadyButton) {
        //        isReady = false;
        //        $playerReadyButton.val("Ready to start");
        //    }
        //}
    };

    window.game.client.resetReadyStatus = function () {
        $playerName.css('color', '#808080');

        if (isPlayer) {
            isReady = false;
            $playerReadyButton.val("Ready to start");
        }
    }

    // Add player who has joined to player list.
    window.game.client.playerJoined = function (playerName) {
        $playerName = $($playerName.selector);
        var $existingPlayer = $playerName.filter(function (i, element) {
            return $(element).text() == playerName;
        });

        if (!$existingPlayer.length) {
            $('#players').append('<li class="player" style="color:#808080">' + playerName + '</li>');
        }
    };

    // Add observer who has joined to observer list.
    window.game.client.observerJoined = function (observerName) {
        $observerName = $($observerName.selector);
        var $existingObserver = $observerName.filter(function (i, element) {
            return $(element).text() == observerName;
        });

        if (!$existingObserver.length) {
            $('#observers').append('<li class="observer" style="color:#808080">' + observerName + '</li>');
        }
    };

    // Remove absent user from list.
    window.game.client.userLeft = function (name) {
        $gameUsers = $($gameUsers.selector);
        var $existingUser = $gameUsers.filter(function (i, element) {
            return $(element).text() == name;
        });
        $existingUser.remove();
    };


    window.game.client.activateGame = function () {
        alert("LET THE GAMES BEGIN!");
    }
    // ------------------------ GAME DISPLAY ----------------------- START
});