﻿(function (chat, $, undefined) {

    chat.initialize = function initializeChat() {

        chat.registerEventHandlers();
        chat.adjustRoomTabs();
        chat.roomTabs.first().trigger('click');
        chat.display.scrollContainerToBottom('#room-messages-container-default');
    };

}(window.chat = window.chat || {}, jQuery));