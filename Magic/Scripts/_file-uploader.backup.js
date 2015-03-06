$(function () {
    if (!window.File || !window.FileReader || !window.FileList || !window.Blob) {
        alert('The file upload functionality is not fully supported in your browser version, you are advised to update your browser to a newer version.');
        return;
    }

    var text = $('#file-uploader-selected-file-overlay').val();
    var uploader = document.getElementById('file-uploader');
    uploader.ondragover = function () { return false; };
    uploader.ondragend = function () { return false; };
    uploader.ondragenter = function () {
        $('#file-uploader-selected-file-overlay').addClass('hover');
        this.classList.add('hover');
    };
    uploader.ondragleave = function () {
        $('#file-uploader-selected-file-overlay').removeClass('hover');
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

    $('#file-uploader-cancel-btn').click(closeFileUploader);

    $('#file-uploader-upload-btn').click(function () {
        
    });

    $('#file-uploader-selected-file').change(function () {
        var file = document.getElementById('file-uploader-selected-file').files[0];
        readyToUpload(file);
    });

    $('#file-uploader-selected-file-overlay').click(function () {
        document.getElementById('file-uploader-selected-file').click();
    });

    function showFileUploader() {
        $('#file-property-id').val($(this).prop('id').substr(3));

        moveToScroll('#file-uploader');
        $('#file-uploader-overlay').show();
        $('#file-uploader').show();
    }

    function clearFileUploader() {
        $('#file-property-id').val('');
        $('#file-uploader-selected-file').val('');
        $('#file-uploader-selected-file-overlay').val(text);
        $('#file-uploader-selected-file-overlay').removeClass('disabled');
    }

    function closeFileUploader() {
        $('#file-uploader-overlay').hide();
        $('#file-uploader').hide();
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

        xhr.upload.progress = function (e) {
            if (e.lengthComputable) {
                var percentComplete = Math.floor(e.loaded / e.total) * 100;
                $('#file-uploader-selected-file-overlay').val('Uploading: ' + percentComplete + '%');
            }
        }

        xhr.onreadystatechange = function () {
            if (xhr.readyState != 4) {
                return;
            }

            if (xhr.readyState == 4 && xhr.status == 200) {
                // Successfully uploaded?
                if (xhr.responseText[0] == '/') {
                    $('#file-uploader-selected-file-overlay').val('Uploading: 100%');
                    // Call returned path to new image.
                    updateImage(xhr.responseText);
                    closeFileUploader();
                } else {
                    // Call returned message.
                    $('#file-uploader-selected-file-overlay').val(xhr.responseText);
                }
            }
            else if (xhr.readyState == 4 && xhr.status == 500 && xhr.responseText.match('Maximum request length exceeded.')) {
                alert('The file exceeds the allowed size limit.')
                clearFileUploader();
            }
            else if (xhr.readyState == 4) {
                alert('There was an unexpected error while uploading this file, it may be a server issue or the file overwritten is currently in use.')
                clearFileUploader();
            }
        };

        // Initiate a multipart/form-data upload
        $('#file-uploader-selected-file-overlay').val('Uploading: 0%');
        xhr.open('POST', uri, true);
        xhr.send(fd);
    }

    function updateImage(image) {
        var targetPropertyId = $('#file-property-id').val();
        $('#' + targetPropertyId).val(image);
        $('#img-' + targetPropertyId).prop('src', image);
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
});