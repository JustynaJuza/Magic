if (!String.format) {
    String.format = function (format) {
        var args = Array.prototype.slice.call(arguments, 1);
        return format.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
              ? args[number]
              : match
            ;
        });
    };
}

Array.prototype.equals = function (array) {
    if (!array)
        return false;

    if (this.length != array.length)
        return false;

    for (var i = 0, l = this.length; i < l; i++) {
        // Check if we have nested arrays
        if (this[i] instanceof Array && array[i] instanceof Array) {
            if (!this[i].equals(array[i]))
                return false;
        }
        else if (this[i] != array[i]) {
            // Warning - two different object instances will never be equal: {x:20} != {x:20}
            return false;
        }
    }
    return true;
}


(function (helpers, $, undefined) {

    helpers.htmlEncode = function (value) {
        return (value ? jQuery('<div />').text(value).html() : '');
    }

    helpers.htmlDecode = function (value) {
        return (value ? $('<div />').html(value).text() : '');
    }


    helpers.moveToScroll = function (element) {
        var top = $(window).scrollTop() - $(element).offset().top;
        //var left = 0.5 * $(document).outerWidth() - offset.left;
        $(element).css({ 'top': top + 'px' }); //, {'left': left + 'px'});
    }

    helpers.attachToThis = function (element, otherElement) {
        var box = $(element)[0].getBoundingClientRect();
        var left = box.left;
        //var left = 0.5 * $(document).outerWidth() - offset.left;
        var width = $(otherElement).outerWidth();
        var x = left - 0.5 * width;
        $(otherElement).css({ 'left': x + 'px' }); //, {'left': left + 'px'});
    }

    helpers.animateScrollLeft = function (element) {
        var overflow = element.scrollWidth - element.offsetWidth + 2;
        $(element).animate({ scrollLeft: overflow }, 1625);
    }

    helpers.animateScrollRight = function (element) {
        $(element).animate({ scrollLeft: 0 }, 1625);
    }

    helpers.blink = function (element) {
        $(element).fadeTo(500, 0.75);
        $(element).fadeTo(500, 1);
    }

    //$(document).popover({
    //    selector: '.chat-user',
    //    container: '.chat',
    //    trigger: 'manual',
    //    html: true,
    //    content: function () {
    //        var url = window.basePath + 'Chat/GetUserProfileTooltipPartial/';
    //        jQuery.ajaxSetup({
    //            async: false,
    //            traditional: true
    //        });
    //        $.get(url, { userName: $(this).text() }, function (htmlContent) {
    //            return htmlContent;
    //        });
    //    },
    //    title: function () {
    //        return 'Test';
    //    },
    //    placement: function (tip, element) {
    //        var offset = $(element).offset();
    //        height = $(document).outerHeight();
    //        width = $(document).outerWidth();
    //        vert = 0.5 * height - offset.top;
    //        vertPlacement = vert > 0 ? 'bottom' : 'top';
    //        horiz = 0.5 * width - offset.left;
    //        horizPlacement = horiz > 0 ? 'right' : 'left';
    //        placement = Math.abs(horiz) > Math.abs(vert) ? horizPlacement : vertPlacement;
    //        return placement;
    //    }
    //});

}(window.helpers = window.helpers || {}, jQuery));