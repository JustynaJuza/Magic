$(function () {

    window.game = $.connection.gameHub;

    var isReady = false,
        isPlayer = ($('#player').length ? true : false),
        userName = ($('#player').length ? $('#player').val() : $('#observer').val()),
        gameId = $('#gameId').val(),
        $gameTimer = $('#game-timer'),
        $pauseBtn = $('#game-pause-btn'),
        $playerReadyButton = $('#player-ready-btn'),
        $playerName = $('.player'),
        $observerName = $('.observer'),
        $gameUsers = $('.player, .observer'),
        $gameField = $('#game-field'),
        $gameFieldOverlay = $('#game-field-overlay'),
        $gameFieldOverlayMsg = $('#game-field-overlay-message');
    
    $(document).on('click', '#game-pause-btn', function () {
        window.game.server.pauseGame(gameId, false);
    });

    // ---------------------------- HUB ---------------------------- BEGIN

    // Initialize game handling.
    window.game.initialize = function initializeGame() {
        window.chat.server.subscribeGameChat(gameId);
        window.game.server.joinGame(gameId, userName, isPlayer);

        $playerReadyButton.click(function () {
            isReady = !isReady;
            $playerReadyButton.val((isReady ? "Not ready!" : "Ready to start"));
            game.server.togglePlayerReady(gameId, isReady);
        });
    }
    window.game.client.togglePlayerReady = function (playerName, playerColor) {
        $playerName = $($playerName.selector);
        var $existingPlayer = $playerName.filter(function (i, element) {
            return $(element).text() == playerName;
        });

        // Display the player is ready.
        if (playerColor) {
            $existingPlayer.css({ 'color': playerColor });
            $existingPlayer.addClass('player-ready');
        }
            // Display the player is not ready.    
        else {
            $existingPlayer.css({ 'color': '#808080' });
            $existingPlayer.removeClass('player-ready');
        }
    };

    window.game.client.activateGame = function (gameTime) {
        if (gameTime) {
            $gameTimer.text(gameTime);
        }
        window.gameTimer = setInterval(updateGameTimer, 1000);
        $gameFieldOverlayMsg.text('Let\'s play!');
        $pauseBtn.removeClass('disabled');
        $gameFieldOverlay.slideUp();
    }

    window.game.client.pauseGame = function (message) {
        $pauseBtn.addClass('disabled');
        clearInterval(window.gameTimer);
        $gameFieldOverlayMsg.text(message);
        $gameFieldOverlay.slideDown();
    }

    // ---------------------- GAME DISPLAY --------------------- START
    // Reset players and ready buttons when player joins or leaves before game start.
    window.game.client.resetReadyStatus = function () {
        $playerName = $($playerName.selector);
        $playerName.css('color', '#808080');
        $playerName.removeClass('player-ready');

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
    // ---------------------- GAME DISPLAY --------------------- END
    // ---------------------------- HUB ---------------------------- END


    // --------------------- HELPER FUNCTIONS ---------------------- START
    function updateGameTimer() {
        var time = $gameTimer.text().split(':').map(parseFloat);
        time[2] = time[2] + 1;
        if (time[2] == 60) {
            time[2] = 0;
            time[1] = time[1] + 1;
            if (time[1] == 60) {
                time[1] = 0;
                time[0] = time[0] + 1;
            }
        }
        $gameTimer.text(formatTime(time));
    }

    function formatTime(timeArray) {
        return (timeArray[1] < 10 ? '0' : '') + timeArray[0] + ':'
            + (timeArray[1] < 10 ? '0' : '') + timeArray[1] + ':'
            + (timeArray[2] < 10 ? '0' : '') + timeArray[2];
    }
    // --------------------- HELPER FUNCTIONS ---------------------- END

});