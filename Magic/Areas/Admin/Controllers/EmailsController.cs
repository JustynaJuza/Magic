using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using RazorEngine;
using RazorEngine.Templating;

namespace Magic.Areas.Admin.Controllers
{
    public class EmailService
    {
        private static string templatePath = "~/Areas/Admin/Views/Emails/";

        public static bool SendEmail<T>(T model, string templateName) where T : EmailModel
        {
            var path = HostingEnvironment.MapPath(VirtualPathUtility.ToAbsolute(templatePath + templateName));

            var message = new MailMessage(
                new MailAddress("service@magicgame.co.uk"),
                new MailAddress(model.Email, model.Name))
            {
                Subject = "Magic: " + model.Name + "'s message.",
                IsBodyHtml = true,
                Body = Engine.Razor.RunCompile(System.IO.File.ReadAllText(path), "contact", typeof (T), model)
                        .Replace("\r", "")
                        .Replace("\n", "<br />")
            };

            message.ReplyToList.Add(new MailAddress(model.Email, model.Name));

            try
            {
                var emailService = new SmtpClient();
                emailService.SendCompleted += SendCompletedCallback;
                emailService.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // Get the unique identifier for this asynchronous operation.
            var token = (string) e.UserState;

            if (e.Cancelled)
            {
                Console.WriteLine("[{0}] Send canceled.", token);
            }
            if (e.Error != null)
            {
                Console.WriteLine("[{0}] {1}", token, e.Error.ToString());
            }
            else
            {
                Console.WriteLine("Message sent.");
            }
        }
    }

    public class EmailModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class EmailsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
	}
}