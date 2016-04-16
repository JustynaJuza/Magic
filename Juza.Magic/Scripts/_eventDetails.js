(function (eventDetails, $, undefined) {

    function scrollToTop() {
        $('html, body').animate({ scrollTop: 0 }, 0);
    }

    eventDetails.id = null;
    eventDetails.url = null;
    eventDetails.status = null;
    eventDetails.eventStatusAllowingChanges = null;
    eventDetails.attendeeStatusConfirmingBooking = null;
    eventDetails.capacity = 0;
    eventDetails.clientsBooked = 0;
    eventDetails.clientsRequestingBooking = 0;
    eventDetails.isPastScheduledDate = false;
    eventDetails.defaultView = '#edit';
    eventDetails.defaultViewOnDelivery = '#attendees';
    eventDetails.defaultViewOnCancel = '#edit';

    // set up request manager
    var activeRequests = [];
    eventDetails.ajaxManager = {
        addRequest: function (promise) {
            activeRequests.push(promise);
        },
        cancelAllRequests: function () {
            _.each(activeRequests, function (request, index) {
                eventDetails.ajaxManager.cancelRequest(request, index);
            });
        },
        cancelRequest: function (request, index) {
            request.abort();
            eventDetails.ajaxManager.removeRequest(request, index);
        },
        removeRequest: function (request, index) {
            if (!index) {
                index = activeRequests.indexOf(request);
            }

            if (index !== -1) {
                activeRequests.splice(index, 1);
            }
        }
    };

    eventDetails.checkIfChangesAllowed = function () {
        return eventDetails.status === eventDetails.eventStatusAllowingChanges;
    }

    eventDetails.checkIfHasAttendees = function () {
        return eventDetails.clientsBooked !== 0 || eventDetails.clientsRequestingBooking !== 0;
    }

    eventDetails.updateIfEventFull = function () {
        if (eventDetails.capacity <= eventDetails.clientsBooked) {
            $('#event-booking-full').show();
            $('#add-attendee-btn').hide();
            $('.update-attendee-submit[data-status="' + eventDetails.attendeeStatusConfirmingBooking + '"]').prop('disabled', true);
        }
        else {
            $('#event-booking-full').hide();
            $('#add-attendee-btn').show();
            $('.update-attendee-submit[data-status="' + eventDetails.attendeeStatusConfirmingBooking + '"]').prop('disabled', false);
        }
    }

    eventDetails.setTabMessage = function (message) {
        $('#tab-message').html(message);
    }

    eventDetails.urlReplaceForCurrentEventId = function (url, eventIdPlaceholder) {
        return url.replace(eventIdPlaceholder, eventDetails.id);
    }

    eventDetails.onUpdateAttendee = function (bookingCountChange, requestCountChange) {
        eventDetails.clientsBooked += bookingCountChange;
        eventDetails.clientsRequestingBooking += requestCountChange;

        eventDetails.updateIfEventFull();
        eventDetails.updateBookingOverview();
    }

    eventDetails.onUpdateEvent = function (isCreate, eventId, capacity, eventOverviewPartial, newStatus) {

        function updateUrlOnCreate() {
            var eventUrl = eventDetails.url.replace('currentEventId', eventDetails.id) + window.location.hash;
            window.history.replaceState({}, $('title').text(), eventUrl);
        }

        function enableAllTabs() {
            $('.nav-tab').removeClass('disabled');
        }

        if (isCreate) {
            eventDetails.id = eventId;
            eventDetails.status = newStatus;
            updateUrlOnCreate();
            enableAllTabs();
        }

        // update capacity used on attendee booking tab
        eventDetails.capacity = capacity;

        $('#event-overview').html(eventOverviewPartial);
        scrollToTop();
    }

    eventDetails.onDeliverEvent = function (newStatus) {
        eventDetails.setStatus(newStatus);
        $('.nav-tab:not(#edit-tab):not(#attendees-tab)').addClass('disabled');
        window.location.hash = eventDetails.defaultViewOnDelivery;
    }

    eventDetails.onCancelEvent = function (newStatus) {
        eventDetails.setStatus(newStatus);
        $('.nav-tab:not(#edit-tab):not(#attendees-tab)').addClass('disabled');
        window.location.hash = eventDetails.defaultViewOnCancel;
    }


    eventDetails.setStatus = function (status) {
        eventDetails.status = status;
        $('#event-status').text(status);
        $('#event-status').prop('class', 'event-status event-status-' + status);
    }

    eventDetails.disableEditForStatus = function (status) {
        if (eventDetails.status === status) {
            $('input, select, textarea').prop('disabled', true);
        }
    }

    eventDetails.updateBookingOverview = function () {
        $('#event-bookings-capacity').text(eventDetails.capacity);
        $('#event-bookings-count').text(eventDetails.clientsBooked);
        $('#event-requests-count').text(eventDetails.clientsRequestingBooking);
    }

    eventDetails.errorSelector = null;
    eventDetails.onError = function (xhr) {
        if (xhr.status !== 0 && xhr.statusText !== 'abort') {
            var error = $('<p>An error occured while processing your request: </p>')
                .addClass('red message')
                .prop('id', 'error-message-display')
                .append(xhr.statusText);

            var errorMessage = $(xhr.responseText).find('h2 i').html();
            if (errorMessage) {
                error.append("<br/>Message: " + errorMessage);
            }

            if (!eventDetails.errorSelector) {
                throw Error('The request failed, but there is no element defined to display the error in.' +
                    '\nAssign eventDetails.errorSelector to displasy the error on the page. ' +
                    '\nError details:\n' + xhr.statusText + ' ' + errorMessage, xhr)
            } else {
                $(eventDetails.errorSelector).html(error);
                scrollToTop();
            }
        }
    }

}(window.eventDetails = window.eventDetails || {}, jQuery));