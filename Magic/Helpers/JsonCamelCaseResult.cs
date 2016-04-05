using System;
using System.Net;
using System.Text;
using System.Web.Mvc;
using System.Web.WebPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Magic.Helpers
{
    public class JsonCamelCaseResult : JsonResult
    {
        public static JsonCamelCaseResult Error(string error)
        {
            return new JsonCamelCaseResult(new { success = false, error }, JsonRequestBehavior.DenyGet, HttpStatusCode.InternalServerError);
        }

        public JsonCamelCaseResult(object data = null, JsonRequestBehavior jsonRequestBehavior = JsonRequestBehavior.AllowGet, HttpStatusCode responseStatusCode = HttpStatusCode.OK)
        {
            Data = data ?? new { };
            JsonRequestBehavior = jsonRequestBehavior;
            ResponseStatusCode = responseStatusCode;
        }

        public HttpStatusCode ResponseStatusCode { get; set; }

        public string ResponseMessage { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (JsonRequestBehavior == JsonRequestBehavior.DenyGet && string.Equals(context.HttpContext.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("This request has been blocked because sensitive information could be disclosed to third party web sites when this is used in a GET request. To allow GET requests, set JsonRequestBehavior to AllowGet.");
            }

            var response = context.HttpContext.Response;

            response.ContentType = !string.IsNullOrEmpty(ContentType) ? ContentType : "application/json";
            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }
            if (Data == null)
                return;

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            response.Write(JsonConvert.SerializeObject(Data, jsonSerializerSettings));
            if (!string.IsNullOrWhiteSpace(ResponseMessage))
            {
                response.AddHeader("Message", ResponseMessage);
            }
            response.SetStatus(ResponseStatusCode);
        }
    }
}