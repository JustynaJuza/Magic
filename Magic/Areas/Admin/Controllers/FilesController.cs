using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace Magic.Areas.Admin.Controllers
{
    public class FilesController : Controller
    {
        public static string UploadFile(HttpPostedFileBase file, string uploadPath = "", bool allowImageOnly = true)
        {
            if (allowImageOnly)
            {
                var isImageFile = Regex.IsMatch(file.ContentType, "image");
                if (!isImageFile) return "This must be an image file.";
            }

            return SaveFile(file.InputStream, file.FileName, uploadPath);
        }

        public static string SaveFile(Stream fileStream, string fileName, string uploadPath = "")
        {
            var path = VirtualPathUtility.ToAbsolute("~/Content/Images" + uploadPath + "/");
            var serverPath = HostingEnvironment.MapPath(path);
            if (!Directory.Exists(serverPath))
            {
                Directory.CreateDirectory(serverPath);
            }

            var file = new FileStream(serverPath + fileName, FileMode.Create, FileAccess.Write);
            fileStream.CopyToAsync(file);
            return path + fileName;
        }

    }
}