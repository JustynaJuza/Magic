MVC-ImageEditorTemplate-JSFileUploader Package

This package will display your string properties as images and add 'Upload'/'Delete' buttons. 
'Upload' will show a JavaScript upload control on click.

The default upload path is set in Controllers/FilesController.cs as "~/Content/Images".
The default place holder image path is set in Helpers/UploaderHelpers.cs as "~/Content/Images/placeholder.png".

Feel free to inspect /Files/Example in your browser and delete the example view in Views/Files/Example/ and corresponding Example() method in Controllers/FilesController.cs.

USAGE
To enable the functionality in your view:
- render "_UploaderPartial" (with optional "uploadPath" ViewData dictionary entry)
- use the "ImageUpload" editor template for your model's string properties intended for image paths
