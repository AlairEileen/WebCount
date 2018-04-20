using ConfigData;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tools.DB;
using Tools.Response;
using Tools.Response.Json;
using We7Tools.Extend;
using We7Tools.Models;

namespace WebCommon.Controllers
{
    public class ExternalController : Controller
    {

        private IHostingEnvironment hostingEnvironment;
        public ExternalController(IHostingEnvironment environment)
        {
            this.hostingEnvironment = environment;
        }
        // GET: /<controller>/
        public IActionResult Index(string key)
        {
            if (key==null)
            {
                return new RedirectToActionResult("Index", "WebError", new { errorType = ErrorType.ErrorNoUserOrTimeOut });
            }
            ViewData["key"] = key;
            var db = new MongoDBTool().GetMongoCollection<We7Temp>();
            We7Temp data = null;

            if (MainConfig.IsDev||hostingEnvironment.IsDevelopment())
                data = db.Find(x => x.We7TempID.Equals(new ObjectId(key))).FirstOrDefault();
            else
                data = db.FindOneAndDelete(x => x.We7TempID.Equals(new ObjectId(key)));

            if (data == null)
            {
                return new RedirectToActionResult("Index", "WebError", new { errorType = ErrorType.ErrorNoUserOrTimeOut });
            }
            ViewData["we7Data"] = data.Data;
            JObject jObject = (JObject)JsonConvert.DeserializeObject(data.Data);
            string uniacid = (string)jObject["uniacid"];
            if (!string.IsNullOrEmpty(uniacid))
            {
                HttpContext.Session.PushWe7Data(data.Data);
            }
            //hasIdentity = true;
            //var urlReferrer = HttpContext.Request.Headers["referer"].FirstOrDefault();

            //if (urlReferrer!=null)
            //{
            //    var urlBase = urlReferrer.ToString();
            //    HttpContext.SetReferrer(urlBase);
            //}
            //HttpContext.SetReferrer(JsonConvert.SerializeObject(HttpContext.Request.Headers));

            string preUrl = Request.Headers["Referer"];
            if (HttpContext.GetReferrer() == null&&preUrl!=null)
            {
                HttpContext.SetReferrer(preUrl);
            }
            return RedirectToAction("Index", "Merchant");
        }
        public IActionResult ReceiveWe7Data()
        {
            try
            {
                string json = new StreamReader(Request.Body).ReadToEnd();
                var db = new MongoDBTool().GetMongoCollection<We7Temp>();
                var we7Temp = new We7Temp() { Data = json };
                db.InsertOne(we7Temp);
                return new JsonResponse1<string>() { JsonData = we7Temp.We7TempID.ToString() }.ToJsonResult(this);
            }
            catch (Exception)
            {
                return this.JsonErrorStatus();
            }
        }
    }
}
