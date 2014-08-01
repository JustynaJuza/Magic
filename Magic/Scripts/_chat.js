$(function () {
    window.chatRoomUsers = [];
    window.chatCommands = ['/msg', '/game', '/all'];
    window.chatCommand_all = function () {
        $newChatMessage.val('');
    };

    var userName = $('#user-name').text(),
        $chatContainer = $('.chat'),
    $chatRoomContainer = $('#chat-room-container'),
    $chatRooms = $('.chat-room'),
    $chatSendButton = $('.chat-message-new-btn-send'),
    $newChatMessage = $('.chat-message-new'),
    $chatMessagesContainer = $('.chat-messages-container'),
    $chatMessages = $('.chat-messages'),
    $chatMessage = $('.chat-message'),
    $chatUsersContainer = $('.chat-users-container'),
    $chatUsers = $('.chat-users'),
    $chatUser = $('.chat-user'),
    $chatRoomSelection = $('#chat-room-selection'),
    $chatRoomTabs = $('.chat-room-tab'),
    $chatRoomTabNames = $('.chat-room-tab'),
    $chatRoomContents = $('.chat-room-content'),
    basicNewMessagePadding = parseInt($newChatMessage.css('padding-left')),
    //activeChatRoom = $chatRoomSelection.data('chatRoomId'),
    tabBlinkingTracker = [],
    horizontalMouseOverScroll;

    //adjustNewMessageElementPadding();
    //adjustRoomTabs();
    $chatRoomSelection.data('chatRoomId', 'default');
    $chatRoomSelection.data('recipients', '');
    $chatRoomSelection.data('isNew', 'false');

    // Make chat sender names clickable for reply.
    $(document).on('click', '.chat-message-sender, .chat-room .chat-user', function () {
        $newChatMessage.val('@' + $(this).text() + ' ');
        $newChatMessage.focus();
    });
    $(document).on('click', '.chat-message-recipient', function () {
        $newChatMessage.val($(this).text() + ' ');
        $newChatMessage.focus();
    });

    $(document).on('click', '.chat-room-tab', function () {
        var roomId = $(this).prop('id').substr(9);
        $chat.roomTabs.css({
            'border-width': '1px',
            'box-shadow': '',
            '-webkit-box-shadow': '',
        });

        $(this).css({
            'border-width': '2px',
            'box-shadow': 'inset 0 3px 5px rgba(0, 0, 0, 0.125)',
            '-webkit-box-shadow': 'inset 0 3px 5px rgba(0, 0, 0, 0.125)',
        });

        $('#room-content-' + roomId).show();
        $chat.roomContents.not('#room-content-' + roomId).hide();
        $chat.newMessage.focus();

        if (tabBlinkingTracker[roomId]) {
            clearInterval(tabBlinkingTracker[roomId]);
            scrollContainerToBottom('#room-messages-container-' + roomId);
        }

        $chatRoomSelection.data('chatRoomId', roomId);
        $chatRoomSelection.data('recipients', $(this).data('recipients'));
        $chatRoomSelection.data('isNew', $(this).data('isNew') || false);
    });

    $(document).on('click', '.chat-room-tab-close', function (event) {
        var roomTab = $(this).closest('.chat-room-tab');
        var roomId = roomTab.prop('id').substr(9);
        if (roomTab.data('isNew') == true) {
            if ($chatRoomSelection.data('chatRoomId') == roomId) {
                $chat.roomTabs.first().trigger('click');
            }
            $('#room-' + roomId).remove();
            $chat.adjustRoomTabs();
        } else {
            window.chat.server.unsubscribeChatRoom(roomId, null);
        }
        event.stopPropagation();
    });

    $(document).on('click', '.chat-room-tab-add-user', function () {
        var requestIsMade = null;
        if (!$('#available-users').length) {
            var url = basePath + 'Chat/GetAvailableUsersPartial/';
            requestIsMade = $.get(url, function (htmlContent) {
                $chat.container.append(htmlContent);
            });
        }

        $.when(requestIsMade).then(function () {
            moveToScroll('#available-users');
            $('#available-users-overlay').show();
            $('#available-users').show();
        });
    });

    $(document).on('click', '.available-chat-user', function () {
        $(this).toggleClass('toggle');
    });

    $(document).on('click', '#available-users-confirm-btn', function () {
        var selectedUsers = $('.toggle').map(function () {
            return $(this)[0].textContent;
        });

        $chat.addRoomTab(selectedUsers.toArray());
        $('#available-users-overlay').fadeOut();
        $('#available-users').fadeOut();
    });

    $(document).on('click', '#available-users-close-btn', function () {
        $('#available-users-overlay').fadeOut();
        $('#available-users').fadeOut();
    });

    //$(document).on('click', '.available-chat-user', function () {
    //var roomId = $chatRoomSelection.data('chatRoomId');
    //var recipients = $chatRoomSelection.data('recipients');

    //$('#available-users-overlay').fadeOut();
    //$('#available-users').fadeOut();
    //window.chat.server.addChatUser($chatRoomSelection);
    ////roomTab.data('recipients', Array(roomTab.data('recipients')).push(member));
    //});

    $(document).on('mouseover', '.chat-room-tab-name, .chat-user, .available-chat-user', function () {
        if (this.offsetWidth < this.scrollWidth) {
            var overflow = this.scrollWidth - this.offsetWidth + 2;
            horizontalMouseOverScroll = setInterval(function scrolling(element) {
                if ($(element).is(':animated')) {
                    return null;
                }
                scrollLeft(element, overflow);
                setTimeout(function() {
                    scrollRight(element);
                }, 250);
                return scrolling;
            }(this), 3500, this);
        }
    });

    $(document).on('mouseout', '.chat-room-tab-name, .chat-user, .available-chat-user', function () {
        clearInterval(horizontalMouseOverScroll);
    });

    //$('.chat-room-tab, .chat-user, .chat-message-sender').hover(function () {
    //    $(this).fadeTo(0, 0.8);
    //}, function () {
    //    $(this).fadeTo(0, 1);
    //});

    $(document).on('click', '.chat-message-new-btn-send', function () {
        if (typeof window.chat.initialized != 'undefined') {
            var roomId = $chatRoomSelection.data('chatRoomId');
            var recipients = $chatRoomSelection.data('recipients');
            var isNew = $chatRoomSelection.data('isNew');

            if (isNew == true) {
                window.chat.server.createChatRoom(recipients, roomId).done(function () {
                    $('#room-tab' + roomId).data('isNew', false);
                });
            }

            // Call the message sending method on server.
            window.chat.server.send($newChatMessage.val(), roomId);
            // Clear text box and reset focus for next comment.
            $newChatMessage.val('').focus();
        }
    });

    $(document).on('dblclick', '.chat-message-sender, .chat-room .chat-user', function () {
        checkIfRoomExists($(this).text());

        $chat.addRoomTab($(this).text());
        $chat.newMessage.focus();
    });

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

        this.container = $chatContainer;
        this.roomTabs = $chatRoomTabs;
        this.roomTabNames = $chatRoomTabNames;
        this.roomContents = $chatRoomContents;
        this.usersContainer = $chatUsersContainer;
        this.users = $chatUsers;
        this.messageLogContainer = $chatMessagesContainer;

        this.roomsContainer = $chatRoomContainer;
        this.rooms = $chatRooms;
        this.newMessage = $newChatMessage;

        this.addRoomTab = function (recipientNames, roomId) {
            var recipients = [];
            if (recipientNames instanceof Array) {
                recipients = [userName].concat(recipientNames);
            } else {
                recipients = [recipientNames, userName];
            }
            var isExistingRoom = roomId != null;
            var activateTabAfterwards = roomId == null;
            var url = basePath + 'Chat/GetChatRoomPartial/';

            // Extension used to append new room markup to chat.
            function appendRoomToChat(htmlContent) {
                $chat.roomsContainer.append(htmlContent);
                $chat.adjustRoomTabs();

                $('#room-tab-' + roomId).data('recipients', recipients);
                $('#room-tab-' + roomId).data('isNew', !isExistingRoom);
                if (activateTabAfterwards) {
                    $('#room-tab-' + roomId).trigger('click');
                }
                else {
                    $('#room-content-' + roomId).hide();
                }
                scrollContainerToBottom('#room-messages-container-' + roomId);

            }

            if (roomId) {
                // Room id already known, get markup only.
                $.get(url, { roomId: roomId }, appendRoomToChat);
            }
            else {
                // Request information for room with selected memebers.
                window.chat.server.getChatRoom(recipients)
                    .done(function (existingRoomId) {
                        isExistingRoom = existingRoomId.length != 0;
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
                            jQuery.ajaxSettings.traditional = true;
                            $.get(url, { recipientNames: recipients }, function (htmlContent) {
                                roomId = $($.parseHTML(htmlContent)).find('.chat-room-tab').prop('id').substr(9);
                                appendRoomToChat(htmlContent);
                            });
                        }
                    });
            }
        };

        this.removeRoomTab = function (roomId) {
            this.roomsContainer.find('#room-' + roomId).remove();
            this.adjustRoomTabs();
        };

        this.adjustRoomTabs = function () {
            this.roomTabs = $(this.roomTabs.selector);
            this.roomContents = $(this.roomContents.selector);

            var tabCount = this.roomTabs.length;
            var avgWidth = 100 / tabCount;
            this.roomTabs.css({
                'width': avgWidth + '%',
                'margin-left': function (index) {
                    return avgWidth * index + '%';
                }
            });


            // Tabs closed - only default chat room left, hide tab bar.
            if (tabCount <= 1) {
                $chatRoomSelection.data('chatRoomId', 'default');
                $chatRoomSelection.data('recipients', '');

                //$('.chat-room-content').animate({ 'border-top-left-radius': '4px', 'border-top-right-radius': '4px' }, 350);
                //$('.chat-room-tab').slideUp();
            }
            //else {
            //    $('.chat-room-content').css({ 'border-top-left-radius': 0, 'border-top-right-radius': 0 });
            //    $('.chat-room-tab').slideDown();
            //}
        }

        this.joinChatRoom = function () { };
    }

    // ---------------------------- HUB ---------------------------- BEGIN
    // Reference the auto-generated proxy for the hub.
    window.chat = $.connection.chatHub;
    var $chat = new Chat();
    $chat.adjustRoomTabs();
    $chat.roomTabs.first().trigger('click');
    scrollContainerToBottom('#room-messages-container-default');

    // Initialize chat handling.
    window.chat.initialize = function initializeChat() {
        //chat.server.subscribeChatRoom('default');
        //chat.server.getChatRoomUsers('default');
    };

    // Hub callback delivering new messages.
    window.chat.client.addMessage = function (roomId, time, sender, senderColor, message) {
        if (!$('#room-' + roomId).length) {
            $chat.addRoomTab(sender, roomId);
        }

        $('#room-messages-' + roomId).append('<li class="chat-message">' + time + ' <span class="chat-message-sender" style="font-weight:bold;color:' + htmlEncode(senderColor) + '">' + htmlEncode(sender)
            + ' </span>' + htmlEncode(message) + '</li>');

        if ($chatRoomSelection.data('chatRoomId') != roomId) {
            var tabBlinkerProcess = setInterval(blink, 1000, '#room-tab-' + roomId);
            tabBlinkingTracker[roomId] = tabBlinkerProcess;
        } else {
            scrollContainerToBottom('#room-messages-container-' + roomId);
        }
    };

    window.chat.client.closeChatRoom = function (roomId) {
        if ($chatRoomSelection.data('chatRoomId') == roomId) {
            $chat.roomTabs.first().trigger('click');
        }
        $('#room-' + roomId).remove();
        $chat.adjustRoomTabs();
    }

    window.chat.client.loadChatRoom = function (roomId) {

    }

    // Hub callback for updating chat user list on each change.
    window.chat.client.updateChatRoomUsers = function (chatUsers, roomId) {
        chatUsers = $.parseJSON(chatUsers);
        var updatedChatUsersHtml = '';
        for (var i = 0; i < chatUsers.length; i++) {
            updatedChatUsersHtml += '<li class="chat-user" style="color:' + htmlEncode(chatUsers[i].ColorCode) + '">' + chatUsers[i].UserName + '</li>';
        }

        $('#room-users-' + roomId).children().remove();
        $('#room-users-' + roomId).append(updatedChatUsersHtml);
    };

    // Hub callback to remove chat room tab when current user leaves.
    window.chat.client.leaveChatRoom = function (roomId) {
        $('.chat-room-users-selectlist #' + roomId).remove();
        $('#chat-room-selectlist #' + roomId).remove();
        adjustRoomTabs();
    }

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
        } else {
            $chatSendButton.prop('disabled', false);
        }
    });

    // Send messages on enter.
    $newChatMessage.keyup(function (e) {
        if (e.keyCode == 13 && $newChatMessage.val().length > 0) {
            $chatSendButton.toggleClass('clicked');
            $chatSendButton.trigger('click');
            $chatSendButton.prop('disabled', true);
        }
        else if (e.keyCode == 32 && $newChatMessage.val().split(' ').length == 2) {
            if (_.any(chatCommands, matchNewChatMessage)) {
                console.log(window['chatCommand_' + $newChatMessage.val().split(' ')[0].toLowerCase()]);
                window['chatCommand_' + $newChatMessage.val().split(' ')[0].toLowerCase()].apply();
            };
        }
    });

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

    // Enable smooth scrolling chat messages and user list.
    //$chatMessagesContainer.scroll(smoothScroll($chatMessagesContainer, $chatMessage));
    //$chatUsersContainer.scroll(smoothScroll($chatUsersContainer, $chatUser)); 

    // Html-encode messages for display in the page.
    function htmlEncode(value) {
        var encodedValue = $('<div />').text(value).html();
        return encodedValue;
    }

    //function adjustNewMessageElementPadding() {
    //    var newPadding = basicNewMessagePadding + parseInt($chatRoomSelection.css('width'));
    //    $newChatMessage.css('padding-left', newPadding);
    //}

    //$chatRoomSelection.click(function () {
    //    $chatRoomSelectList.toggle();
    //});

    //$chatRoomSelectList.select(function () {
    //    alert('select in list changed')
    //    activeChatRoom = $chatRoomSelection.prop('id');
    //});

    // ---------------- CHAT DISPLAY & FUNCTIONALITY --------------- END

    // --------------------- HELPER FUNCTIONS ---------------------- START
    function smoothScroll($container, $scrollingEntry) {
        alert('scroll');
        var lineHeightInPixels = 20;
        var marginSize = 10;
        var linesVisible = ($container.height() / lineHeightInPixels).toFixed(0);
        var linesTotal = (($container[0].scrollHeight - marginSize) / lineHeightInPixels).toFixed(0);

        // Get number of oldest message lines to fade out based on line height and scroll position.
        var linesToFadeUpper = ($container.scrollTop() / lineHeightInPixels).toFixed(0);
        console.log(linesToFadeUpper);
        // Fade upper lines out.
        $scrollingEntry.slice(0, linesToFadeUpper).fadeTo(0, 0.01);
        // Fade visible lines in.
        $scrollingEntry.slice(linesToFadeUpper, linesToFadeUpper + linesVisible).fadeTo(0, 1);
        // Fade lower lines out.
        $scrollingEntry.slice(linesToFadeUpper + linesVisible, linesTotal).fadeTo(0, 0.01);
    }

    function matchNewChatMessage(value) {
        return $newChatMessage.val().split(' ')[0].toLowerCase() == value;
    }

    function checkIfRoomExists(selectedUser) {
        var exisitingRoom = _.find($chat.roomTabNames, function (element) {
            return element.textContent.substr(1, element.textContent.length - 2) == selectedUser;
        });

        if (exisitingRoom) {
            $('#room-' + exisitingRoom.id).trigger('click');
        }
    }

    function scrollLeft(element, overflow) {
        $(element).animate({ scrollLeft: overflow }, 1625);
    }

    function scrollRight(element) {
        $(element).animate({ scrollLeft: 0 }, 1625);
    }

    function blink(element) {
        $(element).fadeTo(500, 0.75);
        $(element).fadeTo(500, 1);
    }

    function scrollContainerToBottom(element) {
        $(element).animate({ scrollTop: $(element)[0].scrollHeight }, 1000);
    }

    function moveToScroll(element) {
        var top = $(window).scrollTop();
        $(element).css('top', top + 'px');
    }
    // --------------------- HELPER FUNCTIONS ---------------------- END
});