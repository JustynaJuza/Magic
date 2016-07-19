/**
 * Fetch data from url to populate the selectlist at elementId.
 * @param {object} settings - settings to initialize the component with, include listType, label, valueField, textField, url, elementId, currentSelection, loadingElementId, callback(urlRequestData), onChange(event, this, urlRequestData);
 * @param {string} settings.listType - selectlist mode, either 'single' or 'multiple'
 * @param {string} settings.label - optional label as first item
 * @param {boolean} settings.disableLabel - set to true if the label item should be disabled for selection
 * @param {string} settings.url - url from which selectlist data is fetched
 * @param {string} settings.elementId - element on which to initialize the selectlist
 * @param {string} settings.loadingElementId - optional element to display while data is fetched and selectlist is loading
 * @param {string|string[]} settings.currentSelection - element(s) already selected, multiple items should be passed in an array
 * @param {string} settings.valueField - property used for value selection from fetched data, defaut 'Id'
 * @param {string} settings.textField - property used for display text selection from fetched data, defaut 'Text'
 * @callback settings.onChange - subscribe to selection onChange event
 * @callback settings.callback - function to call once selectlist is initialized with data
 */
var SelectList = function (settings) {
    var listType = settings.listType.toLowerCase();
    var label = settings.label;
    var isLabelDisabled = settings.isLabelDisabled || false;
    var url = settings.url;
    var elementId = settings.elementId;
    var loadingElementId = settings.loadingElementId;
    var currentSelection = settings.currentSelection;
    var valueField = settings.valueField || 'Id';
    var textField = settings.textField || 'Text';
    var callback = settings.callback;
    var onChange = settings.onChange;

    var type = {
        multiple: 'multiple',
        single: 'single'
    }

    var createOptionItemForList; // function
    var setCurrentlySelectedItems; // function
    var $selectList;
    var urlRequestData;

    if (listType === type.multiple) {
        $selectList = $('#multiple-list-' + elementId);

        createOptionItemForList = function (item, props) {
            var optionItem = $('<li />').text(item.text).data('value', item.key);
            if (props) {
                optionItem.prop(props);
            }
            return optionItem;
        }

        setCurrentlySelectedItems = function () {
            $selectList.children().filter(function () {
                var itemValue = $(this).data('value');
                return currentSelection.indexOf(itemValue) > -1;
            }).addClass('selected');
        }
    } else {
        $selectList = $('select#' + elementId);

        createOptionItemForList = function (item, props) {
            var optionItem = $('<option />').val(item.key).text(item.text);
            if (props) {
                optionItem.prop(props);
            }
            return optionItem;
        }

        setCurrentlySelectedItems = function () {
            if (currentSelection.length && $selectList.children('option[value=' + currentSelection + ']').length) {
                $selectList.val(currentSelection);
            }
            else if (label) {
                $selectList.children().first().prop('selected', true);
            }
        }
    }

    var createOptionItem = function (data, props) {
        var item = {
            key: data[valueField],
            text: data[textField]
        }
        return createOptionItemForList(item, props);
    }

    function appendToSelectList(data) {
        var items = data.map(createOptionItem);
        if (label) {
            var labelItem = {};
            labelItem[valueField] = '$label';
            labelItem[textField] = label;
            items.splice(0, 0, createOptionItem(labelItem, { 'disabled': isLabelDisabled === true }));
        }

        $selectList.html(items);
    }

    function setSelectListData(data) {
        urlRequestData = data;
        appendToSelectList(data);
        setCurrentlySelectedItems();
    }

    function getSelectListData() {
        return $.get(url);
    }

    function disableSelectList() {
        if (loadingElementId) {
            var $loadingElement = $('#' + loadingElementId);
            $loadingElement.addClass('loading');
            $loadingElement.css({ 'pointer-events': 'none' });
        } else {
            $selectList.addClass('loading');
            $selectList.css({ 'pointer-events': 'none' });
        }
    }

    function enableSelectList() {
        if (loadingElementId) {
            var $loadingElement = $('#' + loadingElementId);
            $loadingElement.removeClass('loading');
            $loadingElement.css({ 'pointer-events': 'auto' });
        } else {
            $selectList.removeClass('loading');
            $selectList.css({ 'pointer-events': 'auto' });
        }
    }

    function formatCurrentSelection() {
        currentSelection = currentSelection && currentSelection.length > 0
            ? currentSelection instanceof Array
                ? currentSelection
                : typeof currentSelection === 'string'
                    ? currentSelection.split(',')
                    : []
            : [];
    }

    function throwError(xhr) {
        if (xhr.status !== 0 && xhr.statusText !== 'abort') {
            var errorMessage = 'An error occured while processing your request: ' + xhr.statusText;

            var errorDetails = $(xhr.responseText).find('h2 i').html();
            if (errorDetails) {
                errorMessage += '\nMessage: ' + errorDetails;
            }
            throw Error(errorMessage);
        }
    }

    if (onChange) {
        $(document).on('change', '#' + elementId, function (event) {
            onChange(event, $selectList, urlRequestData);
        });
    }

    disableSelectList();
    formatCurrentSelection();
    return getSelectListData()
        .done(setSelectListData)
        .fail(throwError)
        .always(enableSelectList)
        .then(function () {
            if (callback) {
                callback($selectList, urlRequestData);
            }

            return urlRequestData;
        });
};