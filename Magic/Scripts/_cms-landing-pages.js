    //$(document).on('keyup', '#Summary', function () { textCounter('Summary') });
    //var limit = $('#Summary').attr('data-val-length-max');
    //$('#SummaryCounter').text(limit - $('#Summary').val().length);
    
    //function textCounter(fieldName) {
    //    var fieldCounter = $('#' + fieldName + "Counter");
    //    var field = $('#' + fieldName);
    //    var counter = parseInt(fieldCounter.val());
    //    var fieldCount = field.attr('value').length;

    //    if (fieldCount > limit) {
    //        // Trim if too long.
    //        field.attr('value', field.attr('value').substring(0, limit));
    //    }
    //    fieldCounter.text(limit - field.attr('value').length);
    //}

$('.accordion').accordion({ head: '.accordion-header, header', next: 'div', heightStyle: 'content', collapsible: true, active: false, initShow: '' });

    function addHiddenInput(form, key, value) {
        // Create a hidden input element and append it to the form:
        var input = document.createElement('input');
        input.type = 'hidden';
        input.name = key;
        input.value = value;
        console.log(input);
        form.appendChild(input);
        console.log(form);
    }

    function UrlExists(url, callback) {
        $.ajax({
            url: url,
            dataType: 'text',
            type: 'GET',
            async: false,
            complete: function (xhr) {
                if (typeof callback === 'function') {
                    //var status = xhr.responseText.match('/Error/') || xhr.responseText.match(new RegExp($('#Name').val())) ? 001 : xhr.status;
                    return callback.apply(this, [xhr.status]);
                }
            },
        });
    }

    var oldShortUrl = $('#ShortUrl').val();
    var form = document.forms['CreateEditForm'];

    $('#CreateEditForm').submit(function (e) {
        if ($('#CreateEditForm').valid()) {
            var shortUrl = $('#ShortUrl').val();
            var shortUrlValid = true;

            if (shortUrl.length > 0 && oldShortUrl != $('#ShortUrl').val()) {
                UrlExists(BasePath + shortUrl, function (status) {
                    if (status === 200) {
                        // file was found
                        var confirmResult = confirm('The link ' + shortUrl + ' is already in use, if your changes get saved successfully you will overwrite the old link! Are you sure you want to do this?');
                        if (confirmResult.valueOf() == true) {
                            shortUrlValid = true;
                            addHiddenInput(form, "updateShortUrl", true);
                        }
                        else {
                            shortUrlValid = false;
                        }
                    }
                    else if (status === 404 || status === 500) {
                        // 404 not found
                        shortUrlValid = true;
                        addHiddenInput(form, "updateShortUrl", true);
                    }
                    else if (status === 001) {
                        shortUrlValid = true;
                        // No need for adding hidden input and forcing link update.
                    }
                    else {
                        shortUrlValid = false;
                    }
                });
            }

            return shortUrlValid;
        }
    });