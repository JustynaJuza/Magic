$(function () {
    var $chatSendButton = $('#chat-send');
    var $chatMessage = $('#chat-message');
    var $chatLog = $('.chat-log');

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

    // Hub callback function.
    chat.client.addMessage = function (time, sender, senderColor, message, recipient, recipientColor) {
        $('#discussion').append('<li>' + time + ' <span class="chat-message-sender" style="font-weight:bold;color:' + htmlEncode(senderColor) + '">' + htmlEncode(sender)
            + '</span> ' + (recipient != null ? ' <span class="chat-message-recipient" style="font-weight:bold;color:' + htmlEncode(recipientColor) + '">@' + htmlEncode(recipient)
            + '</span> ' : '') + htmlEncode(message) + '</li>');

        // Scroll to bottom message.
        $chatLog.animate({ scrollTop: $chatLog[0].scrollHeight }, 1000);
        //$chatLog.animate($chatLog.scrollTop(200), 1000); //removes scrollbars for some reason
    };

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
        if (e.keyCode == 13) {
            $chatSendButton.toggleClass('clicked');
            $chatSendButton.trigger('click');
            $chatSendButton.prop("disabled", true);
        }
    });

    // Make chat sender/recipient names clickable for reply.
    $(document).on('click', '.chat-message-sender', function () {
        $chatMessage.val('@' + $(this).text() + ' ');
        $chatMessage.focus();
    });

    $(document).on('click', '.chat-message-recipient', function () {
        $chatMessage.val($(this).text() + ' ');
        $chatMessage.focus();
    });

    // This optional function html-encodes messages for display in the page.
    function htmlEncode(value) {
        var encodedValue = $('<div />').text(value).html();
        return encodedValue;
    }
});