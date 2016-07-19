(function (chat, $, undefined) {

    chat.registerClientCallbacks = function (chatClient) {

        // Hub callback delivering new messages.
        chatClient.addMessage = function (roomId, time, sender, senderColor, message, activateTabAfterwards) {
            if (!$('#room-' + roomId).length && !chatRoomRequestInProgress[roomId]) {
                chat.addRoomTab(sender, roomId, false, activateTabAfterwards);
            }

            $('#room-messages-' + roomId).append(
                String.format(
                '<li class="chat-message clearfix">' +
                    '<span>{0}</span>' +
                    '<span class="chat-message-sender" style="color:{1}">' +
                        '{2}' +
                    '</span>' +
                    '<span>{3}</span>' +
                '</li>',
                time,
                senderColor,
                helpers.htmlEncode(sender),
                helpers.htmlEncode(message)));

            if (chat.roomSelection.data('chatRoomId') !== roomId) {
                var tabBlinkerProcess = setInterval(helpers.blink, 1000, '#room-tab-' +roomId);
                chat.display.tabBlinkingTracker[roomId] = tabBlinkerProcess;
            } else {
                chat.display.scrollContainerToBottom('#room-messages-container-' + roomId);
            }
        };

        chatClient.closeChatRoom = function (roomId) {
            if (chat.roomSelection.data('chatRoomId') === roomId) {
                chat.roomTabs.first().trigger('click');
            }
            $('#room-' + roomId).remove();
            chat.adjustRoomTabs();
        }

        // Hub callback for updating chat user list on each change.
        chatClient.updateChatRoomUsers = function (chatUsers, roomId) {
            chatUsers = $.parseJSON(chatUsers);
            var updatedChatUsersHtml = '';
            for (var i = 0; i < chatUsers.length; i++) {
                updatedChatUsersHtml += '<li class="chat-user display-name" style="color:' + chatUsers[i].ColorCode
                    + (chatUsers[i].Status === 0 ? '; font-weight:normal;">' : ';">')
                    + chatUsers[i].UserName + '</li>';
            }

            $('#room-users-' + roomId).children().remove();
            $('#room-users-' + roomId).append(updatedChatUsersHtml);
        };

        //// Hub callback disconnecting client. 
        //chat.client.stopClient = function () {
        //    $.connection.hub.stop();
        //};

        //// Hub callback to make client join appointed group.
        //chat.client.joinRoom = function (userId, roomId) {
        //    chat.server.joinRoom(userId, roomId)
        //}

        //// Hub callback to make client leave appointed group.
        //chat.client.leaveRoom = function (userId, roomId) {
        //    chat.server.leaveRoom(userId, roomId)
        //}

        //$.connection.hub.connectionSlow(function () {
        //    alert('slow connection');
        //});        
    }


    // Provide checkboxes for hiding general/private messages.
    //$chatGeneralCheckbox.change(function () {
    //    $chatMessage = $($chatMessage.selector);
    //    $chatMessage.each(function () {
    //        if ($(this).find('.chat-message-recipient').length == 0) {
    //            $(this).toggle();
    //        }
    //    });
    //});
    //$chatPrivateCheckbox.change(function () {
    //    $chatMessage = $($chatMessage.selector);
    //    $chatMessage.each(function () {
    //        if ($(this).find($('.chat-message-recipient')).length > 0) {
    //            $(this).toggle();
    //        }
    //    });
    //});

    // Enable smooth scrolling chat messages and chat users.
    //$chat.messagesContainers.each(function() {
    //    $(this).scroll(smoothScroll(this, '.chat-message'));
    //});
    //$chat.usersContainers.each(function() {
    //    $(this).scroll(smoothScroll($(this), $('.chat-user')));
    //});

    //function adjustNewMessageElementPadding() {
    //    var newPadding = basicNewMessagePadding + parseInt($chat.roomSelection.css('width'));
    //    $newChatMessage.css('padding-left', newPadding);
    //}

}(window.chat = window.chat || {}, jQuery));