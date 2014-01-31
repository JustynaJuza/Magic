$(function () {
    var $chatSendButton = $('#chat-send'),
        $newChatMessage = $('#new-chat-message'),
        $chatLog = $('#chat-log'),
        $chatMessages = $('#chat-messages'),
        $chatMessage = $('.chat-message'),
        $userList = $('#user-list'),
        $chatUsers = $('#chat-users'),
        $chatUser = $('.chat-user'),
        $chatGeneralCheckbox = $('#chat-general-check'),
        $chatPrivateCheckbox = $('#chat-private-check');

    // ---------------------------- HUB ---------------------------- BEGIN
    // Reference the auto-generated proxy for the hub.
    var chat = $.connection.chatHub;

    // Start the connection.
    $.connection.hub.start().done(function () {
        $chatSendButton.click(function () {
            // Call the message sending method on server.
            chat.server.send($newChatMessage.val(), "");
            // Clear text box and reset focus for next comment.
            $newChatMessage.val('').focus();
        });
    });

    // Hub callback delivering new messages.
    chat.client.addMessage = function (time, sender, senderColor, message, recipient, recipientColor) {
        $chatMessages.append('<li>' + time + ' <span class="chat-message-sender" style="font-weight:bold;color:' + htmlEncode(senderColor) + '">' + htmlEncode(sender)
            + ' </span>' + (recipient != null ? ' <span class="chat-message-recipient" style="font-weight:bold;color:' + htmlEncode(recipientColor) + '">@' + htmlEncode(recipient)
            + '</span> ' : '') + htmlEncode(message) + '</li>');

        // Scroll to bottom message.
        $chatLog.animate({ scrollTop: $chatLog[0].scrollHeight }, 1000);
        //$chatLog.animate($chatLog.scrollTop(200), 1000); //removes scrollbars for some reason
    };

    chat.client.updateChatUsers = function (chatUsers) {
            chatUsers = $.parseJSON(chatUsers);
            var $updatedChatUsers = '<ul id="chat-users" style="list-style-type: none; margin:0px">';
            for (var i = 0; i < chatUsers.length; i++) {
                $updatedChatUsers = $updatedChatUsers.concat('<li class="chat-user" style="font-weight:bold;color:' + htmlEncode(chatUsers[i].ColorCode) + '">' + chatUsers[i].UserName + '</li>');
            }
            $updatedChatUsers = $updatedChatUsers.concat('</ul>');

        $chatUsers.replaceWith($updatedChatUsers);
    };

    // Hub callback disconnecting client. 
    chat.client.stopClient = function () {
        $.connection.hub.stop();
    };

    // Hub callback to make client join appointed group.
    chat.client.joinRoom = function (userId, roomName) {
        chat.server.joinRoom(userId, roomName)
    }

    // Hub callback to make client leave appointed group.
    chat.client.leaveRoom = function (userId, roomName) {
        chat.server.leaveRoom(userId, roomName)
    }

    $.connection.hub.connectionSlow(function () {
        alert("slow connection");
    });
    // ---------------------------- HUB ---------------------------- END

    // ---------------- CHAT DISPLAY & FUNCTIONALITY --------------- START
    // Send button enabled only on chat message input.
    $chatSendButton.prop("disabled", true);
    $newChatMessage.on('input', function () {
        if ($newChatMessage.val() == '') {
            $chatSendButton.prop('disabled', true);
        }
        else {
            $chatSendButton.prop('disabled', false);
        }
    })

    // Send messages on enter.
    $newChatMessage.keyup(function (e) {
        if (e.keyCode == 13 && $newChatMessage.val().length > 0) {
            $chatSendButton.toggleClass('clicked');
            $chatSendButton.trigger('click');
            $chatSendButton.prop("disabled", true);
        }
    });

    // Provide checkboxes for hiding general/private messages.
    // TODO: Check why this doesn't work sometimes... Maybe bacause of dynamically added elements? Maybe because of fading scroll?
    $chatGeneralCheckbox.change(function () {
        $chatMessage.each(function () {
            if ($(this).find($('.chat-message-recipient')).length == 0) {
                $(this).toggle();
            }
        });
    });
    $chatPrivateCheckbox.change(function () {
        $chatMessage.each(function () {
            if ($(this).find($('.chat-message-recipient')).length != 0) {
                $(this).toggle();
            }
        });
    });
    //$chatGeneralCheckbox.change(function () {
    //    var $chatMessageList = $chatLog.find('li');
    //    $chatMessageList.not('.chat-message-recipient').each(function () {
    //            $(this).toggle();
    //        })
    //});

    // Enable smooth scrolling chat messages and user list.
    $chatLog.scroll(smoothScroll($chatLog, $chatMessage));
    $userList.scroll(smoothScroll($userList, $chatUser));

    // Make chat sender/recipient names clickable for reply (works with dynamically added elements).
    $(document).on('click', '.chat-message-sender', function () {
        $newChatMessage.val('@' + $(this).text() + ' ');
        $newChatMessage.focus();
    });
    $(document).on('click', '.chat-message-recipient', function () {
        $newChatMessage.val($(this).text() + ' ');
        $newChatMessage.focus();
    });

    function smoothScroll ($container, $scrollingEntry) {
        var lineHeightInPixels = 20;
        var marginSize = 10;
        var linesVisible = ($container.height() / lineHeightInPixels).toFixed(0);
        var linesTotal = (($container[0].scrollHeight - marginSize) / lineHeightInPixels).toFixed(0);

        // Get number of oldest message lines to fade out based on line height and scroll position.
        var linesToFadeUpper = ($container.scrollTop() / lineHeightInPixels).toFixed(0);

        // Fade upper lines out.
        $scrollingEntry.slice(0, linesToFadeUpper).fadeTo(0, 0.01, null);
        // Fade visible lines in.
        $scrollingEntry.slice(linesToFadeUpper, linesToFadeUpper + linesVisible).fadeTo(0, 1, null);
        // Fade lower lines out.
        $scrollingEntry.slice(linesToFadeUpper + linesVisible, linesTotal).fadeTo(0, 0.01, null);
    }

    // Html-encode messages for display in the page.
    function htmlEncode(value) {
        var encodedValue = $('<div />').text(value).html();
        return encodedValue;
    }
    // ---------------- CHAT DISPLAY & FUNCTIONALITY --------------- START
});