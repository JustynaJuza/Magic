(function (chat, $, undefined) {

    chat.registerEventHandlers = function() {

        $(document).click(function(e) {
            if (chat.userTooltip.is(':hidden')) return;
            if (e.target.id !== 'user-profile-tooltip-container' && !$(e.target).hasClass('chat-user'))
                chat.display.toggleUserTooltip();
        });

        $(document).on('click', '.chat-user', function() {
            clearTimeout(window.clickTimer);
            window.clickTimer = setTimeout(function(element) {
                chat.display.toggleUserTooltip($(element).text());
                return $(element).data('click', false);
            }, 350, this);

            if ($(this).data('click')) {
                clearTimeout(window.clickTimer);
                var recipients = [chat.userName, $(this).text()].sort();
                if (!chat.selectExiststingRoom(recipients)) {
                    chat.hub.server.joinChatRoomWithUserNames(recipients);
                    //chat.addRoomTab(recipients);
                    chat.newMessage.focus();
                }
                return $(this).data('click', false);
            }
            return $(this).data('click', true);
        });

//$(document).on('mouseenter', '.chat-user', function () {
//    window.tooltipTimer = setTimeout(function (element) {
//        var user = $(element).text();
//        if (chat.userTooltip.data('user') != user) {
//            var url = window.basePath + 'Chat/GetUserProfileTooltipPartial/';
//            $.get(url, { chat.userName: user }, function(htmlContent) {
//                chat.userTooltip.data('user', user);
//                chat.userTooltip.html('');
//                chat.userTooltip.append(htmlContent);
//            });
//        }
//        chat.userTooltip.show();
//    }, 1000, this);
//});

//$(document).on('mouseleave', '.chat-user', function () {
//    clearTimeout(window.tooltipTimer);
//    chat.userTooltip.hide();
//});

        $(document).on('click', '.chat-message-recipient, .chat-message-sender', function() {
            chat.newMessage.val(chat.newMessage.text() + '@' + $(this).text() + ' ');
            chat.newMessage.focus();
        });

        $(document).on('click', '.chat-room-tab', function() {
            var roomId = $(this).prop('id').substr(9);
            chat.roomTabs.css({
                'border-width': '1px',
                'box-shadow': '',
                '-webkit-box-shadow': ''
            });

            $(this).css({
                'border-width': '2px',
                'box-shadow': 'inset 0 3px 5px rgba(0, 0, 0, 0.125)',
                '-webkit-box-shadow': 'inset 0 3px 5px rgba(0, 0, 0, 0.125)',
            });

            $('#room-content-' + roomId).show();
            chat.roomContents.not('#room-content-' + roomId).hide();
            chat.newMessage.focus();

            if (chat.display.tabBlinkingTracker[roomId]) {
                clearInterval(chat.display.tabBlinkingTracker[roomId]);
                chat.display.scrollContainerToBottom('#room-messages-container-' + roomId);
            }

            chat.roomSelection.data('chatRoomId', roomId);
            chat.roomSelection.data('recipients', $(this).data('recipients'));
            chat.roomSelection.data('isNew', $(this).data('isNew') || false);
        });

        $(document).on('click', '.chat-room-tab-close-btn', function(event) {
            var roomTab = $(this).closest('.chat-room-tab');
            var roomId = roomTab.prop('id').substr(9);
            if (roomTab.data('isNew') === true) {
                if (chat.roomSelection.data('chatRoomId') === roomId) {
                    chat.roomTabs.first().trigger('click');
                }
                $('#room-' + roomId).remove();
                chat.adjustRoomTabs();
            } else {
                window.chat.server.unsubscribeChatRoom(roomId);
            }
            event.stopPropagation();
        });

        $(document).on('click', '#chat-add-user-btn, .chat-room-tab-add-user', function() {
            var requestIsMade = null;
            if (!$('#available-users').length) {
                var url = window.basePath + 'Chat/GetAvailableUsersPartial/';
                requestIsMade = $.get(url, function(htmlContent) {
                    chat.header.append(htmlContent);
                });
            }

            $.when(requestIsMade).then(function() {
                chat.display.toggleAvailableUsers($('#available-users').hasClass('zero-size'));
            });
        });

        $(document).on('click', '.available-chat-user', function() {
            $(this).toggleClass('toggle');
        });

        $(document).on('click', '#available-users-confirm-btn', function() {
            var selectedUsers = $('.toggle').map(function() {
                return $(this)[0].textContent;
            });
            selectedUsers.push(chat.userName);
            selectedUsers.sort();
            selectedUsers = selectedUsers.toArray();

            if (!chat.selectExiststingRoom(selectedUsers)) {
                chat.addRoomTab(selectedUsers);
            }
            chat.display.toggleAvailableUsers(false);
        });

        $(document).on('click', '#available-users-overlay', function() {
            chat.display.toggleAvailableUsers(false);
        });

        $(document).on('mouseover', '.chat-room-tab-name, .chat-user, .available-chat-user', function() {
            if (this.offsetWidth < this.scrollWidth) {
                chat.horizontalMouseOverScroll = setInterval(function scrolling(element) {
                    if ($(element).is(':animated')) {
                        return null;
                    }
                    animateScrollLeft(element);
                    setTimeout(function() {
                        animateScrollRight(element);
                    }, 250);
                    return scrolling;
                }(this), 3500, this);
            }
        });

        $(document).on('mouseout', '.chat-room-tab-name, .chat-user, .available-chat-user', function() {
            clearInterval(chat.horizontalMouseOverScroll);
        });

        $(document).on('click', '#chat-message-send-btn', function() {
            var roomId = chat.roomSelection.data('chatRoomId');
            var recipients = chat.roomSelection.data('recipients');
            var isNew = chat.roomSelection.data('isNew');

            if (isNew === true) {
                chat.hub.server.createChatRoom(roomId, false, true, recipients).done(function() {
                    $('#room-tab' + roomId).data('isNew', false);
                });
            }

            // Call the message sending method on server.
            chat.hub.server.sendMessage(chat.newMessage.val(), roomId);
            // Clear text box and reset focus for next comment.
            chat.newMessage.val('').focus();
        });
    }

}(window.chat = window.chat || {}, jQuery));