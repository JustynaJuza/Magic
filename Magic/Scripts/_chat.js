﻿$(function () {
    window.chatRoomUsers = [];
    var $chatSendButton = $('#new-chat-message-send'),
        $newChatMessage = $('#new-chat-message'),
        $chatMessagesContainer = $('#chat-messages-container'),
        $chatMessages = $('#chat-messages'),
        $chatMessage = $('.chat-message'),
        $chatRoomSelection = $('.chat-room-selection'),
        $chatRoomSelectList = $('#chat-room-selectlist'),
        $chatRoomUsersSelectList = $('#chat-room-users-selectlist')
        $chatUsersContainer = $('#chat-users-container'),
        $chatUsers = $('#chat-users'),
        $chatUser = $('.chat-user'),
        $chatGeneralCheckbox = $('#chat-messages-general-check'),
        $chatPrivateCheckbox = $('#chat-messages-private-check'),
        basicNewMessagePadding = parseInt($newChatMessage.css('padding-left'));

    adjustNewMessageElementPadding();
    adjustRoomTabs();

    function ChatRoomUsers(chatUsersHtml, roomName) {
        /// <summary>Creates an object storing the chat room user list related identified by room name, can also be used as clone constructor.</summary>
        /// <param name="chatUsersHtml" type="String" optional="false">The chat room related user list in HTML markup.</param>
        /// <param name="roomName" type="String" optional="false">The chat room name.</param>
        /// <returns type="ChatRoomUsers">An object storing the chat room user list related identified by room name.</returns>
        /// <field name="chatUsersHtml" type="String">The chat room related user list in HTML markup.</field>
        /// <field name="roomName" type="String">The chat room name.</field>
        if (arguments.length > 1){
            this.chatUsersHtml = chatUsersHtml;
            this.roomName = roomName;
        }
        else {
            this.chatUsersHtml = arguments[0].chatUsersHtml;
            this.roomName = arguments[0].roomName;
        }
    }

    // ---------------------------- HUB ---------------------------- BEGIN
    // Reference the auto-generated proxy for the hub.
    window.chat = $.connection.chatHub;
    var chat = window.chat;

    // Initialize chat handling.
    window.chat.initialize = function initializeChat() {
        $chatSendButton.click(function () {
            // Call the message sending method on server.
            chat.server.send($newChatMessage.val(), "");
            // Clear text box and reset focus for next comment.
            $newChatMessage.val('').focus();
        });
    }

    // Hub callback delivering new messages.
    chat.client.addMessage = function (time, sender, senderColor, message, recipient, recipientColor) {
        $chatMessages.append('<li class="chat-message">' + time + ' <span class="chat-message-sender" style="font-weight:bold;color:' + htmlEncode(senderColor) + '">' + htmlEncode(sender)
            + ' </span>' + (recipient != null ? ' <span class="chat-message-recipient" style="font-weight:bold;color:' + htmlEncode(recipientColor) + '">@' + htmlEncode(recipient)
            + '</span> ' : '') + htmlEncode(message) + '</li>');

        // Scroll to bottom message.
        $chatMessagesContainer.animate({ scrollTop: $chatMessagesContainer[0].scrollHeight }, 1000);
        //$chatMessagesContainer.animate($chatMessagesContainer.scrollTop(200), 1000); //removes scrollbars for some reason
    };

    // Hub callback for updating chat user list on each change.
    chat.client.updateChatRoomUsers = function (chatUsers, roomName) {
        chatUsers = $.parseJSON(chatUsers);
        var updatedChatUsersHtml = '<ul id="chat-users" style="list-style-type: none; margin:0px">';
        for (var i = 0; i < chatUsers.length; i++) {
            updatedChatUsersHtml += '<li class="chat-user" style="font-weight:bold;color:' + htmlEncode(chatUsers[i].ColorCode) + '">' + chatUsers[i].UserName + '</li>';
        }
        updatedChatUsersHtml += '</ul>';

        var foundChatRoomUsers = _.find(window.chatRoomUsers, function (element, index) {
            return element.roomName == roomName;
        });
        
        if (foundChatRoomUsers != null) {
            foundChatRoomUsers.chatUsersHtml = updatedChatUsersHtml;
        }
        else {
            window.chatRoomUsers.push(new ChatRoomUsers(updatedChatUsersHtml, roomName));
        }

        filterChatUsers(roomName);
    };

    // Hub callback to remove chat room tab when current user leaves.
    chat.client.leaveChatRoom = function(roomName) {
        $('#chat-room-users-selectlist li:contains(' + roomName + ')').remove();
        adjustRoomTabs();
    }

    // Hub callback to add  chat room tab when current user joins.
    chat.client.joinChatRoom = function(roomName, tabColor, tabBorderColor) {
        $chatRoomUsersSelectList.append('<li style="background-color: ' + tabColor + '; border-color: ' + tabBorderColor + '">' + roomName + '</li>');
        adjustRoomTabs();
    }

    //// Hub callback disconnecting client. 
    //chat.client.stopClient = function () {
    //    $.connection.hub.stop();
    //};

    //// Hub callback to make client join appointed group.
    //chat.client.joinRoom = function (userId, roomName) {
    //    chat.server.joinRoom(userId, roomName)
    //}

    //// Hub callback to make client leave appointed group.
    //chat.client.leaveRoom = function (userId, roomName) {
    //    chat.server.leaveRoom(userId, roomName)
    //}

    //$.connection.hub.connectionSlow(function () {
    //    alert('slow connection');
    //});
    // ---------------------------- HUB ---------------------------- END

    // ---------------- CHAT DISPLAY & FUNCTIONALITY --------------- START
    // Send button enabled only on chat message input.
    $chatSendButton.prop('disabled', true);
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
            $chatSendButton.prop('disabled', true);
        }
    });

    // Provide checkboxes for hiding general/private messages.
    // TODO: Check why this doesn't work sometimes... Maybe bacause of dynamically added elements? Maybe because of fading scroll?
    $chatGeneralCheckbox.change(function () {
        $chatMessage = $($chatMessage.selector);
        $chatMessage.each(function () {
            if ($(this).find('.chat-message-recipient').length == 0) {
                $(this).toggle();
            }
        });
    });
    $chatPrivateCheckbox.change(function () {
        $chatMessage = $($chatMessage.selector);
        $chatMessage.each(function () {
            if ($(this).find($('.chat-message-recipient')).length > 0) {
                $(this).toggle();
            }
        });
    });

    // Enable smooth scrolling chat messages and user list.
    $chatMessagesContainer.scroll(smoothScroll($chatMessagesContainer, $chatMessage));
    $chatUsersContainer.scroll(smoothScroll($chatUsersContainer, $chatUser));

    // Make chat sender/recipient names clickable for reply (works with dynamically added elements).
    $(document).on('click', '.chat-message-sender, .chat-user', function () {
        $newChatMessage.val('@' + $(this).text() + ' ');
        $newChatMessage.focus();
    });
    $(document).on('click', '.chat-message-recipient', function () {
        $newChatMessage.val($(this).text() + ' ');
        $newChatMessage.focus();
    });

    function smoothScroll($container, $scrollingEntry) {
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

    function adjustNewMessageElementPadding() {
        var newPadding = basicNewMessagePadding + parseInt($chatRoomSelection.css('width'))
        $newChatMessage.css('padding-left', newPadding);
    }

    function adjustRoomTabs() {
        $('#chat-room-users-selectlist li').css('width', Math.floor(parseInt($chatRoomUsersSelectList.css('width')) / $chatRoomUsersSelectList.children().length))
    }

    $chatRoomSelection.click(function () {
        $chatRoomSelectList.toggle();
    });

    $(document).on('click', '#chat-room-selectlist li, .chat-message-sender, .chat-user', function () {
        $chatRoomSelectList.hide();
        $chatRoomSelection.val($(this).text());
        $chatRoomSelection.css('color', $(this).css('color'));
        adjustNewMessageElementPadding();
    });

    function filterChatUsers(roomName) {
        var chatRoomUsers = _.find(window.chatRoomUsers, function (element) {
            return element.roomName == roomName;
        });

        console.log(chatRoomUsers)
        $chatUsers.replaceWith(chatRoomUsers.chatUsersHtml);
        // Refresh the variable content.
        $chatUsers = $($chatUsers.selector);
    }
    
    //$('#add-tab').click(alert(window.chatRoomUsers)); //chat.client.joinChatRoom);

    $('#remove-tab').click(chat.client.leaveChatRoom);

    // ---------------- CHAT DISPLAY & FUNCTIONALITY --------------- END

    // --------------------- HELPER FUNCTIONS ---------------------- START


    
    // --------------------- HELPER FUNCTIONS ---------------------- START
});