using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Magic.Areas.Admin.Controllers;
using Magic.Models;
using Magic.Models.DataContext;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Magic.Hubs
{
    public class AdminHub : Hub
    {
        //public void Insert(Card model, bool isUpdate = false)
        //{
        //    model.Id = model.Name.ToLower().Replace(" ", "_").Replace("[^a-z0-9]*", ""); //Guid.NewGuid().ToString();
        //    InsertOrUpdate(model, isUpdate);
        //}
        
        //public void Update(Card model, bool isUpdate = false) //[Bind(Include = "Id, Name")] 
        //{
        //    InsertOrUpdate(model, isUpdate);
        //}

        //private void InsertOrUpdate(Card model, bool isUpdate = false)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        string errorText;
        //        TempData["Error"] = context.InsertOrUpdate(model, out errorText) ? null : errorText;
        //        return RedirectToAction("Index");
        //    }

        //    // Process model errors.
        //    ViewBag.IsUpdate = isUpdate;
        //    return View("CreateOrEdit", model);
        //}


        //public static async Task<Card> MakeCardsRequest()
        //{
        //    var requestUrl = new Uri("http://api.mtgdb.info/cards/TrainedCondor");
        //    var requestHandler = new WebClient();
        //    requestHandler.DownloadProgressChanged += DownloadProgressCallback;

        //    var response = await requestHandler.DownloadStringTaskAsync(requestUrl);

        //    //if (response[0] == '{')
        //    //{
        //    //    JsonConvert.DeserializeObject<Card>(response, new CardConverter());
        //    //}
        //    //else
        //    //{
        //       var cards = JArray.Parse(response).Select(c => JsonConvert.DeserializeObject<Card>(c.ToString(), new CardConverter()));
        //    //}
        //    foreach (var card in cards)
        //    {
        //        using (var context = new MagicDbContext())
        //        {
        //            card.AssignTypes(context);
        //            card.DecodeManaCost(context);
        //            context.InsertOrUpdate(card);
        //        }
        //    }
        //    return cards.FirstOrDefault();
        //}

        public async Task FetchSetWithCards(string id)
        {
            FetchSet(id, true);
        }

        public async Task FetchSet(string id, bool includeCards)
        {
            //Task<IList<Card>> cardProcessing = null;
            //var adminHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            //adminHubContext.Clients.Caller.updateRequestProgress("Request in progress...");
            var requestUrl = new Uri("http://api.mtgdb.info/sets/" + id);
            var requestHandler = new HttpClient();
            //setHandler.DownloadProgressChanged += DownloadProgressCallback; 
            //cardsHandler.DownloadProgressChanged += DownloadProgressCallback;
            try
            {
                var setProcessing = requestHandler.GetStringAsync(requestUrl).ContinueWith(request => ProcessSet(request.Result));
                requestHandler.GetStringAsync(requestUrl + "/cards/").ContinueWith(async request =>
                {
                        await setProcessing;
                        ProcessCards(request.Result);
                    });
                //    Task.Factory.StartNew(() => setHandler.DownloadStringTaskAsync(requestUrl).ContinueWith(request => ProcessSet(request.Result)));
                //    Task.Run(() => cardsHandler.DownloadStringTaskAsync(requestUrl + "/cards/").ContinueWith(request => ProcessCards(request.Result))));
            }
            catch (Exception ex) { }

            //return cardProcessing.Result ?? new List<Card>();
            //return new List<Card>();
        }

        private void ProcessSet(string response)
        {
            Clients.Caller.updateRequestProgress("Processing set data...");
            var set = JsonConvert.DeserializeObject<CardSet>(response, new CardSetConverter());
            using (var context = new MagicDbContext())
            {
                context.InsertOrUpdate(set);
            }

            Clients.Caller.updateRequestProgress("Set data saved!");
            Clients.Caller.updateCardsInSet(set.Total);
        }

        private async void ProcessCards(string response)
        {
            Clients.Caller.updateRequestProgress("Processing card data...");
            var cards = JArray.Parse(response).Select(c => JsonConvert.DeserializeObject<Card>(c.ToString(), new CardConverter())).ToList();
            Clients.Caller.updateCardsTotal(cards.Count);

            foreach (var card in cards)
            {
                var path = FetchCardImage(card.MultiverseId, card.Id);
                try
                {
                    using (var context = new MagicDbContext())
                    {
                        card.AssignTypes(context);
                        card.DecodeManaCost(context);
                        card.Image = "/Content/Images/Cards/" + card.Id;
                        card.ImagePreview = card.Image.Replace(".jpg", ".jpeg");
                        context.InsertOrUpdate(card);
                    }
                }
                catch (Exception ex)
                {
                    Clients.Caller.updateRequestProgress("There was an error when processing this card: " + card.ToString());
                }
                Clients.Caller.updateCardsProcessed();
            }

            Clients.Caller.updateRequestProgress("Card data saved!");
            //return cards.ToList();
        }

        public static async Task FetchCardImage(int id, string fileName)
        {
            var imagePreviewUrl = new Uri("http://api.mtgdb.info/content/card_images/" + id + ".jpeg");
            var imageUrl = new Uri("http://api.mtgdb.info/content/hi_res_card_images/" + id + ".jpg");
            var requestHandler = new HttpClient();

            try
            {
                requestHandler.GetStreamAsync(imageUrl).ContinueWith(request => FilesController.SaveFile(request.Result, fileName, "/Cards"));
                requestHandler.GetStreamAsync(imagePreviewUrl).ContinueWith(request => FilesController.SaveFile(request.Result, fileName, "/Cards"));
            }
            catch (Exception ex) { }
        }

        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine("{0} downloaded {1} of {2} bytes. {3} % complete...",
        e.UserState,
        e.BytesReceived,
        e.TotalBytesToReceive,
        e.ProgressPercentage);
        }
    }
}