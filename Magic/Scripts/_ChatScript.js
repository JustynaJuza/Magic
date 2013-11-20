$(function () {
    var $chatSendButton = $('#chat-send');
    var $chatMessage = $('#chat-message');
    var $chatLog = $('.chat-log');
    var $chatGeneralCheckbox = $('#chat-general-check');
    var $chatPrivateCheckbox = $('#chat-private-check');

    // Send button enabled only on chat message input.
    $chatSendButton.prop("disabled", true);
    $chatMessage.on('input', function () {
        if ($chatMessage.val() == '') {
            $chatSendButton.prop('disabled', true);
        }
        else {
            $chatSendButton.prop('disabled', false);
        }
    })

    // Reference the auto-generated proxy for the hub.
    var chat = $.connection.chatHub;

    // Hub callback delivering new messages.
    chat.client.addMessage = function (time, sender, senderColor, message, recipient, recipientColor) {
        $('#discussion').append('<li>' + time + ' <span class="chat-message-sender" style="font-weight:bold;color:' + htmlEncode(senderColor) + '">' + htmlEncode(sender)
            + ' </span>' + (recipient != null ? ' <span class="chat-message-recipient" style="font-weight:bold;color:' + htmlEncode(recipientColor) + '">@' + htmlEncode(recipient)
            + '</span> ' : '') + htmlEncode(message) + '</li>');

        // Scroll to bottom message.
        $chatLog.animate({ scrollTop: $chatLog[0].scrollHeight }, 1000);
        //$chatLog.animate($chatLog.scrollTop(200), 1000); //removes scrollbars for some reason
    };

    // Hub callback disconnecting client. 
    chat.client.stopClient = function () {
        $.connection.hub.stop();
    };

    $chatLog.scroll(function () {

        var lineHeightInPixels = 20;
        var marginSize = 10;
        var linesVisible = ($chatLog.height() / lineHeightInPixels).toFixed(0);
        var linesTotal = (($chatLog[0].scrollHeight - marginSize) / lineHeightInPixels).toFixed(0);

        // Get number of lines to fade out based on line height and scrollable area.
        var linesToFadeUpper = ($chatLog.scrollTop() / lineHeightInPixels).toFixed(0);//$chatLog[0].scrollHeight - marginSize - $chatLog.height(); // when scrolling down
        var linesToFadeLower = (($chatLog[0].scrollHeight - marginSize - $chatLog.scrollTop()) / lineHeightInPixels).toFixed(0) - linesVisible; // when scrolling up
        //alert(linesToFadeUpper + " " + linesVisible + " " + linesTotal);

        var $chatMessageList = $chatLog.find('li');
        // Fade upper lines out.
        $chatMessageList.slice(0, linesToFadeUpper).fadeTo(100, 0.01, null);
        // Fade visible lines in.
        $chatMessageList.slice(linesToFadeUpper == 0? linesToFadeUpper: linesToFadeUpper+1, linesVisible).fadeTo(100, 1, null);
        // Fade lower lines out.
        $chatMessageList.slice(linesToFadeUpper == 0? linesVisible : linesToFadeUpper + linesVisible, linesTotal).fadeTo(100, 0.01, null);
    });

    // Hub callback to make client join appointed group.
    chat.client.joinRoom = function (userId, roomName) {
        // Client invoke to join room.
        chat.server.joinRoom(userId, roomName)
    }

    // Hub callback to make client leave appointed group.
    chat.client.leaveRoom = function (userId, roomName) {
        // Client invoke to leave room.
        chat.server.leaveRoom(userId, roomName)
    }

    $.connection.hub.connectionSlow(function () {
        alert("slow connection"); // Your function to notify user.
    });

    // Start the connection.
    $.connection.hub.start().done(function () {
        $chatSendButton.click(function () {
            // Call the Send method on the hub.
            chat.server.send($chatMessage.val(), "");
            // Clear text box and reset focus for next comment.
            $chatMessage.val('').focus();
        });
    });

    // Send messages on enter.
    $chatMessage.keyup(function (e) {
        if (e.keyCode == 13 && $chatMessage.val().length > 0) {
            $chatSendButton.toggleClass('clicked');
            $chatSendButton.trigger('click');
            $chatSendButton.prop("disabled", true);
        }
    });

    // Provide checkboxes for hiding general/private messages.
    $chatGeneralCheckbox.change(function () {
        $('#discussion li').each(function () {
            if ($(this).find($('.chat-message-recipient')).length == 0) {
                $(this).toggle();
            }
        });
    });
    $chatPrivateCheckbox.change(function () {
        $('#discussion li').each(function () {
            if ($(this).find($('.chat-message-recipient')).length != 0) {
                $(this).toggle();
            }
        });
    });

    // Make chat sender/recipient names clickable for reply (works with dynamically added elements).
    $(document).on('click', '.chat-message-sender', function () {
        $chatMessage.val('@' + $(this).text() + ' ');
        $chatMessage.focus();
    });
    $(document).on('click', '.chat-message-recipient', function () {
        $chatMessage.val($(this).text() + ' ');
        $chatMessage.focus();
    });

    // Html-encode messages for display in the page.
    function htmlEncode(value) {
        var encodedValue = $('<div />').text(value).html();
        return encodedValue;
    }
});