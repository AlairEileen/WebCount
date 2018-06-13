using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Tools.DB;
using Tools.Response;
using Tools.Response.Json;
using WebCommon.Controllers;
using WebCount.AppDatas;
using WebCount.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebCount.Controllers
{
    public class AllCountController : BaseController<AllCountData, MerchantModel>
    {
        #region 页面
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult DetailCounts()
        {
            return View();
        }
        public IActionResult MerchantList()
        {
            return View();
        }
        public IActionResult MiniAppList()
        {
            return View();
        }
        #endregion


        public IActionResult GetAllCounts()
        {
            return DoData("string", RequestClientType.None, null, (s, k) =>
            {
                return this.JsonSuccessWithLimit(new JsonResponse1<object> { JsonData = thisData.GetAllCounts() }, new string[] { "JsonData", "AccountSum", "ShareSum", "ShareAccountSum" });
                //return thisData.GetAllCounts().ToJsonSuccess(this);
            });
        }
        public IActionResult GetMerchantCounts(string merchantID)
        {
            return DoData("string", RequestClientType.None, null, (s, k) =>
            {
                return this.JsonSuccessWithLimit(new JsonResponse1<object> { JsonData = thisData.GetMerchantCounts(merchantID) }, new string[] { "JsonData", "AccountSum", "ShareSum", "ShareAccountSum" });
                //return thisData.GetMerchantCounts(merchantID).ToJsonSuccess(this);
            });
        }
        public IActionResult GetMiniApptCounts(string uniacid)
        {
            return DoData("string", RequestClientType.None, null, (s, k) =>
            {
                return this.JsonSuccessWithLimit(new JsonResponse1<object> { JsonData = thisData.GetMiniApptCounts(uniacid) }, new string[] { "JsonData", "AppName", "CountDataList", "DataType", "WeChatCountTypeName", "CountData" });
                //return thisData.GetMiniApptCounts(uniacid).ToJsonSuccess(this);
            });
        }
        public IActionResult Merchants(string merchantName)
        {
            ViewData["merchantName"] = merchantName;
            return View();
        }
        public IActionResult MiniApps(string appName, string merchantID)
        {
            ViewData["appName"] = appName;
            ViewData["merchantID"] = merchantID;
            return View();
        }
        public IActionResult QueryMerchants(string merchantName, int pageIndex)
        {
            return DoData("string", RequestClientType.None, null, (s, k) =>
            {
                return thisData.QueryMerchants(merchantName, pageIndex).ToJsonSuccess(this);
            });
        }
        public IActionResult QueryMiniApps(string appName, string merchantID, int pageIndex)
        {
            return DoData("string", RequestClientType.None, null, (s, k) =>
            {
                return thisData.QueryMiniApps(appName, merchantID, pageIndex).ToJsonSuccess(this);
            });
        }


        public IActionResult Test()
        {
            var collection = new MongoDBTool().GetMongoCollection<WeChatCountModel>();
            var filter = Builders<WeChatCountModel>.Filter;
            var filterSum = (filter.Eq("CountDataList.DataType", WeChatCountType.上周的画像) |
                filter.Eq("CountDataList.DataType", WeChatCountType.上月的画像) |
                filter.Eq("CountDataList.DataType", WeChatCountType.昨天的画像));
            var list = collection.Find(filterSum).ToList();
            return list.ToJsonSuccess(this);
        }
    }
}
