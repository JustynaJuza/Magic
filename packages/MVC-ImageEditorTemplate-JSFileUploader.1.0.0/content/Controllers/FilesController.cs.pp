using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace $rootnamespace$.Controllers
{
    public class FilesController : Controller
    {
        public string UploadFile(HttpPostedFileBase file, string uploadPath = "", bool allowImageOnly = true)
        {
            var path = VirtualPathUtility.ToAbsolute("~/Content/Images" + uploadPath + "/");
            var serverPath = Server.MapPath(path);
            if (!Directory.Exists(serverPath)) {
                Directory.CreateDirectory(serverPath);
            }

            if (allowImageOnly)
            {
                bool isImageFile = Regex.IsMatch(file.ContentType, "image");
                if (!isImageFile) return "This must be an image file.";
            }

            file.SaveAs(serverPath + file.FileName);
            return path + file.FileName;
        }

        public ActionResult Example() {
            return View();
        }
    }
}