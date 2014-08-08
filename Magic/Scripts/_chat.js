$(function () {

    window.chat = $.connection.chatHub;
    window.chatRoomUsers = [];

    var $chat = new chat();

    var userName = $('#user-name').text(),
    tabBlinkingTracker = [],
    horizontalMouseOverScroll;

    $(document).on('click', '.chat-user', function () {
        clearTimeout(window.clickTimer);
        window.clickTimer = setTimeout(function (element) {
            console.log('singleclick');
            showUserTooltip();
            return $(element).data('click', false);
        }, 350, this);

        if ($(this).data('click')) {
            clearTimeout(window.clickTimer);
            console.log('dblclick');
            if (!selectExiststingRoom($(this).text())) {
                $chat.addRoomTab($(this).text());
                $chat.newMessage.focus();
            }
            return $(this).data('click', false);
        }
        return $(this).data('click', true);
    });

    //$(document).on('dblclick', '.chat-user', function () {
    //    console.log('set')
    //    $(this).data('dblclick', true);
    //});

    $(document).on('click', '.chat-message-recipient, .chat-message-sender', function () {
        $chat.newMessage.val($chat.newMessage.text() + '@' + $(this).text() + ' ');
        $chat.newMessage.focus();
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

        $chat.roomSelection.data('chatRoomId', roomId);
        $chat.roomSelection.data('recipients', $(this).data('recipients'));
        $chat.roomSelection.data('isNew', $(this).data('isNew') || false);
    });

    $(document).on('click', '.chat-room-tab-close', function (event) {
        var roomTab = $(this).closest('.chat-room-tab');
        var roomId = roomTab.prop('id').substr(9);
        if (roomTab.data('isNew') == true) {
            if ($chat.roomSelection.data('chatRoomId') == roomId) {
                $chat.roomTabs.first().trigger('click');
            }
            $('#room-' + roomId).remove();
            $chat.adjustRoomTabs();
        } else {
            window.chat.server.unsubscribeChatRoom(roomId);
        }
        event.stopPropagation();
    });

    $(document).on('click', '.chat-room-tab-add-user', function () {
        var requestIsMade = null;
        if (!$('#available-users').length) {
            var url = window.basePath + 'Chat/GetAvailableUsersPartial/';
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

    $(document).on('mouseover', '.chat-room-tab-name, .chat-user, .available-chat-user', function () {
        if (this.offsetWidth < this.scrollWidth) {
            var overflow = this.scrollWidth - this.offsetWidth + 2;
            horizontalMouseOverScroll = setInterval(function scrolling(element) {
                if ($(element).is(':animated')) {
                    return null;
                }
                scrollLeft(element, overflow);
                setTimeout(function () {
                    scrollRight(element);
                }, 250);
                return scrolling;
            }(this), 3500, this);
        }
    });

    $(document).on('mouseout', '.chat-room-tab-name, .chat-user, .available-chat-user', function () {
        clearInterval(horizontalMouseOverScroll);
    });

    $(document).on('click', '#chat-message-send-btn', function () {
        if (typeof window.chat.initialized != 'undefined') {
            var roomId = $chat.roomSelection.data('chatRoomId');
            var recipients = $chat.roomSelection.data('recipients');
            var isNew = $chat.roomSelection.data('isNew');

            if (isNew == true) {
                window.chat.server.createChatRoom(roomId, false, true, recipients).done(function () {
                    $('#room-tab' + roomId).data('isNew', false);
                });
            }

            // Call the message sending method on server.
            window.chat.server.send($chat.newMessage.val(), roomId);
            // Clear text box and reset focus for next comment.
            $chat.newMessage.val('').focus();
        }
    });

    function chat() {
        this.container = $('.chat');
        this.roomsContainer = $('#chat-rooms-container');
        this.rooms = $('.chat-room');
        this.roomTabs = $('.chat-room-tab');
        this.roomTabNames = $('.chat-room-tab-name');
        this.roomContents = $('.chat-room-content');

        this.roomSelection = $('#chat-room-selection'),

        this.messagesContainers = $('.chat-room-messages-container');
        this.usersContainers = $('.chat-room-users-container');

        this.newMessage = $('#chat-message-new');
        this.sendButton = $('#chat-message-send-btn'),

        this.commands = ['/msg', '/game', '/all'];
        this.command_all = function () {
            $chat.newMessage.val('');
        };

        this.addRoomTab = function (recipientNames, roomId, isAsyncRequest) {
            var recipients = [];
            if (recipientNames instanceof Array) {
                recipients = [userName].concat(recipientNames);
            } else {
                recipients = [recipientNames, userName];
            }
            var isExistingRoom = roomId != null;
            var activateTabAfterwards = roomId == null;
            var url = window.basePath + 'Chat/GetChatRoomPartial/';

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

            jQuery.ajaxSetup({
                async: isAsyncRequest,
                traditional: true
            });

            console.log('get')
            if (roomId) {
                // Room id already known, get markup only.
                $.get(url, { roomId: roomId }, appendRoomToChat);
            }
            else {
                // Request information for room with selected memebers.
                window.chat.server.getExistingChatRoomIdForUsers(recipients)
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
                $chat.roomSelection.data('chatRoomId', 'default');
                $chat.roomSelection.data('recipients', '');

                setTimeout(toggleTabBar(), 150);
            }
            else {
                toggleTabBar(true);
            }
        }

        this.joinChatRoom = function () { };
    }

    $chat.adjustRoomTabs();
    $chat.roomTabs.first().trigger('click');
    scrollContainerToBottom('#room-messages-container-default');

    // ---------------------------- HUB ---------------------------- BEGIN
    // Reference the auto-generated proxy for the hub.

    // Initialize chat handling.
    window.chat.initialize = function initializeChat() {
    };

    // Hub callback delivering new messages.
    window.chat.client.addMessage = function (roomId, time, sender, senderColor, message) {
        if (!$('#room-' + roomId).length) {
            $chat.addRoomTab(sender, roomId, false);
        }

        $('#room-messages-' + roomId).append('<li class="chat-message">' + time + ' <span class="chat-message-sender" style="font-weight:bold;color:' + htmlEncode(senderColor) + '">' + htmlEncode(sender)
            + ' </span>' + htmlEncode(message) + '</li>');

        if ($chat.roomSelection.data('chatRoomId') != roomId) {
            var tabBlinkerProcess = setInterval(blink, 1000, '#room-tab-' + roomId);
            tabBlinkingTracker[roomId] = tabBlinkerProcess;
        } else {
            scrollContainerToBottom('#room-messages-container-' + roomId);
        }
    };

    window.chat.client.closeChatRoom = function (roomId) {
        if ($chat.roomSelection.data('chatRoomId') == roomId) {
            $chat.roomTabs.first().trigger('click');
        }
        $('#room-' + roomId).remove();
        $chat.adjustRoomTabs();
    }

    // Hub callback for updating chat user list on each change.
    window.chat.client.updateChatRoomUsers = function (chatUsers, roomId) {
        chatUsers = $.parseJSON(chatUsers);
        var updatedChatUsersHtml = '';
        for (var i = 0; i < chatUsers.length; i++) {
            updatedChatUsersHtml += '<li class="chat-user" style="color:' + htmlEncode(chatUsers[i].ColorCode) + (chatUsers[i].Status == 'Offline' ? "; font-weight:normal" : ';">') + chatUsers[i].UserName + '</li>';
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
    // ---------------------------- HUB ---------------------------- END

    // ---------------- CHAT DISPLAY & FUNCTIONALITY --------------- START
    // Send button enabled only on chat message input.
    $chat.sendButton.prop('disabled', true);
    $chat.newMessage.on('input', function () {
        if ($chat.newMessage.val() == '') {
            $chat.sendButton.prop('disabled', true);
        } else {
            $chat.sendButton.prop('disabled', false);
        }
    });

    // Send messages on enter.
    $chat.newMessage.keyup(function (e) {
        if (e.keyCode == 13 && $chat.newMessage.val().length > 0) {
            $chat.sendButton.toggleClass('clicked');
            $chat.sendButton.trigger('click');
            $chat.sendButton.prop('disabled', true);
        }
        else if (e.keyCode == 32 && $chat.newMessage.val().split(' ').length == 2) {
            if (_.any($chat.commands, matchNewChatMessage)) {
                $chat['command_' + $chat.newMessage.val().split(' ')[0].toLowerCase()].apply();
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

    // Enable smooth scrolling chat messages and chat users.
    //$chat.messagesContainers.each(function() {
    //    $(this).scroll(smoothScroll($(this), $('.chat-message')));
    //});
    //$chat.usersContainers.each(function() {
    //    $(this).scroll(smoothScroll($(this), $('.chat-user')));
    //});

    //function adjustNewMessageElementPadding() {
    //    var newPadding = basicNewMessagePadding + parseInt($chat.roomSelection.css('width'));
    //    $newChatMessage.css('padding-left', newPadding);
    //}

    //$chat.roomSelection.click(function () {
    //    $chatRoomSelectList.toggle();
    //});

    //$chatRoomSelectList.select(function () {
    //    alert('select in list changed')
    //    activeChatRoom = $chat.roomSelection.prop('id');
    //});

    // ---------------- CHAT DISPLAY & FUNCTIONALITY --------------- END

    //$('.chat-user').tooltip();
    //$('.chat-user').tooltipster({
    //    trigger: 'click',
    //    interactive: 'true',
    //    content: 'Loading...',
    //    functionBefore: function (element, continueTooltip) {
    //        alert(0)
    //        // we'll make this function asynchronous and allow the tooltip to go ahead and show the loading notification while fetching our data
    //        //continueTooltip();

    //        // next, we want to check if our data has already been cached
    //        if (!element.data('ajax')) {
    //            var url = window.basePath + 'Chat/GetAvailableUsersPartial/';
    //            $.get(url, { userName: element.text() }, function (htmlContent) {
    //                alert(htmlContent)
    //                element.tooltipster('content', htmlContent).data('ajax', true);
    //            });
    //        }
    //    }
    //});

    function showUserTooltip() {
        $('.chat-user').tooltip('show');
    }


    // --------------------- HELPER FUNCTIONS ---------------------- START
    function smoothScroll($container, $scrollingEntry) {
        var lineHeightInPixels = 20;
        var marginSize = 10;
        var linesVisible = ($container.height() / lineHeightInPixels).toFixed(0);
        var linesTotal = (($container[0].scrollHeight - marginSize) / lineHeightInPixels).toFixed(0);

        // Get number of oldest message lines to fade out based on line height and scroll position.
        var linesToFadeUpper = ($container.scrollTop() / lineHeightInPixels).toFixed(0);
        console.log(linesTotal, linesVisible, linesToFadeUpper);
        // Fade upper lines out.
        $scrollingEntry.slice(0, linesToFadeUpper).fadeTo(0, 0.01);
        // Fade visible lines in.
        $scrollingEntry.slice(linesToFadeUpper, linesToFadeUpper + linesVisible).fadeTo(0, 1);
        // Fade lower lines out.
        $scrollingEntry.slice(linesToFadeUpper + linesVisible, linesTotal).fadeTo(0, 0.01);
    }

    function toggleTabBar(show) {
        if (show) {
            $('.chat-room-users-container').css({ 'border-top-right-radius': 0 });
            $('.chat-room-messages-container').css({ 'border-top-left-radius': 0 });
            $('.chat-room-tab').slideDown();
            $('.chat').animate({ 'margin-top': '5px' }, 400);
        } else {
            $('.chat-room-users-container').animate({ 'border-top-right-radius': '4px' }, 400);
            $('.chat-room-messages-container').animate({ 'border-top-left-radius': '4px' }, 400);
            $('.chat-room-tab').slideUp();
            $('.chat').animate({ 'margin-top': '-31px' }, 400);
        }
    }

    function matchNewChatMessage(value) {
        return $chat.newMessage.val().split(' ')[0].toLowerCase() == value;
    }

    function selectExiststingRoom(selectedUser) {
        var exisitingRoom = _.find($chat.roomTabs, function (element) {
            return element.textContent.trim().split(' ')[0] == selectedUser;
        });

        if (exisitingRoom) {
            $('#' + exisitingRoom.id).trigger('click');
            return true;
        }
        return false;
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

    // Html-encode messages for display in the page.
    function htmlEncode(value) {
        var encodedValue = $('<div />').text(value).html();
        return encodedValue;
    }
    // --------------------- HELPER FUNCTIONS ---------------------- END
});