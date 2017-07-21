(function (chat, $, undefined) {

    chat.initialize = function initializeChat() {

        chat.registerEventHandlers();
        chat.adjustRoomTabs();
        chat.roomTabs.first().trigger('click');
        chat.display.scrollContainerToBottom('#room-messages-container-default');
    };

}(window.chat = window.chat || {}, jQuery));

    //React.renderComponent(<Chat />, document.body);
    //React.render('<Chat />', document.getElementById('chat-container'));
    //ReactDOM.render(React.createElement(Chat, {"isAuthenticated":false,"userName":""}), document.getElementById("chat-container"))