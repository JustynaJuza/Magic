$(function () {
    window.chatRoomUsers = [];
    window.chatCommands = ['/msg', '/game', '/all'];
    window.chatCommand_all = function () {
        $newChatMessage.val('');
    };

    var userName = $('#user_name').text();
    $chatSendButton = $('.new-chat-message-send'),
    $newChatMessage = $('.new-chat-message'),
    $chatMessagesContainer = $('.chat-messages-container'),
    $chatMessages = $('.chat-messages'),
    $chatMessage = $('.chat-message'),
    $chatUsersContainer = $('.chat-users-container'),
    $chatUsers = $('.chat-users'),
    $chatUser = $('.chat-user'),
    $chatRoomSelection = $('#chat-room-selection'),
    $chatRoomSelectList = $('#chat-room-selectlist'),
    $chatRoomSelectListTabs = $('#chat-room-selectlist li'),
    $chatRoomContainer = $('#chat_room_container');
    $chatRooms = $('.chat-room'),
    basicNewMessagePadding = parseInt($newChatMessage.css('padding-left'));
    activeChatRoom = $chatRoomSelection.data('chatRoomId'),
    tabBlinkingTracker = [];


    //$chatGeneralCheckbox = $('#chat-messages-general-check'),
    //$chatPrivateCheckbox = $('#chat-messages-private-check'),
    //$chatRoomUsersSelectList = $('.chat-room-users-selectlist'),
    //$chatRoomUsersSelection = 

    //adjustNewMessageElementPadding();
    //adjustRoomTabs();
    $chatRoomSelection.data('chatRoomId', 'default');
    $chatRoomSelection.data('recipients', '');


    function ChatRoomUsers(chatUsersHtml, roomId, roomName) {
        /// <summary>Creates an object storing the chat room user list related identified by room name, can also be used as clone constructor.</summary>
        /// <param name="chatUsersHtml" type="String" optional="false">The chat room related user list in HTML markup.</param>
        /// <param name="roomId" type="String">The chat room identifier.</param>
        /// <param name="roomName" type="String" optional="false">The chat room display name.</param>
        /// <returns type="ChatRoomUsers">An object storing the related chat room user list identified by room name.</returns>
        /// <field name="chatUsersHtml" type="String">The chat room related user list in HTML markup.</field>
        /// <field name="roomId" type="String">The chat room identifier.</field>
        /// <field name="roomName" type="String">The chat room display name.</field>
        if (arguments.length > 1) {
            this.chatUsersHtml = chatUsersHtml;
            this.roomId = roomId;
            this.roomName = roomName;
        }
        else {
            this.chatUsersHtml = arguments[0].chatUsersHtml;
            this.roomId = arguments[0].roomId;
            this.roomName = arguments[0].roomName;
        }
    }

    function Chat() {
        this.defaultRecipients = '';
        this.defaultRoomName = 'default';

        this.roomSelectList = $chatRoomSelectList;
        this.roomTabs = $chatRoomSelectListTabs;
        this.usersContainer = $chatUsersContainer;
        this.users = $chatUsers;
        this.messageLogContainer = $chatMessagesContainer;

        this.rooms = $chatRooms;
        this.roomsContainer = $chatRoomContainer;
        this.newMessage = $newChatMessage;

        this.setChatUsers = function (chatUsers, roomId) {
            this.usersContainer.find('#messages-' + roomId);
        }

        this.addRoomTab = function (recipientName, tabColor) {
            var recipients = [recipientName, userName];

            window.chat.server.getChatRoom(recipients).done(function (roomId) {
                if (roomId.length == 0) {
                    var roomTab = $($.parseHTML('<li style="background-color: ' + tabColor
                        + '"><span class="chat-tab-btn-add-member">+</span>' + recipientName + '<span class="chat-tab-btn-close">X</span></li>'));
                    var url = '/Chat/GetChatRoomPartial/';
                    jQuery.ajaxSettings.traditional = true;
                    $.get(url, { recipientNames: recipients }, function (htmlContent) {
                        $chat.roomsContainer.append(htmlContent);
                        roomId = $($.parseHTML(htmlContent)).prop('id').substr(10);
                    }).done(function () {
                        roomTab.prop('id', roomId);
                        scrollContainerToBottom('#chat_messages_container-' + roomId);
                        roomTab.trigger('click');
                    });
                    roomTab.data('recipients', recipients);
                    roomTab.data('isNew', true);
                    $chat.roomSelectList.append(roomTab);
                    $chat.adjustRoomTabs();
                }
                else {
                    if ($('#' + roomId).length) {
                        return $('#' + roomId).trigger('click');
                    }

                    var url = '/Chat/GetChatRoomPartial/';
                    $.get(url, { roomId: roomId }, function (htmlContent) {
                        $chat.roomsContainer.append(htmlContent);
                    }).done(function () {
                        roomTab.trigger('click');
                        scrollContainerToBottom('#chat_messages_container-' + roomId);
                    });
                    var roomTab = $($.parseHTML('<li id="' + roomId + '" style="background-color: ' + tabColor
                        + '"><span class="chat-tab-btn-add-member">+</span>' + recipientName + '<span class="chat-tab-btn-close">X</span></li>'));

                    roomTab.data('recipients', recipients);
                    roomTab.data('isNew', false);
                    $chat.roomSelectList.append(roomTab);
                    $chat.adjustRoomTabs();
                }
            });
        };

        this.removeRoomTab = function (roomId) {
            this.roomSelectList.find('#' + roomId).remove();
            this.adjustRoomTabs();
        };

        this.adjustRoomTabs = function () {
            var tabCount = this.roomSelectList.children().length
            var avgWidth = 100 / tabCount;
            this.roomSelectList.children('li').css('width', avgWidth + '%');

            // Tabs closed - only default chat room left, hide tab bar.
            if (tabCount <= 1) {
                $chatRoomSelection.data('chatRoomId', 'default');
                $chatRoomSelection.data('recipients', '');

                $('#chat_room_container').animate({ 'border-top-left-radius': '4px', 'border-top-right-radius': '4px' }, 350);
                return this.roomSelectList.slideUp();
            }
            else {
                $('#chat_room_container').css({ 'border-top-left-radius': 0, 'border-top-right-radius': 0 });
            }

            this.roomSelectList.slideDown();
        }

        this.joinChatRoom = function () { };
    }

    // ---------------------------- HUB ---------------------------- BEGIN
    // Reference the auto-generated proxy for the hub.
    window.chat = $.connection.chatHub;
    var chat = window.chat;
    var $chat = new Chat();

    // Initialize chat handling.
    window.chat.initialize = function initializeChat() {
        //chat.server.subscribeChatRoom('default');
        //chat.server.getChatRoomUsers('default');
    }

    // Hub callback delivering new messages.
    window.chat.client.addMessage = function (roomId, time, sender, senderColor, message) {
        $('#chat_messages-' + roomId).append('<li class="chat-message">' + time + ' <span class="chat-message-sender" style="font-weight:bold;color:' + htmlEncode(senderColor) + '">' + htmlEncode(sender)
            + ' </span>' + htmlEncode(message) + '</li>');

        if ($chatRoomSelection.data('chatRoomId') != roomId) {
            var tabBlinkerProcess = setInterval(blink, 1000, '#' + roomId);
            tabBlinkingTracker[roomId] = tabBlinkerProcess;
        }
        else {
            scrollContainerToBottom('#chat_messages_container-' + roomId);
        }
    };

    window.chat.client.loadChatRoom = function (roomId, roomName, tabColors, recipients, chatLog) {
        var roomTabHtml = '<li id="' + roomId + '" style="background-color: ' + tabColors[0] + '"><span class="chat-tab-btn-add-member">+</span>';
        if (roomName != null) {
            roomTabHtml += roomName;
        }
        else {
            recipients.splice(recipients.indexOf(userName), 1);
            recipients.forEach(function (recipient) {
                roomTabHtml += recipient + ' | ';
            });
            roomTabHtml = roomTabHtml.substring(0, roomTabHtml.length - 3)
        }
        roomTabHtml += '<span class="chat-tab-btn-close">X</span></li>';

        var roomTab = $($.parseHTML(roomTabHtml));
        roomTab.data('recipients', recipients);
        $chat.roomSelectList.append(roomTab);
        $chat.adjustRoomTabs();
        roomTab.trigger('click');
    }

    //TODO: Check and edit.
    // Hub callback for updating chat user list on each change.
    window.chat.client.updateChatRoomUsers = function (chatUsers, roomId) {
        chatUsers = $.parseJSON(chatUsers);
        var updatedChatUsersHtml = ''; //'<ul id="chat_users-' + roomId + '" class="chat-users">';
        for (var i = 0; i < chatUsers.length; i++) {
            updatedChatUsersHtml += '<li class="chat-user" style="color:' + htmlEncode(chatUsers[i].ColorCode) + '">' + chatUsers[i].UserName + '</li>';
        }

        $chat.users.children().remove();
        $chat.users.append(updatedChatUsersHtml);

        //updatedChatUsersHtml += '</ul>';

        //var foundChatRoomUsers = _.find(window.chatRoomUsers, function (element, index) {
        //    return element.roomId == roomId;
        //});

        //if (foundChatRoomUsers != null) {
        //    foundChatRoomUsers.chatUsersHtml = updatedChatUsersHtml;
        //}
        //else {
        //    window.chatRoomUsers.push(new ChatRoomUsers(updatedChatUsersHtml, roomId));
        //}

        //filterChatUsers(roomId);
    };

    window.chat.client.updateChatRoomUser = function (userName, colorCode, roomId) {

        if (activeChatRoom == roomId) {
            var $foundUser = $('.chat-user:contains(' + userName + ')');
            if ($foundUser.length > 0) {
                $foundUser.remove();
            }
            else {
                $chat.users.append('<li class="chat-user" style="font-weight:bold;color:' + htmlEncode(colorCode) + '">' + userName + '</li>');
            }
        }
    };

    window.chat.client.updateChatTab = function (recipients, recipientColors, roomId) {
        var targetRoom = _.find($chat.roomSelectList.find('li'), function (roomTab) {
            var savedRecipients = $(roomTab).data('recipients');
            return $(savedRecipients).not(recipients).length == 0 && $(recipients).not(savedRecipients).length == 0;
        });
        $(targetRoom).prop('id', roomId);
    }

    window.chat.client.loadActiveChatRooms = function (chatRooms) {
        console.log(recipients, roomId);
    }

    // Hub callback to remove chat room tab when current user leaves.
    window.chat.client.leaveChatRoom = function (roomId) {
        $('.chat-room-users-selectlist #' + roomId).remove();
        $('#chat-room-selectlist #' + roomId).remove();
        adjustRoomTabs();
    }

    // Hub callback to add  chat room tab when current user joins.
    //window.chat.client.joinChatRoom = function (roomId, roomName, tabColor, tabBorderColor) {
    //    $chatRoomUsersSelectList.append('<li id=' + roomId + 'style="background-color: ' + tabColor + '; border-color: ' + tabBorderColor + '">' + roomName + '</li>');
    //    $chatRoomSelectList.append('<li data-recipients id=' + roomId + 'style="color: ' + tabColor + '">' + roomName + '<span class="chat-tab-btn-close">X</span></li>');
    //    adjustRoomTabs();
    //}

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
        else if (e.keyCode == 32 && $newChatMessage.val().split(' ').length == 2) {
            if (_.any(chatCommands, matchNewChatMessage)) {
                console.log(window['chatCommand_' + $newChatMessage.val().split(' ')[0].toLowerCase()])
                window['chatCommand_' + $newChatMessage.val().split(' ')[0].toLowerCase()].apply();
            };
        }
    });

    // Provide checkboxes for hiding general/private messages.
    // TODO: Check why this doesn't work sometimes... Maybe bacause of dynamically added elements? Maybe because of fading scroll?
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

    // Enable smooth scrolling chat messages and user list.
    //$chatMessagesContainer.scroll(smoothScroll($chatMessagesContainer, $chatMessage));
    //$chatUsersContainer.scroll(smoothScroll($chatUsersContainer, $chatUser));

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
        alert('scroll')
        var lineHeightInPixels = 20;
        var marginSize = 10;
        var linesVisible = ($container.height() / lineHeightInPixels).toFixed(0);
        var linesTotal = (($container[0].scrollHeight - marginSize) / lineHeightInPixels).toFixed(0);

        // Get number of oldest message lines to fade out based on line height and scroll position.
        var linesToFadeUpper = ($container.scrollTop() / lineHeightInPixels).toFixed(0);
        console.log(linesToFadeUpper)
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

    $chatRoomSelection.click(function () {
        $chatRoomSelectList.toggle();
    });

    $chatRoomSelectList.select(function () {
        alert('select in list changed')
        activeChatRoom = $chatRoomSelection.prop('id');
    });

    $(document).on('dblclick', '.chat-message-sender, .chat-user', function () {
        checkIfRoomExists($(this).text());

        $chat.addRoomTab($(this).text(), $(this).css('color'));
        $chat.newMessage.focus();
        //$chatRoomSelectList.hide();
        //$chatRoomSelection.val($(this).text());
        //$chatRoomSelection.attr('data-recipient', $(this).text());
        //var roomId = $(this).prop('id');
        //if (roomId.length) {
        //    $chatRoomSelection.prop('id', roomId);
        //}
        //$chatRoomSelection.css('color', $(this).css('color'));
        //adjustNewMessageElementPadding();
        //$newChatMessage.focus();
    });

    $(document).on('click', '.chat-tab-btn-close', function (event) {
        $(this).parent().remove();
        $chat.adjustRoomTabs();
        event.stopPropagation();
        $('#default').trigger('click');
    });

    $(document).on('click', '.chat-tab-btn-add-member', function () {
        var roomTab = $(this).parent();
        roomTab.data('recipients', Array(roomTab.data('recipients')).push(member))
        $('#user-list-add-member');
    });

    function showUserList() {
        $('#file-property-id').val($(this).prop('id').substr(3));

        moveToScroll('#file-uploader');
        $('#file-uploader-overlay').show();
        $('#file-uploader').show();
    }

    function closeFileUploader() {
        $('#file-property-id').val('');
        $('#file-uploader-selected-file-overlay').val('Select or drag and drop file');
        $('#file-uploader-selected-file').val('');
        $('#file-uploader-overlay').hide();
        $('#file-uploader').hide();
    }

    $(document).on('mouseover', '#chat-room-selectlist li', function () {
        $(this).fadeTo(0, 0.8);
    });

    $(document).on('mouseout', '#chat-room-selectlist li', function () {
        $(this).fadeTo(0, 1);
    });

    $(document).on('click', '#chat-room-selectlist li', function () {
        $('.chat-room').hide()
        $('#chat-room-selectlist li').css('border-width', '1px');
        $('#chat-room-selectlist li').css({ 'box-shadow': '', '-webkit-box-shadow': '' });
        $(this).css('border-width', '2px');
        $(this).css({ 'box-shadow': 'inset 0 3px 5px rgba(0, 0, 0, 0.125)', '-webkit-box-shadow': 'inset 0 3px 5px rgba(0, 0, 0, 0.125)' });
        if (tabBlinkingTracker[$(this).prop('id')]) {
            clearInterval(tabBlinkingTracker[$(this).prop('id')]);
            scrollContainerToBottom('#chat_messages_container-' + $(this).prop('id'));
        }
        $('#chat_room-' + $(this).prop('id')).show();
        $chatRoomSelection.data('chatRoomId', $(this).prop('id'));
        $chatRoomSelection.data('recipients', $(this).data('recipients'));
        $chatRoomSelection.data('isNew', $(this).data('isNew') || false);
    });

    $(document).on('click', '.new-chat-message-send', function () {
        if (typeof window.chat.initialized != 'undefined') {
            var chatRoomId = $chatRoomSelection.data('chatRoomId');
            var recipients = $chatRoomSelection.data('recipients');
            var isNew = $chatRoomSelection.data('isNew');

            if (isNew == true) {
                window.chat.server.createChatRoom(recipients, chatRoomId).done(function () {
                    $('#' + chatRoomId).data('isNew', false)
                });
            }

            // Call the message sending method on server.
            window.chat.server.send($newChatMessage.val(), chatRoomId);
            // Clear text box and reset focus for next comment.
            $newChatMessage.val('').focus();
        }
    });

    //function filterChatUsers(roomId) {
    //    var chatRoomUsers = _.find(window.chatRoomUsers, function (element) {
    //        return element.roomId == roomId;
    //    });

    //    console.log(chatRoomUsers)
    //    $chatUsers.replaceWith(chatRoomUsers.chatUsersHtml);
    //    // Refresh the variable content.
    //    $chatUsers = $($chatUsers.selector);
    //}
    // ---------------- CHAT DISPLAY & FUNCTIONALITY --------------- END

    // --------------------- HELPER FUNCTIONS ---------------------- START
    function matchNewChatMessage(value) {
        return $newChatMessage.val().split(' ')[0].toLowerCase() == value;
    }

    function checkIfRoomExists(userName) {
        var userName = $(this).text();
        var exisitingRoom = _.find($chat.roomSelectList.find('li'), function (element) {
            return element.textContent.substr(1, element.textContent.length - 2) == userName;
        });

        if (exisitingRoom != null) {
            return $('#' + exisitingRoom.id).trigger('click');
        }
    }

    function blink(element) {
        $(element).fadeTo(500, 0.75);
        $(element).fadeTo(500, 1);
    }

    function scrollContainerToBottom(element) {
        $(element).animate({ scrollTop: $(element)[0].scrollHeight }, 1000);
    }
    // --------------------- HELPER FUNCTIONS ---------------------- END
});