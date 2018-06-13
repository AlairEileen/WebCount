using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Tools.Response.Json;
using WebCount.Models;

namespace WebCount.Controllers
{
    public class TimerConfigController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetConfig()
        {
            return TimerConfigModelContext.GetConfig().ToJsonSuccess(this);
        }
        public IActionResult SetConfig()
        {
            string json = new StreamReader(Request.Body).ReadToEnd();
            var tcm = JsonConvert.DeserializeObject<TimerConfigModel>(json);
            TimerConfigModelContext.SaveConfigLog(tcm);
            return this.JsonSuccessStatus();
        }
    }
}
