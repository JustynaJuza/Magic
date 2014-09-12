using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web;
using System.Web.Mvc;
using Magic.Models;

namespace Magic.Areas.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetCards()
        {
            MakeCardRequest();
            return View();
        }

        private void MakeCardRequest()
        {
            var requestUrl = new Uri("http://api.mtgdb.info/cards/TrainedCondor");
            var request = WebRequest.Create(requestUrl) as HttpWebRequest;
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(String.Format(
                    "Server error (HTTP {0}: {1}).",
                    response.StatusCode,
                    response.StatusDescription));
                var jsonSerializer = new DataContractJsonSerializer(typeof(Card));

                //var objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                //Response jsonResponse
                //= objResponse as Response;
                //return jsonResponse;
            }
            //Response locationsResponse = MakeRequest(locationsRequest);
            //ProcessResponse(locationsResponse);

        }
    }
}