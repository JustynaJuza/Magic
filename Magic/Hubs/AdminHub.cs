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

        public async Task FetchSet(string id, bool includeCards)
        {
            Clients.Caller.updateRequestProgress("Request in progress...");
            var requestUrl = new Uri("http://api.mtgdb.info/sets/" + id);
            var requestHandler = new HttpClient();
            //setHandler.DownloadProgressChanged += DownloadProgressCallback; 
            //cardsHandler.DownloadProgressChanged += DownloadProgressCallback;

            try
            {
                requestHandler.GetStringAsync(requestUrl).ContinueWith(request => ProcessSet(request.Result));
                requestHandler.GetStringAsync(requestUrl + "/cards/").ContinueWith(request => ProcessCards(request.Result));
                //    Task.Factory.StartNew(() => setHandler.DownloadStringTaskAsync(requestUrl).ContinueWith(request => ProcessSet(request.Result)));
                //    Task.Run(() => cardsHandler.DownloadStringTaskAsync(requestUrl + "/cards/").ContinueWith(request => ProcessCards(request.Result))));
            }
            catch (Exception ex) { }
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
        }

        private async void ProcessCards(string response)
        {
            Clients.Caller.updateRequestProgress("Processing card data...");
            var cards = JArray.Parse(response).Select(c => JsonConvert.DeserializeObject<Card>(c.ToString(), new CardConverter())).ToList();
            foreach (var card in cards)
            {
                var path = FetchCardImage(card.MultiverseId, card.Id);
                using (var context = new MagicDbContext())
                {
                    card.AssignTypes(context);
                    card.DecodeManaCost(context);
                    card.Image = await path;
                    card.ImagePreview = card.Image.Replace(".jpg", ".jpeg");
                    context.InsertOrUpdate(card);
                }
            }
            Clients.Caller.updateRequestProgress("Card data saved!");
        }

        public async Task<string> FetchCardImage(int id, string fileName)
        {
            string path = null;
            var requestPreviewUrl = new Uri("http://api.mtgdb.info/content/card_images/" + id + ".jpeg");
            var requestUrl = new Uri("http://api.mtgdb.info/content/hi_res_card_images/" + id + ".jpg");
            var requestHandler = new HttpClient();

            try
            {
                requestHandler.GetStreamAsync(requestUrl).ContinueWith(request =>
                {
                    path = FilesController.SaveFile(request.Result, fileName, "/Cards");
                });
                requestHandler.GetStreamAsync(requestPreviewUrl).ContinueWith(request => FilesController.SaveFile(request.Result, fileName, "/Cards"));
            }
            catch (Exception ex) { }
            return path;
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