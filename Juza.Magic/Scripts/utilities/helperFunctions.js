window.initAutocompleteForValue = function (data, autocompleteFieldId, valueFieldId, autocompletePropName, valuePropName, autocompleteOptionsOverride) {
    
    // predefined default options
    var autocompleteDefaultOptions = {
        minLength: 0,
        source: function (request, response) {

            //var matcher = new RegExp('^' + $.ui.autocomplete.escapeRegex(request.term), 'i');
            var matcher = new RegExp('\\b' + $.ui.autocomplete.escapeRegex(request.term) + '\\s*', 'gi');
            response(data.filter(function (item) {
                return matcher.test(item[autocompletePropName]);
            }));
        },
        select: function (event, ui, isReset) {
            var $autocompleteField = $('#' + autocompleteFieldId);
            var $valueField = $('#' + valueFieldId);
            $autocompleteField.val(isReset ? '' : ui.item[autocompletePropName]);
            $valueField.val(isReset ? '' : ui.item[valuePropName]);

            $autocompleteField.removeClass('input-validation-error');
            $valueField.removeClass('input-validation-error');
            $('span[data-valmsg-for="' + autocompleteFieldId + '"]').hide();
            $('span[data-valmsg-for="' + valueFieldId + '"]').hide();
            return false;
        }
    }
    var renderItemDefault = function (ul, item) {
        return $('<li></li>')
            .data('ui-autocomplete-item', item)
            .append('<a>' + item[autocompletePropName] + '</a>')
            .appendTo(ul);
    }

    // replace defaults with passed in option overrides
    var autocompleteOptions = {};
    autocompleteOptions.minLength = autocompleteOptionsOverride && autocompleteOptionsOverride.minLength
        ? autocompleteOptionsOverride.minLength
        : autocompleteDefaultOptions.minLength;
    autocompleteOptions.source = autocompleteOptionsOverride && autocompleteOptionsOverride.source
        ? autocompleteOptionsOverride.source
        : autocompleteDefaultOptions.source;
    autocompleteOptions.select = autocompleteOptionsOverride && autocompleteOptionsOverride.select
        ? autocompleteOptionsOverride.select
        : autocompleteDefaultOptions.select;

    $('#' + autocompleteFieldId).autocomplete(autocompleteOptions)
        // customize renderItem if override provided
        .autocomplete('instance')._renderItem = autocompleteOptionsOverride && autocompleteOptionsOverride._renderItem
        ? autocompleteOptionsOverride._renderItem
        : renderItemDefault;


    // check autocomplete lists for typed names
    $(document).on('change', '#' + autocompleteFieldId, function () {
        var list = data;
        var selectedAutocomplete = $(this).val();

        var foundItem = _.find(list, function (item) {
            return item[autocompletePropName] === selectedAutocomplete;
        });

        if (foundItem) {
            var ui = {};
            ui.item = foundItem;
            autocompleteOptions.select(null, ui);
        }
        else {
            autocompleteOptions.select(null, null, true);
        }
    });
};


window.disableSubmitButton = function (submitButtonId) {
    var $submitButton = $('#' + submitButtonId);
    $submitButton.prop('disabled', true);
    $submitButton.addClass('loading');
    $submitButton.val('');
}

window.enableSubmitButton = function (submitButtonId, submitButtonValue) {
    var $submitButton = $('#' + submitButtonId);
    $submitButton.prop('disabled', false);
    $submitButton.removeClass('loading');
    $submitButton.val(submitButtonValue);
}


window.setValueFromNamedItemFoundInList = function (list, itemName, fields) {
    var foundItem = _.find(list, function (item) {
        return item.Name === itemName;
    });

    for (var i = 0; i < fields.length; i++) {
        var propertyDetails = fields[i];
        var $selector = propertyDetails['selector'];
        var property = propertyDetails['property'];

        var value = foundItem ? foundItem[property] : '';
        $selector.val(value);
    }
}