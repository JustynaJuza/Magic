using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Web.Mvc;
using Magic.Helpers;
using Magic.Hubs;
using Magic.Models;
using Magic.Models.DataContext;
using Magic.Models.Helpers;
using Magic.Models.JQueryDataTables;
using Microsoft.AspNet.SignalR;
using RazorEngine;
using RazorEngine.Templating;

namespace Magic.Areas.Admin.Controllers
{
    [System.Web.Mvc.Authorize]
    public class CardsController : Controller
    {
        private MagicDbContext context = new MagicDbContext();

        [HttpGet]
        public ViewResult Index()
        {
            return View();
            //return View(context.Cards.ToList());
        }

        [HttpPost]
        public ViewResult Index(IList<string> ids)
        {
            return View(context.Cards.ToList());
        }
        
        public JsonResult GetCardData(DataTablesRequestIn o)
        {
            //IQueryable cards;
            //if (!string.IsNullOrWhiteSpace(o.Search))
            //{
            //    cards = context.Cards.Where(c => c.SetId.Contains(o.Search.Value) || c.Name.Contains(o.Search.Value));
            //        //|| c.Rarity.GetDisplayName().Contains(o.Search.Value)
            //}
            //var sqlParameter = new SqlParameter("@ClientId", 4);
            var cards = context.Cards.SqlQuery("SearchAllCards @p0, @p1", o.Search.Value, o.SelectedColumns());
            var selectedCards = cards.OrderBy(c => c.SetId).Skip(o.Start)
                .Take(o.Length)
                .ToList();

            var serializedCards = selectedCards.Select(RenderCard).ToList();

            return new JsonResult { Data = new DataTablesRequestOut
            {
                draw = o.Draw,
                recordsTotal = context.Cards.Count(),
                recordsFiltered = selectedCards.Count(),
                data = serializedCards
            }, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        //public static void UpdateCard(Card model)
        //{
        //    var partial = PartialView("~/Areas/Admin/Views/Shared/EditorTemplates/Card.cshtml", model)
        //    var adminHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
        //    adminHubContext.Clients.All.updateCard(model.Id, partial);
        //}

        //public async ActionResult FetchSetWithCards(string id)
        //{
        //    return await FetchSet(id, true).Result;
        //}

        //[AsyncTimeout(200)]
        //public async void FetchCardsAsync()
        //{
        //    AdminHub.MakeCardsRequest();
        //}

        #region CREATE/EDIT
        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.IsUpdate = false;
            return View("CreateOrEdit");
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            if (id != null)
            {
                string errorText;
                // TODO: Fix this or discard?
                //var model = context.Read<Card, string>((string) id, out errorText);
                //TempData["Error"] = errorText;
                var model = context.Cards.Find(id);
                if (model != null)
                {
                    ViewBag.IsUpdate = true;
                    return View("CreateOrEdit", model);
                }

                TempData["Error"] = MagicDbContext.ShowErrorMessage(new ArgumentNullException());
                return RedirectToAction("Index");
            }

            TempData["Message"] = "There was no item ID provided for editing, assuming creation of new item.";

            ViewBag.IsUpdate = false;
            return View("CreateOrEdit");

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public ActionResult Insert(Card model, bool isUpdate = false)
        {
            model.Id = model.Name.ToLower().Replace(" ", "_").Replace("[^a-z0-9]*", ""); //Guid.NewGuid().ToString();
            return InsertOrUpdate(model, isUpdate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public ActionResult Update(Card model, bool isUpdate = false) //[Bind(Include = "Id, Name")] 
        {
            return InsertOrUpdate(model, isUpdate);
        }

        private ActionResult InsertOrUpdate(Card model, bool isUpdate = false)
        {
            if (ModelState.IsValid)
            {
                string errorText;
                TempData["Error"] = context.InsertOrUpdate(model, out errorText) ? null : errorText;
                return RedirectToAction("Index");
            }

            // Process model errors.
            ViewBag.IsUpdate = isUpdate;
            return View("CreateOrEdit", model);
        }
        #endregion CREATE/EDIT

        #region DELETE
        public ActionResult Delete(Card model)
        {
            string errorText;
            TempData["Error"] = context.Delete(model, out errorText) ? null : errorText;
            return RedirectToAction("Index");
        }
        #endregion DELETE

        #region DISPOSE
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion DISPOSE

        private string[] RenderCard(Card card)
        {
            var stringRender = ViewRenderer.RenderRazorViewToString(ControllerContext, "_CardDisplayPartial", card);
            var stringArray = stringRender.Replace("\r", "").Replace("\n","").Split(new[] { "$$" }, StringSplitOptions.RemoveEmptyEntries);
            return stringArray;

            //var data = new string[7];
            //data[0] = card.SetId;
            //data[1] = card.Name;
            //data[2] = card.Rarity.GetDisplayName();

            //    var cardTypeDisplay = "";
            //foreach (var cardType in card.Types)
            //{
            //    cardTypeDisplay += cardType.Name + ' ';
            //}
            //data[3] = cardTypeDisplay;
            //data[4] = card.ConvertedManaCost.ToString();
            //data[5] = "x";
            //data[6] = "<a href=\"" + Url.Action("Edit", new { id = card.Id }) + "\" class=\"btn btn-primary btn-edit\">Edit</a> | " +
            //          "<a href=\"" + Url.Action("Delete", new { id = card.Id }) + "\" class=\"btn btn-danger btn-delete\">Delete</a>";

            //return data;
        }

        //private string Serialize(Card card)
        //{
        //    var data = new StringBuilder("{");
        //    data.AppendFormat("SetId:'{0}',Name:'{1}',Rarity:'{2}',CardType:'",
        //        card.SetId, card.Name, card.Rarity.GetDisplayName());

        //    foreach (var cardType in card.Types)
        //    {
        //        data.Append(' ').Append(cardType.Name);
        //    }

        //    data.AppendFormat("',Mana:'{0}',Colors:'{1}'",
        //        card.ConvertedManaCost, "x");

        //    data.Append(",Controls:'<a href=\"" + Url.Action("Edit", new { id = card.Id }) + "\" class=\"btn btn-primary btn-edit\">Edit</a>");
        //    data.Append(" | <a href=\"" + Url.Action("Delete", new { id = card.Id }) + "\" class=\"btn btn-danger btn-delete\">Delete</a>'}");

        //    return data.ToString();
        //}
    }
}