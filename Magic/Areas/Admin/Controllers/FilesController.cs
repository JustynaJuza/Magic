using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Magic.Areas.Admin.Controllers
{
    public class FilesController : Controller
    {
        [HttpPost]
        public async Task<string> UploadFile(HttpPostedFileBase file, string uploadPath = "", bool allowImageOnly = true)
        {
            if (allowImageOnly)
            {
                var isImageFile = Regex.IsMatch(file.ContentType, "image");
                uploadPath = "/Images" + uploadPath;
                if (!isImageFile) return "This must be an image file.";
            }

            return await SaveFile(file.InputStream, file.FileName, uploadPath);
        }

        public static async Task<string> SaveFile(Stream fileStream, string fileName, string uploadPath = "")
        {
            var path = "/Content" + uploadPath + "/";
            var absolutePath = VirtualPathUtility.ToAbsolute("~" + path);
            var serverPath = HostingEnvironment.MapPath(absolutePath);
            if (!Directory.Exists(serverPath))
            {
                Directory.CreateDirectory(serverPath);
            }

            var bytesTotal = fileStream.Length;
            var bytesTransferred = 0;
            var buffer = new byte[1048576]; // 1048576B = 1MB
            var fileSavingOperations = new List<Task>();
            using (var file = new FileStream(serverPath + fileName, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous)) 
                // 4096 is default, async prevents the file from breaking
            {
                int currentByteBlockSize;
                do
                {
                    currentByteBlockSize = fileStream.Read(buffer, 0, buffer.Length);
                    file.Seek(bytesTransferred, SeekOrigin.Begin);
                    bytesTransferred += currentByteBlockSize;
                    var percentage = bytesTransferred / bytesTotal * 100;
                    
                    fileSavingOperations.Add(file.WriteAsync(buffer, 0, currentByteBlockSize).ContinueWith(
                        finishedTask =>
                        {
                            // TODO: update progress in view
                        }));
                }
                while (currentByteBlockSize != 0);

                await Task.WhenAll(fileSavingOperations);
            }

            //var file = new FileStream(serverPath + fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            //await fileStream.CopyToAsync(file).ContinueWith(finishedTask => file.Close());

            //using (var file = new FileStream(serverPath + fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            //{
            //    await fileStream.CopyToAsync(file);
            //}

            return path + fileName;
        }

        public static string GetFileIconAsString(string filePath)
        {
            //var path = VirtualPathUtility.ToAbsolute(System.Web.HttpContext.Current.Request.ApplicationPath + filePath);
            var path = VirtualPathUtility.ToAbsolute("~" + filePath);
            var icon = Icon.ExtractAssociatedIcon(HostingEnvironment.MapPath(path));
            var image = icon.ToBitmap();
            var stream = new MemoryStream();
            image.Save(stream, ImageFormat.Png);
            return Convert.ToBase64String(stream.ToArray());
        }
    }
}