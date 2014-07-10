@model string
@using $rootnamespace$.Helpers

<style>
    .upload {
        position: relative;
        display: inline-block;
        min-width: 200px;
        min-height: 34px;
    }
    .upload-controls {
        position: absolute;
        top: 0px;
        left: 0px;
        width: auto;
        height: auto;
        align-content: center;
    }

    .upload-controls .btn {
        border-top-left-radius: 0;
        border-top-right-radius: 0;
    }
</style>

<div class="upload image-upload">
    <img src="@(String.IsNullOrWhiteSpace(Model) ? Html.GetPlaceholder() : Url.Content(Model))" alt="" id="img-@Html.GetIdFor(m => m)" />

    @Html.HiddenFor(m => m)
    <div class="upload-controls">
        <input type="button" value="Upload" class="btn btn-warning btn-upload" id="up-@Html.GetIdFor(m => m)" />
        <input type="button" value="Delete" class="btn btn-danger btn-upload-delete" id="del-@Html.GetIdFor(m=> m)" @(String.IsNullOrWhiteSpace(Model) ? "style=display:none;" : "") />
    </div>
</div>
