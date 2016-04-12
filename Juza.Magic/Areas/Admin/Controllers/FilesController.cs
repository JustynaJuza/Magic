using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Juza.Magic.Areas.Admin.Controllers
{
    public class FilesController : Controller
    {
        private readonly IFileHandler _fileHandler;

        public FilesController(IFileHandler fileHandler)
        {
            _fileHandler = fileHandler;
        }

        [HttpPost]
        public async Task<string> UploadFile(HttpPostedFileBase file, string uploadPath = "", bool allowImageOnly = true)
        {
            if (allowImageOnly)
            {
                if (!_fileHandler.PassImageFileTypeConstraint(file.ContentType, allowImageOnly: true))
                    return "This must be an image file";

                if (!_fileHandler.PassImageSizeConstraint(file.InputStream, width: 600, height: 200))
                    return "Your image file must be of the maximum size of 600px x 200px";

                uploadPath = "/Images" + uploadPath;
            }

            var fileName = Path.GetFileName(file.FileName);

            string path;
            try
            {
                path = _fileHandler.GetAppRelativeFilePath(fileName, uploadPath);
            }
            catch (PathTooLongException)
            {
                return "The file or directory name is too long";
            }

            var success = await _fileHandler.SaveFileAsync(file.InputStream, fileName, uploadPath);

            return success ? path : "File saving failed due to a disk access restriction";
        }
    }
}