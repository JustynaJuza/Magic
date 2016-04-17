using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Magic.Models.LandingPages;

namespace Autographer.Areas.CMS.Controllers 
{
    public class LandingPagesController : Controller
    {
        public ActionResult ShowAjaxFileUpload(string propertyId, string propertyName)
        {
            return PartialView("_AjaxUploadFilePartial", new Tuple<string, string>(propertyId, propertyName));
        }

        public string UploadFile(HttpPostedFileBase file, string uploadPath)
        {
            var path = "~/Content/Images/" + uploadPath + file.FileName;

            bool isImageFile = System.Text.RegularExpressions.Regex.IsMatch(file.ContentType, "image");
            //var extension = file.FileName.Split('.').Last();

            if (!isImageFile) return "This must be an image file.";

            file.SaveAs(Server.MapPath(path));
            return path;
        }

        public ActionResult AddAnimationFramePartial(int panelIndex = 0, int tileIndex = 0, int frameIndex = 0)
            //public ActionResult AddAnimationFramePartial(int panelIndex, int tileIndex, int frameIndex, LandingPage landingPage)
        {
            ViewBag.panelIndex = panelIndex.ToString();
            ViewBag.tileIndex = tileIndex.ToString();
            ViewBag.frameIndex = frameIndex.ToString();
            return PartialView("_AnimationFramePartial", new PictureTile.AnimationFrame());
        }

        //public LandingPage GetById(int id)
        //{
        //    return CurrentSession.Get<LandingPage>(id);
        //}

        //public ActionResult Index()
        //{
        //    var modelList = ControllerQuery.ToList();

        //    return View(modelList);
        //}

        [HttpGet]
        public virtual ActionResult Create()
        {
            ViewBag.UpdateMode = false;

            return View("CreateEdit", new LandingPage().AddDefaultPanels());
        }

        //[HttpGet]
        //public ActionResult Edit(int id)
        //{
        //    var model = GetById(id);
        //    //var viewModel = Mapper.Map<Art, CMSArtViewModel>(model);

        //    ViewBag.UpdateMode = true;

        //    return View("CreateEdit", model);
        //}

        [HttpPost]
        public ActionResult CreateEdit(LandingPage model, bool updateShortUrl = false, string publish = "")
        {
            var isNewEntry = model.Id == 0;

            if (ModelState.IsValid)
            {
                //ITransaction trans = CurrentSession.BeginTransaction();
                try
                {
                    model.Published = publish == "Publish" ? (DateTime?)DateTime.Now : null;

                    //CurrentSession.SaveOrUpdate(model);
                    //trans.Commit();

                    //var routeChange = false; //model.ShortUrl != null ? (isNewEntry || oldModel.ShortUrl != model.ShortUrl ? true : false) : false;
                    if (updateShortUrl)
                    {
                        //RouteConfig.RegisterShortRoute(model);
                    }

                    TempData["Message"] = (isNewEntry ? "Created" : "Updated") + " entry (#" + model.Id + ") " + model.Name;
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    //trans.Rollback();
                    TempData["Error"] = ex.ToString();
                }
            }

            ViewBag.UpdateMode = !isNewEntry;
            return View(model);
        }

        //public virtual ActionResult Publish(int id, bool publish = true)
        //{
        //    ITransaction trans = CurrentSession.BeginTransaction();
        //    try
        //    {
        //        var model = GetById(id);

        //        model.Published = publish ? (DateTime?)DateTime.Now : null;

        //        CurrentSession.Update(model);
        //        trans.Commit();

        //        TempData["Message"] = (publish ? "Published" : "Unpublished") + " entry (#" + model.Id + ") " + model.Name;

        //        return RedirectToAction("Index", new { published = publish });
        //    }
        //    catch (Exception ex)
        //    {
        //        trans.Rollback();
        //        TempData["Error"] = ex.ToString();
        //    }

        //    return RedirectToAction("Index");
        //}

    //    [HttpGet]
    //    public virtual ActionResult PreviewDelete(int id)
    //    {
    //        var model = GetById(id);
    //        return View("Delete", model);
    //    }

    //    [HttpPost]
    //    public virtual ActionResult Delete(int id)
    //    {
    //        ITransaction trans = CurrentSession.BeginTransaction();
    //        try
    //        {
    //            var model = GetById(id);
    //            //MvcApplication.registerDetailRoute(model, false);
    //            CurrentSession.Delete(model);
    //            trans.Commit();

    //            TempData["Message"] = "Deleted entry (#" + model.Id + ") " + model.Name;
    //            return RedirectToAction("Index");
    //        }
    //        catch (Exception ex)
    //        {
    //            trans.Rollback();
    //            TempData["Error"] = ex.ToString();
    //        }

    //        return RedirectToAction("Index");
    //    }

    //    public ActionResult AddTestPage()
    //    {
    //        try
    //        {
    //            ITransaction tran = CurrentSession.BeginTransaction();
    //            var landing = new LandingPage
    //            {
    //                Created = DateTime.Now,
    //                ShortUrl = "testlanding",
    //                ShareImageUrl = "C:\\Users\\justyna\\Desktop\\placeholder.png",
    //                ShareDescription = "awesome test landing page"
    //            };
    //            landing.Panels.Add(new IntroPanel
    //            {
    //                Title = "Welcome to landing pages",
    //                Description = "This is an example landing page.",
    //                ImageUrl = "C:\\Users\\justyna\\Desktop\\placeholder.png",
    //                ImageDescription = "placeholder image",
    //                ButtonDescription = "Hello"
    //            });
    //            landing.Panels.Add(new VideoPanel
    //            {
    //                Title = "A video."
    //            });
    //            landing.Panels.Add(new FormPanel
    //            {
    //                Title = "Here we have a form.",
    //                RegistrationCode = "TEST_LANDING"
    //            });

    //            var pictures = new PicturePanel
    //            {
    //                Title = "Here we should see pictures",
    //                Description = "This was a tricky thing to do!"
    //            };
    //            var tile = new PictureTile()
    //            {
    //                Colour = "#GDDGDG",
    //                Title = "A lovely view.",
    //                Description = "May 21, 2014"
    //            };
    //            tile.AnimationFrames.Add("C:\\Users\\justyna\\Desktop\\placeholder.png");


    //            pictures.Tiles.Add(tile);
    //            landing.Panels.Add(pictures);

    //            CurrentSession.Save(landing);
    //            tran.Commit();
    //        }
    //        catch (Exception exe)
    //        {
    //            return RedirectToAction("Index", "Home");
    //        }

    //        return RedirectToAction("Index", "Home");
    //    }
    }
}
