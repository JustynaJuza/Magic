$(function () {
    if (!window.File || !window.FileReader || !window.FileList || !window.Blob) {
        alert('The file upload is not fully supported in your browser version, please update to a newer version.');
        return;
    }

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

    function closeFileUploader() {
        $('#file-property-id').val('');
        $('#file-uploader-selected-file-overlay').val('Select or drag and drop file');
        $('#file-uploader-selected-file').val('');
        $('#file-uploader-overlay').hide();
        $('#file-uploader').hide();
    }

    //function hideControls() {
    //    $(this).parent().hide();
    //}

    function toggleControls() {
        $(this).siblings('.upload-controls').toggle();
    }

    function readyToUpload(file) {
        var xhr = new XMLHttpRequest();
        xhr.open('HEAD', basePath + 'Content/Images' + uploadPath + '/' + file.name, false);
        xhr.onreadystatechange = function () {
            if (xhr.status == 200) {
                var overwrite = confirm('Do you want to overwrite the existing file?');
                if (overwrite) {
                    sendFile(file);
                }
            }
            else {
                sendFile(file);
            }
            console.log(xhr)
        };
        return xhr.send();
    }

    function sendFile(file) {
        var uri = basePath + 'Files/UploadFile';

        var fd = new FormData();
        fd.append('file', file);
        fd.append('uploadPath', uploadPath);
        fd.append('allowImageOnly', true);

        var xhr = new XMLHttpRequest();
        xhr.open("POST", uri, true);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                // Successfully uploaded?
                if (xhr.responseText[0] == '/') {
                    // Call returned path to new image.
                    updateImage(xhr.responseText);
                    closeFileUploader();
                } else {
                    // Call returned message.
                    alert(xhr.responseText);
                }
            }
            else if (xhr.readyState == 4 && xhr.status == 500 && xhr.responseText.match('Maximum request length exceeded.')) {
                alert('The file exceeds the allowed size limit.')
            }
            else if (xhr.readyState == 4) {
                alert('There was an unexpected error while uploading this file, please try a different file.')
            }
        };

        // Initiate a multipart/form-data upload
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