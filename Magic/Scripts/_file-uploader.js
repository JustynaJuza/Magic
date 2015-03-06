$(function () {
    var selectedFileId = 'uploader-file';
    var $selectedFile = $('#' + selectedFileId);
    var $filePropertyField = $('#uploader-property-id');
    var $fileOverlay = $('#uploader-file-overlay');
    var $uploaderProgressBar = $('#uploader-progress-bar');
    var $closeButton = $('.dialog-close-btn');

    if (!window.File || !window.FileReader || !window.FileList || !window.Blob) {
        alert('The file upload functionality is not fully supported in your browser version, you are advised to update your browser to a newer version.');
        return;
    }

    var uploader = document.getElementById('uploader');
    uploader.ondragover = function () { return false; };
    uploader.ondragend = function () { return false; };
    uploader.ondragenter = function () {
        $fileOverlay.addClass('hover');
        this.classList.add('hover');
    };
    uploader.ondragleave = function () {
        $fileOverlay.removeClass('hover');
        this.classList.remove('hover');
    };
    uploader.ondrop = function (e) {
        e.preventDefault();
        var file = e.dataTransfer.files[0];
        readyToUpload(file);
    }

    $(document).on('click', '.btn-upload', showFileUploader);
    $(document).on('click', '.btn-upload-delete', deleteImage);
    $(document).on('click', '.upload img', toggleControls)

    $closeButton.click(closeFileUploader);

    $('#file-uploader-upload-btn').click(function () {
        
    });

    $selectedFile.change(function () {
        var file = document.getElementById(selectedFileId).files[0];
        readyToUpload(file);
    });

    $fileOverlay.click(function () {
        document.getElementById(selectedFileId).click();
    });

    function showFileUploader() {
        $filePropertyField.val($(this).prop('id').substr(3));

        moveToScroll('#uploader');
        $('.overlay').show();
        $('#uploader').show();
    }

    function clearFileUploader() {
        $filePropertyField.val('');
        $selectedFile.val('');
        $fileOverlay.text('');
        $fileOverlay.addClass('uploader-default')
        $fileOverlay.removeClass('disabled');
    }

    function closeFileUploader() {
        $('.overlay').hide();
        $('#uploader').hide();
        clearFileUploader();
    }

    function toggleControls() {
        $(this).siblings('.upload-controls').toggle();
    }

    function readyToUpload(file) {
        var xhr = new XMLHttpRequest();
        xhr.onreadystatechange = function () {
            if (xhr.readyState != 4) {
                return;
            }

            if (xhr.status == 200) {
                var overwrite = confirm('Do you want to overwrite the existing file?');
                if (!overwrite) {
                    return clearFileUploader();
                }
            }
            sendFile(file);
            console.log(xhr)
        };

        xhr.open('HEAD', window.basePath + '/Content'/*/Images'*/ + uploadPath + '/' + file.name, false);
        return xhr.send();
    }

    function sendFile(file) {
        var uri = window.basePath + '/Admin/Files/UploadFile';

        var fd = new FormData();
        fd.append('file', file);
        fd.append('uploadPath', uploadPath);
        fd.append('allowImageOnly', false);

        var xhr = new XMLHttpRequest();

        xhr.upload.onprogress = function (e) {
            if (e.lengthComputable) {
                var percentComplete = Math.floor(e.loaded / e.total) * 100;
                $fileOverlay.text('Uploading: ' + percentComplete + '%');
            }
        }

        xhr.onreadystatechange = function () {
            if (xhr.readyState != 4) {
                return;
            }

            if (xhr.readyState == 4 && xhr.status == 200) {
                // Successfully uploaded?
                if (xhr.responseText[0] == '/') {
                    $fileOverlay.text('Upload finished');
                    // Call returned path to new image.
                    updateImage(xhr.responseText);
                    //closeFileUploader();
                } else {
                    // Call returned message.

                    $fileOverlay.text(xhr.responseText);
                }
            }
            else if (xhr.readyState == 4 && xhr.status == 500 && xhr.responseText.match('Maximum request length exceeded')) {
                //alert('The file exceeds the allowed size limit.')
                $fileOverlay.text('The file exceeds the allowed size limit');
                //clearFileUploader();
            }
            else if (xhr.readyState == 4) {
                $fileOverlay.text('There was an unexpected error while uploading this file, it may be a server issue or the file overwritten is currently in use');
                //alert('There was an unexpected error while uploading this file, it may be a server issue or the file overwritten is currently in use.')
                clearFileUploader();
            }
        };

        // Initiate a multipart/form-data upload
        $fileOverlay.removeClass('uploader-default')
        $fileOverlay.text('Uploading: 0%');
        xhr.open('POST', uri, true);
        xhr.send(fd);
    }

    function updateImage(image) {
        var targetPropertyId = $filePropertyField.val();
        console.log(targetPropertyId)
        $('#' + targetPropertyId).val(image);
        $('#img-' + targetPropertyId).prop('src', window.basePath + image);
        $('#del-' + targetPropertyId).show();
    }

    function deleteImage() {
        var targetPropertyId = $(this).prop('id').substr(4);
        $('#img-' + targetPropertyId).prop('src', placeholderImage);
        $('#' + targetPropertyId).val('');
        $(this).hide();
    }

    function moveToScroll(element) {
        var top = $(window).scrollTop();
        $(element).css('top', top + 200 + 'px');
    }

    //$(window).resize(function () {
    //    var width = $('#uploader .dialog-content').width();
    //    if (width < 225) {
    //        $fileOverlay.text('Select file');
    //        $('#uploader .dialog-name').text('Uploader');
    //    } else {
    //        $fileOverlay.text(fileOverlayText);
    //        $('#uploader .dialog-name').text('File uploader');
    //    }
    //});

    window.admin.client.updateUploadProgress = function (percent) {
        var width = $uploaderProgressBar.width() / $uploaderProgressBar.parent().width() * 100;
        if (width < percent) {
            $uploaderProgressBar.css('width', percent + '%');
        }
    }
});