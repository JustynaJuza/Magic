(function (chat, $, undefined) {

    chat.userName = $('#user-name').text();

    chat.chatRoomRequestInProgress = [];
    chat.horizontalMouseOverScroll = 0;
    chat.initialized = false;

    chat.header = $('#chat-header-bar');

    chat.container = $('.chat');
    chat.roomsContainer = $('#chat-rooms-container');
    chat.rooms = $('.chat-room');
    chat.roomTabs = $('.chat-room-tab');
    chat.roomTabNames = $('.chat-room-tab-name');
    chat.roomContents = $('.chat-room-content');

    chat.roomSelection = $('#chat-room-selection'),

    chat.messagesContainers = $('.chat-room-messages-container');
    chat.usersContainers = $('.chat-room-users-container');
    chat.userTooltip = $('#user-profile-tooltip-container');

    chat.newMessage = $('#chat-message-new');
    chat.sendButton = $('#chat-message-send-btn'),

    chat.commands = ['/msg', '/game', '/all'];
    chat.command_all = function () {
        chat.newMessage.val('');
    };

}(window.chat = window.chat || {}, jQuery));