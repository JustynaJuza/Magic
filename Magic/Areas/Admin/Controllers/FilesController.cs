using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using Elmah;
using Penna.Messaging.Web.Helpers;

namespace Penna.Messaging.Web.Controllers
{
    public class FilesController : Controller
    {
        private readonly IPathProvider _pathProvider;

        public FilesController(IPathProvider pathProvider)
        {
            _pathProvider = pathProvider;
        }

        [HttpPost]
        public async Task<string> UploadFile(HttpPostedFileBase file, string uploadPath = "", bool allowImageOnly = true)
        {
            if (allowImageOnly)
            {
                var isImageFile = Regex.IsMatch(file.ContentType, "image");
                uploadPath = "/Images" + uploadPath;

                if (!isImageFile)
                {
                    return "This must be an image file";
                }

                //var image = Image.FromStream(file.InputStream, true, true);
                //if (image.Size.Width > 600 || image.Size.Height > 200)
                //{
                //    return "Your image file must be of the maximum size of 600px x 200px";
                //}
            }

            return await SaveFile(file.InputStream, file.FileName, uploadPath);
        }

        public async Task<string> SaveFile(Stream fileStream, string fileName, string uploadPath = "")
        {
            var path = "/Content" + uploadPath + "/";
            var serverPath = _pathProvider.GetServerPath("~" + path);

            PrepareFileDirectory(serverPath);
            await SaveFileInBlocksAsync(serverPath + fileName, fileStream);

            return path + fileName;
        }

        public string GetFileIconAsString(string filePath)
        {
            var serverPath = _pathProvider.GetServerPath("~" + filePath);
            var icon = Icon.ExtractAssociatedIcon(serverPath);
            var image = icon.ToBitmap();
            var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            return Convert.ToBase64String(stream.ToArray());
        }

        public int GetImageWidth(string filePath)
        {
            var serverPath = _pathProvider.GetServerPath("~" + filePath);
            try
            {
                var image = Image.FromFile(serverPath);
                return image.Size.Width;
            }
            catch(Exception ex)
            {
                ErrorLog.GetDefault(System.Web.HttpContext.Current).Log(new Error(ex));
                return 0;
            }
        }

        private void PrepareFileDirectory(string serverPath)
        {
            if (!Directory.Exists(serverPath))
            {
                Directory.CreateDirectory(serverPath);
            }
        }

        private Task SaveFileInBlocksAsync(string filePath, Stream fileStream, int blockByteSize = 1048576) // 1048576B = 1MB
        {
            var bytesTransferred = 0;
            var buffer = new byte[blockByteSize];
            var fileSavingOperations = new List<Task>();
            fileStream.Seek(0, SeekOrigin.Begin);

            using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous))
            // 4096 is default, async prevents the file from breaking when writing from the middle of the file
            {
                int currentByteBlockSize;
                do
                {
                    currentByteBlockSize = fileStream.Read(buffer, 0, buffer.Length);
                    file.Seek(bytesTransferred, SeekOrigin.Begin);
                    var fileSaving = file.WriteAsync(buffer, 0, currentByteBlockSize);

                    bytesTransferred += currentByteBlockSize;

                    fileSavingOperations.Add(fileSaving);
                }
                while (currentByteBlockSize != 0);

                return Task.WhenAll(fileSavingOperations);
            }
        }
    }
}