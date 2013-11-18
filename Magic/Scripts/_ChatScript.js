$(function () {
    var $chatSendButton = $("#chat-send");
    var $chatMessage = $("#chat-message");

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
        $('#discussion').append('<li>' + time + ' <strong style="color:' + htmlEncode(senderColor) + '">' + htmlEncode(sender)
            + '</strong> ' + (recipient != null ? ' <strong style="color:' + htmlEncode(recipientColor) + '">@' + htmlEncode(recipient)
            + '</strong> ' : '') + htmlEncode(message) + '</li>');
    };
    //myDiv.animate({ scrollTop: myDiv.attr("scrollHeight") - myDiv.height() }, 3000);

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

    // This optional function html-encodes messages for display in the page.
    function htmlEncode(value) {
        var encodedValue = $('<div />').text(value).html();
        return encodedValue;
    }
});