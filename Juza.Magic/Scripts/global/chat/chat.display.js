(function (chat, $, undefined) {

    (function(display, $) {

        //function smoothScroll($container, $scrollingEntry) {
        //    var lineHeightInPixels = 20;
        //    var marginSize = 10;
        //    var linesVisible = ($container.height() / lineHeightInPixels).toFixed(0);
        //    var linesTotal = (($container[0].scrollHeight - marginSize) / lineHeightInPixels).toFixed(0);

        //    // Get number of oldest message lines to fade out based on line height and scroll position.
        //    var linesToFadeUpper = ($container.scrollTop() / lineHeightInPixels).toFixed(0);
        //    console.log(linesTotal, linesVisible, linesToFadeUpper);
        //    // Fade upper lines out.
        //    $scrollingEntry.slice(0, linesToFadeUpper).fadeTo(0, 0.01);
        //    $scrollingEntry.slice(linesToFadeUpper, linesToFadeUpper + linesVisible).fadeTo(0, 1);
        //    // Fade lower lines out.
        //    $scrollingEntry.slice(linesToFadeUpper + linesVisible, linesTotal).fadeTo(0, 0.01);
        //}

        display.toggleTabBar = function (on) {
            if (on) {
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
        };

        display.toggleAvailableUsers = function (on) {
            if (on) {
                $('#available-users-overlay').fadeIn();
                $('#available-users').removeClass('zero-size', 500);
            } else {
                $('#available-users-overlay').fadeOut();
                $('#available-users').addClass('zero-size', 500);
                $('.available-chat-user').removeClass('toggle');
            }
        };

        display.toggleUserTooltip = function (user) {
            if (!user) {
                return chat.userTooltip.hide();
            }

            if (chat.userTooltip.data('user') !== user) {
                // Get tooltip data.
                var url = window.basePath + 'Chat/GetUserProfileTooltipPartial/';
                $.get(url, { userName: user }, function(htmlContent) {
                    chat.userTooltip.html(htmlContent);
                    chat.userTooltip.data('user', user);
                });
                chat.userTooltip.show();
            } else if (chat.userTooltip.data('user') === user && chat.userTooltip.is(':hidden')) {
                chat.userTooltip.show();
            } else {
                chat.userTooltip.hide();
            }
        };

        display.scrollContainerToBottom = function (element) {
            $(element).animate({ scrollTop: $(element)[0].scrollHeight }, 1000);
        };

    }(chat.display = chat.display || {}, $));

}(window.chat = window.chat || {}, jQuery));