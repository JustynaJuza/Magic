using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Magic.Helpers;
using Magic.Models;
using Magic.Models.DataContext;
using Magic.Models.Extensions;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Magic.Hubs
{
    public class AdminHub : Hub
    {
        private readonly IDbContext _context;
        private readonly IFileHandler _fileHandler;
        private readonly ICardService _cardService;

        public AdminHub(IDbContext context, IFileHandler fileHandler, ICardService cardService)
        {
            _context = context;
            _fileHandler = fileHandler;
            _cardService = cardService;
        }

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
            await FetchSet(id, true);
            _context.SaveChanges();
        }

        public Task FetchSet(string id, bool includeCards)
        {
            //Task<IList<Card>> cardProcessing = null;
            //var adminHubContext = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
            //adminHubContext.Clients.Caller.updateRequestProgress("Request in progress...");
            var requestUrl = new Uri("http://api.mtgdb.info/sets/" + id);

            using (var requestHandler = new HttpClient())
            {
                try
                {
                    var fetchSet = requestHandler.GetStringAsync(requestUrl);
                    var fetchCards = requestHandler.GetStringAsync(requestUrl + "/cards/");

                    var processSet = fetchSet
                        .ContinueWith(_ => ProcessSet(fetchSet.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
                    var processCards = Task.WhenAll(processSet, fetchCards)
                        .ContinueWith(_ => ProcessCards(fetchCards.Result), TaskContinuationOptions.OnlyOnRanToCompletion);
                    
                    return processCards;                    
                }
                catch (Exception ex)
                {
                    ex.LogException();
                    throw;
                }
            }
        }

        private void ProcessSet(string response)
        {
            Clients.Caller.updateRequestProgress("Processing set data...");
            var set = JsonConvert.DeserializeObject<CardSet>(response, new CardSetConverter());
            _context.InsertOrUpdate(set);

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
                var fetchCardImage = FetchCardImage(card.MultiverseId, card.Id);
                try
                {
                        _cardService.AssignTypes(card);
                        _cardService.DecodeManaCost(card);
                        card.Image = "/Content/Images/Cards/" + card.Id;
                        card.ImagePreview = card.Image.Replace(".jpg", ".jpeg");
                        _context.InsertOrUpdate(card);
                }
                catch (Exception)
                {
                    Clients.Caller.updateRequestProgress("There was an error when processing this card: " + card.ToString());
                }

                await fetchCardImage;
                Clients.Caller.updateCardsProcessed();
            }

            Clients.Caller.updateRequestProgress("Card data saved!");
            //return cards.ToList();
        }

        public async Task<bool> FetchCardImage(int id, string fileName)
        {
            var imagePreviewUrl = new Uri("http://api.mtgdb.info/content/card_images/" + id + ".jpeg");
            var imageUrl = new Uri("http://api.mtgdb.info/content/hi_res_card_images/" + id + ".jpg");

            using (var requestHandler = new HttpClient())
            {
                try
                {
                    var imageSaved =
                        requestHandler.GetStreamAsync(imageUrl)
                            .ContinueWith(request => _fileHandler.SaveFileAsync(request.Result, fileName + ".jpg", "/Cards"));
                    var imagePreviewSaved =
                        requestHandler.GetStreamAsync(imagePreviewUrl)
                            .ContinueWith(request => _fileHandler.SaveFileAsync(request.Result, fileName + ".jpeg", "/Cards"));

                    return await imageSaved.Result && await imagePreviewSaved.Result;
                }
                catch (Exception ex)
                {
                    ex.LogException();
                    if (ex.HandleException(typeof(ArgumentNullException)))
                    {
                        return false;
                    }
                    throw;
                }
            }
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