using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
        public static async Task<Card> MakeCardsRequest()
        {
            var requestUrl = new Uri("http://api.mtgdb.info/cards/TrainedCondor");
            var requestHandler = new WebClient();
            requestHandler.DownloadProgressChanged += DownloadProgressCallback;

            var response = await requestHandler.DownloadStringTaskAsync(requestUrl);

            //if (response[0] == '{')
            //{
            //    JsonConvert.DeserializeObject<Card>(response, new CardConverter());
            //}
            //else
            //{
               var cards = JArray.Parse(response).Select(c => JsonConvert.DeserializeObject<Card>(c.ToString(), new CardConverter()));
            //}
            foreach (var card in cards)
            {
                using (var context = new MagicDbContext())
                {
                    card.AssignTypes(context);
                    card.DecodeManaCost(context);
                    context.InsertOrUpdate(card);
                }
            }
            return cards.FirstOrDefault();
        }

        public async Task MakeSetRequest(string id, bool includeCards = true)
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

        private void ProcessCards(string response)
        {
            Clients.Caller.updateRequestProgress("Processing card data...");
            var cards = JArray.Parse(response).Select(c => JsonConvert.DeserializeObject<Card>(c.ToString(), new CardConverter()));
            foreach (var card in cards)
            {
                using (var context = new MagicDbContext())
                {
                    card.AssignTypes(context);
                    card.DecodeManaCost(context);
                    context.InsertOrUpdate(card);
                }
            }
            Clients.Caller.updateRequestProgress("Card data saved!");
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