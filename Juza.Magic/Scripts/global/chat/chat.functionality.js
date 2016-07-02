(function (chat, $, undefined) {

    var keyCode = {
        enter: 13,
        tab: 9,
        space: 32
    }

    // Send button enabled only on chat message input.
    chat.sendButton.prop('disabled', true);
    chat.newMessage.on('input', function () {
        if (chat.newMessage.val() === '') {
            chat.sendButton.prop('disabled', true);
        } else {
            chat.sendButton.prop('disabled', false);
        }
    });

    // Send messages on enter.
    chat.newMessage.keyup(function (e) {
        if (e.keyCode === keyCode.enter && chat.newMessage.val().length > 0) {
            chat.sendButton.toggleClass('clicked');
            chat.sendButton.trigger('click');
            chat.sendButton.prop('disabled', true);
        }
        else if ((e.keyCode === keyCode.space || e.keyCode === keyCode.tab)
            && chat.newMessage.val().split(' ').length === 2) {

            if (_.any(chat.commands, matchNewChatMessage)) {
                this['command_' + chat.newMessage.val().split(' ')[0].toLowerCase()].apply();
            };
        }
    });

    chat.selectExiststingRoom = function (recipients) {
        var exisitingRoom = _.find(chat.roomTabs.not('#room-tab-default'), function (element) {

            var recipientsInRoom = $(element).data('recipients');
            return _.isEqual(recipients.sort(), recipientsInRoom.sort());
        });

        if (exisitingRoom) {
            $('#' + exisitingRoom.id).trigger('click');
            return true;
        }
        return false;
    };

    chat.addRoomTab = function (recipients, roomId, isAsyncRequest, activateTabAfterwards) {
        chat.chatRoomRequestInProgress[roomId] = true;
        var isExistingRoom = roomId != null;
        activateTabAfterwards = roomId == null || activateTabAfterwards;
        var url = window.basePath + 'Chat/GetChatRoomPartial/';

        // Extension used to append new room markup to chat.
        function appendRoomToChat(htmlContent) {
            chat.roomsContainer.append(htmlContent);
            chat.adjustRoomTabs();

            $('#room-tab-' + roomId).data('recipients', recipients);
            $('#room-tab-' + roomId).data('isNew', !isExistingRoom);
            if (activateTabAfterwards) {
                $('#room-tab-' + roomId).trigger('click');
            }
            else {
                $('#room-content-' + roomId).hide();
            }

            chat.display.scrollContainerToBottom('#room-messages-container-' + roomId);
            chat.chatRoomRequestInProgress[roomId] = false;
        }

        jQuery.ajaxSetup({
            async: isAsyncRequest,
            traditional: true
        });

        if (roomId) {
            // Room id already known, get markup only.
            $.get(url, { roomId: roomId }, appendRoomToChat);
        }
        else {
            // Request information for room with selected memebers.
            chat.hub.server.getExistingChatRoomIdForUsers(recipients)
                .done(function (existingRoomId) {
                    isExistingRoom = existingRoomId.length !== 0;
                    roomId = existingRoomId;

                    // Request chat room html markup for existing or new room.
                    if (isExistingRoom) {
                        // Skip request if markup already in page.
                        if ($('#room-' + roomId).length) {
                            return $('#room-tab-' + roomId).trigger('click');
                        }

                        $.get(url + $.now(), { roomId: roomId }, appendRoomToChat);
                    }
                    else {
                        $.get(url, { recipientNames: recipients }, function (htmlContent) {
                            roomId = $($.parseHTML(htmlContent)).find('.chat-room-tab').prop('id').substr(9);
                            appendRoomToChat(htmlContent);
                        });
                    }
                });
        }
    };

    chat.removeRoomTab = function (roomId) {
        chat.roomsContainer.find('#room-' + roomId).remove();
        chat.adjustRoomTabs();
    };

    chat.adjustRoomTabs = function() {
        chat.roomTabs = $(chat.roomTabs.selector);
        chat.roomContents = $(chat.roomContents.selector);

        var tabCount = chat.roomTabs.length;
        var avgWidth = 100 / tabCount;
        chat.roomTabs.css({
            'width': avgWidth + '%',
            'margin-left': function(index) {
                return avgWidth * index + '%';
            }
        });

        // Tabs closed - only default chat room left, hide tab bar.
        if (tabCount <= 1) {
            this.roomSelection.data('chatRoomId', 'default');
            this.roomSelection.data('recipients', '');

            setTimeout(chat.display.toggleTabBar(), 150);
        } else {
            chat.display.toggleTabBar(true);
        }
    };
    
    chat.matchNewChatMessage = function (value) {
        return chat.newMessage.val().split(' ')[0].toLowerCase() === value;
    };

}(window.chat = window.chat || {}, jQuery));