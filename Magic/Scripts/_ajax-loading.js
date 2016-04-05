function isNullOrUndefined(variable) {
    return variable == null || variable == 'undefined';
}

function AjaxRequestHandler(elementIds) {
    var loadingId = elementIds.loading;
    var successId = elementIds.success;
    var errorId = elementIds.error;
    var submitId = elementIds.submit;

    return {
        onLoading: function (id) {
            id = id || '';

            $('#' + submitId + id).prop('disabled', true);
            $('#' + successId + id).addClass('hidden');
            $('#' + errorId + id).addClass('hidden');
            $('#' + loadingId + id).removeClass('hidden');
        },

        onComplete: function (xhr, id, isSuccess) {
            id = id || '';

            $('#' + submitId + id).prop('disabled', false);
            $('#' + loadingId + id).addClass('hidden');

            if (isSuccess) {
                $('#' + successId + id).removeClass('hidden');
            }
            else {
                if (xhr && xhr.status == '200' && xhr.responseText) {
                    var result = JSON.parse(xhr.responseText);
                    console.log(result);
                    if (result.Success == true) {
                        $('#' + successId + id).removeClass('hidden');
                    } else {
                        $('#' + errorId + id).removeClass('hidden');
                    }
                } else {
                    $('#' + errorId + id).removeClass('hidden');
                }
            }

        }
    }
}