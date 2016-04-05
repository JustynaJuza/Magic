/**
 * Change content when tab is clicked by fetching data witch getContent function.
 * @param {function} getContent Function used to get a promise of html content. Should be a switch statement on the location hash.
 * @param {object} settings Parameters: tabContentId, tabContentSpinner, callback
 */
var HashNavigation = function (getContent, defaultView, settings) {
    settings = settings || {};
    var $content = $('#' + (settings.tabContentId || 'tab-content'));
    var $spinner = $('#' + (settings.tabContentSpinner || 'tab-content-spinner'));
    var callback = settings.callback;

    if (!window.location.hash.length) {
        window.location.hash = defaultView;
    }

    var currentHash = window.location.hash; // stores information of current hash to know where to return in case future request fails.
    
    function renderContent() {
        var hash = window.location.hash;

        function isActiveTab(name) {
            $(name + '-tab').hasClass('active');
        }

        function clearActiveTabs() {
            $('.tabs li').removeClass('active');
        }

        function setActiveTab(name) {
            clearActiveTabs();
            $(name + '-tab').addClass('active');
        }

        function showLoading() {
            $content.hide();
            $spinner.show();
        }

        function hideLoading() {
            $spinner.hide();
            $content.show();
        }

        function handleFailedRequest(xhr) {

            function rewriteHistory(hash) {
                var url = window.location.href.split('#')[0] + hash;
                window.history.replaceState({}, $('title').text(), url);
            }

            if (xhr.status !== 0 && xhr.statusText !== 'abort') {
                alert("The link produced an error.\n" +
                    "Please report this to the IT development team along with the details about which link you clicked and the current page.\n" +
                    "Current page as in browser url: " + window.location);

                clearActiveTabs();
                setActiveTab(currentHash);
                rewriteHistory(currentHash);
                window.history.back();
            }
        }

        function appendContent(html) {
            
            function resetValidation() {
                $content.removeData("validator");
                $content.removeData("unobtrusiveValidation");
                $.validator.unobtrusive.parse($content);
            }

            $content.html(html);
            resetValidation();
            currentHash = hash;
        }

        if (!isActiveTab(hash)) {
            setActiveTab(hash);
            showLoading();
            return getContent()
                .done(appendContent)
                .fail(handleFailedRequest)
                .always(hideLoading);
        }
    }

    $('.nav-tabs a').click(function (e) {
        if (window.location.hash === $(this).prop('hash')) {
            e.preventDefault();
        }
    });

    var isNavigating = false;
    var rendering = null;
    function processHashChange() {
        if (isNavigating) {
            rendering.abort();
        }

        isNavigating = true;
        rendering = renderContent();
        if (callback) {
            rendering.done(callback);
        }

        rendering.always(function () {
            isNavigating = false;
        });
    }

    window.onhashchange = processHashChange;
    return processHashChange();
}