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

    // Hub message sending callback function.
    chat.client.addMessage = function (time, sender, senderColor, message, recipient, recipientColor) {
        $('#discussion').append('<li>' + time + ' <span class="chat-message-sender" style="font-weight:bold;color:' + htmlEncode(senderColor) + '">' + htmlEncode(sender)
            + ' </span>' + (recipient != null ? ' <span class="chat-message-recipient" style="font-weight:bold;color:' + htmlEncode(recipientColor) + '">@' + htmlEncode(recipient)
            + '</span> ' : '') + htmlEncode(message) + '</li>');

        // Scroll to bottom message.
        $chatLog.animate({ scrollTop: $chatLog[0].scrollHeight }, 1000);
        //$chatLog.animate($chatLog.scrollTop(200), 1000); //removes scrollbars for some reason
    };

    // Hub disconnecting client function. 
    chat.client.stopClient = function () {
        $.connection.hub.stop();
    };

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
        if (e.keyCode == 13) {
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