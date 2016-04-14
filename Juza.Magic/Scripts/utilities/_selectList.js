/**
 * Fetch data from url to populate the selectlist at elementId.
 * @param {object} settings Parameters: listType, label, valueField, textField, url, elementId, currentSelection, loadingElementId, callback(urlRequestData), onChange(event, this, urlRequestData);
 */
var SelectList = function (settings) {
    var listType = settings.listType;
    var label = settings.label;
    var disableLabel = settings.disableLabel || false;
    var url = settings.url;
    var elementId = settings.elementId;
    var currentSelection = settings.currentSelection;
    var loadingElementId = settings.loadingElementId;
    var callback = settings.callback;
    var valueField = settings.valueField || "Id";
    var textField = settings.textField || "Text";
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
            var optionItem = $('<li />').text(item.text).data('value', item.key.toString());
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
            items.splice(0, 0, createOptionItem(labelItem, { 'disabled': disableLabel === true }));
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

    function formatcurrentSelection() {
        currentSelection = currentSelection && currentSelection.length > 0
            ? currentSelection.isArray
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
    formatcurrentSelection();
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