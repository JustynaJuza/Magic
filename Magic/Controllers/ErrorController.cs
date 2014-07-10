using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Magic.Controllers
{
    public enum ErrorType {
        DbReadError,
        DbInsertError,
        DbUpdateError,
        DbDeleteError
    }

    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public string ShowErrorMessage(Exception ex) {
            //var type = ex.GetType();
            //if (type == typeof(System.Data.Entity.Infrastructure.DbUpdateException)) return "";
            //else 
            return "There was a problem with saving to the database..." + ex.ToString(); 
        }
	}
}