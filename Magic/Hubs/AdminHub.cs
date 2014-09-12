using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Magic.Models;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Magic.Hubs
{
    public class AdminHub : Hub
    {
        public static async Task<Card> MakeCardRequest()
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
            return cards.FirstOrDefault();
        }

        private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine("{0}    downloaded {1} of {2} bytes. {3} % complete...",
        e.UserState,
        e.BytesReceived,
        e.TotalBytesToReceive,
        e.ProgressPercentage);
        }
    }
}