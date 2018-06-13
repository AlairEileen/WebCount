using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Tools.Models;
using Tools.Strings;
using WebCommon;
using WebCount.Models;

namespace WebCount.AppDatas
{
    public class AllCountData : BaseData<MerchantModel>
    {
        #region 刷新商户及商户的小程序方法
        const string merchantsUrl = "http://we7api.360yingketong.com/index/index/users?key=dlkZ%24%23H1Ky%5E%252n%5E%5Dz%25%2Cv0VlC%40ktvb~B%40&p=",
           appsUrl = "http://we7api.360yingketong.com/index/index/wxapplist?key=dlkZ%24%23H1Ky%5E%252n%5E%5Dz%25%2Cv0VlC%40ktvb~B%40&p=";

        internal void RefreshMerchants(object state)
        {
            RefreshMerchants();
        }

        internal List<WeChatCountAppModel> GetAllCounts()
        {
            //var fb = Builders<WeChatCountAppModel>.Projection;
            //var fields = fb.Exclude(d => d.AccountSum).Exclude(d => d.ShareAccountSum).Exclude(d => d.ShareSum);
            return mongo.GetMongoCollection<WeChatCountAppModel>().Find(Builders<WeChatCountAppModel>.Filter.Empty).ToList();
        }

        private void RefreshMerchants()
        {

            try
            {
                using (WebClient wcc = new WebClient())
                {
                    var json = wcc.DownloadStringTaskAsync(merchantsUrl + 1).Result;
                    var list = JsonConvert.DeserializeObject<AllCountRequestJsonModel<MerchantModel>>(json);
                    var pageSum = list.data.maxpage;
                    TaskFactory taskFactory = new TaskFactory();
                    var tasks = new Task[pageSum];
                    var pageIndexs = new List<int>();
                    for (int i = 0; i < pageSum; i++)
                    {
                        pageIndexs.Add(i + 1);
                    }
                    foreach (var item in pageIndexs)
                    {
                        tasks[item - 1] = new Task(() => { GetMerchantsTask(item); });
                    }
                    taskFactory.ContinueWhenAll(tasks, t => { RefreshMerchantsApps(); });
                    collection.DeleteMany(Filter.Empty);
                    Array.ForEach(tasks, x => x.Start());
                }
            }
            catch (Exception e)
            {
                e.Save();
            }





        }




        private void GetMerchantsTask(int index)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    var json = wc.DownloadStringTaskAsync(merchantsUrl + index).Result;
                    var list = JsonConvert.DeserializeObject<AllCountRequestJsonModel<MerchantModel>>(json);
                    collection.InsertMany(list.data.list);
                }
            }
            catch (Exception e)
            {
                e.Save();
            }

        }


        internal List<MerchantCountAppModel> GetMerchantCounts(string merchantID)
        {
            var list = mongo.GetMongoCollection<WeChatCountAppModel>().Find(Builders<WeChatCountAppModel>.Filter.Empty).ToList();
            var result = new List<MerchantCountAppModel>();
            list.ForEach(x =>
            {
                var item = x.MerchantCountAppModels.Find(y => y.uid == int.Parse(merchantID));
                if (item != null)
                {
                    result.Add(item);
                }
            });
            return result;
        }

        internal WeChatCountModel GetMiniApptCounts(string uniacid)
        {
            var model = mongo.GetMongoCollection<WeChatCountModel>().Find(Builders<WeChatCountModel>.Filter.Eq(x => x.uniacid, uniacid)).FirstOrDefault();
            return model;
        }
        private void RefreshMerchantsApps()
        {
            try
            {
                using (WebClient wcc = new WebClient())
                {
                    var json = wcc.DownloadStringTaskAsync(appsUrl + 1).Result;
                    var list = JsonConvert.DeserializeObject<AllCountRequestJsonModel<MerchantAppModel>>(json);
                    var pageSum = list.data.maxpage;
                    TaskFactory taskFactory = new TaskFactory();
                    Task[] tasks = new Task[pageSum];

                    var pageIndexs = new List<int>();
                    for (int i = 0; i < pageSum; i++)
                    {
                        pageIndexs.Add(i + 1);
                    }
                    foreach (var item in pageIndexs)
                    {
                        tasks[item - 1] = new Task(() => { GetMiniAppsTask(item); });
                    }


                    taskFactory.ContinueWhenAll(tasks, t => { });
                    mongo.GetMongoCollection<MerchantAppModel>().DeleteMany(Builders<MerchantAppModel>.Filter.Empty);
                    Array.ForEach(tasks, x => x.Start());
                }
            }
            catch (Exception e)
            {
                e.Save();
            }

        }
        private void GetMiniAppsTask(int index)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    var mamCollection = mongo.GetMongoCollection<MerchantAppModel>();
                    var json = wc.DownloadStringTaskAsync(appsUrl + index).Result;
                    var list = JsonConvert.DeserializeObject<AllCountRequestJsonModel<MerchantAppModel>>(json);
                    if (list == null || list.code != 0)
                    {
                        return;
                    }
                    mamCollection.InsertMany(list.data.list);
                }
            }
            catch (Exception e)
            {
                e.Save();
            }
        }

        #endregion


        /// <summary>
        /// 获取accessToken外部方法
        /// </summary>
        /// <param name="uniacID"></param>
        /// <returns></returns>
        internal string GetAccessToken(string uniacID)
        {
            var mamCollection = mongo.GetMongoCollection<MerchantAppModel>();
            var mamFilter = Builders<MerchantAppModel>.Filter;
            var mamUpdate = Builders<MerchantAppModel>.Update;
            var filterCountModel = mamFilter.Eq(x => x.uniacid, uniacID);
            var countModel = mamCollection.Find(filterCountModel).FirstOrDefault();
            var access_token = countModel?.AccessToken;
            //var timeOut = 0;


            if (countModel == null)
            {
                throw new ExceptionModel { ExceptionParam = Tools.Response.ResponseStatus.验证失败 };
            }

            return countModel.AccessToken;
        }
        /// <summary>
        /// 获取accessToken 内部方法
        /// </summary>
        /// <param name="timeOut">超时时间</param>
        /// <param name="access_token">token</param>
        /// <param name="appID">appid</param>
        /// <param name="appSecret">secret</param>
        private void GetWeChatAccessToken(ref int timeOut, ref string access_token, string appID, string appSecret)
        {
            WebClient wc = new WebClient();
            var response = wc.DownloadStringTaskAsync($"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appID}&secret={appSecret}");

            var jObj = JsonConvert.DeserializeObject<JObject>(response.Result);
            JToken at;
            if (jObj.TryGetValue("access_token", out at))
            {
                access_token = at.ToString();
            }
            else
            {
                access_token = null;
            }
            JToken ei;
            if (jObj.TryGetValue("expires_in", out ei))
            {
                timeOut = Convert.ToInt32(ei.ToString());
            }
            else
            {
                timeOut = 0;
            }
        }
        /// <summary>
        /// 获取accessToken 异步方法
        /// </summary>
        /// <param name="wc"></param>
        /// <param name="appID"></param>
        /// <param name="appSecret"></param>
        /// <returns></returns>
        public string GetWeChatAccessToken(WebClient wc, string appID, string appSecret)
        {
            try
            {
                JObject jObj = null;
                var access_token = "";
                var timeOut = 0;
                var response = wc.DownloadStringTaskAsync($"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appID}&secret={appSecret}");

                jObj = JsonConvert.DeserializeObject<JObject>(response.Result);

                JToken at;
                if (jObj.TryGetValue("access_token", out at))
                {
                    access_token = at.ToString();
                }
                else
                {
                    access_token = null;
                }
                JToken ei;
                if (jObj.TryGetValue("expires_in", out ei))
                {
                    timeOut = Convert.ToInt32(ei.ToString());
                }
                else
                {
                    timeOut = 0;
                }
                return access_token;
            }
            catch (Exception)
            {
                return null;
            }

        }
        /// <summary>
        /// 刷新AccessToken函数，利用timer刷新
        /// </summary>
        /// <param name="state"></param>
        internal void RefreshAccessToken(object state)
        {
            var mamCollection = mongo.GetMongoCollection<MerchantAppModel>();
            var mamFilter = Builders<MerchantAppModel>.Filter;
            var mamUpdate = Builders<MerchantAppModel>.Update;
            var countModelList = mamCollection.Find(mamFilter.Empty).ToList();
            foreach (var item in countModelList)
            {
                var access_token = item.AccessToken;
                var timeOut = 0;
                GetWeChatAccessToken(ref timeOut, ref access_token, item.AppID, item.AppSecret);
                if (access_token != null)
                {
                    mamCollection.UpdateOne(x => x.ID.Equals(item.ID), mamUpdate
                      .Set(x => x.AccessToken, access_token)
                      .Set(x => x.TimeOutLength, timeOut)
                      .Set(x => x.LastRefreshTime, DateTime.Now));
                }

                Thread.Sleep(new Random().Next(100));
            }
        }

        int pageSize = 60;


        internal PageContent<List<MerchantAppModel>> QueryMiniApps(string appName, string merchantID, int pageIndex)
        {
            var pageContent = new PageContent<List<MerchantAppModel>>();
            var wcca = mongo.GetMongoCollection<MerchantAppModel>();
            var filter = Builders<MerchantAppModel>.Filter.Empty;
            if (!string.IsNullOrEmpty(appName))
            {
                filter = Builders<MerchantAppModel>.Filter.Regex(x => x.AppName, $"/{appName}/");
            }

            if (!string.IsNullOrEmpty(merchantID))
            {
                filter = Builders<MerchantAppModel>.Filter.Eq(x => x.uid, int.Parse(merchantID));
            }
            var wccaData = wcca.Find(filter);
            pageContent.Sum = wccaData.Count();
            pageContent.PageIndex = pageIndex;
            pageContent.PageSum = pageContent.Sum / pageSize + (pageContent.Sum % pageSize == 0 ? 0 : 1);
            pageContent.PageData = wccaData.Skip(pageIndex * pageSize).Limit(pageSize).ToList();
            return pageContent;
        }


        internal PageContent<object> QueryMerchants(string merchantName, int pageIndex)
        {
            var pageContent = new PageContent<object>();
            var wcca = mongo.GetMongoCollection<MerchantModel>();
            var filter = Builders<MerchantModel>.Filter.Empty;
            if (!string.IsNullOrEmpty(merchantName))
            {
                filter = Builders<MerchantModel>.Filter.Regex(x => x.MerchatName, $"/{merchantName}/");
            }
            var wccaData = wcca.Find(filter);
            pageContent.Sum = wccaData.Count();
            pageContent.PageIndex = pageIndex;
            pageContent.PageSum = pageContent.Sum / pageSize + (pageContent.Sum % pageSize == 0 ? 0 : 1);
            var list = new List<object>();
            var wccaDataList = wccaData.Skip(pageIndex * pageSize).Limit(pageSize).ToList();
            var appCollection = mongo.GetMongoCollection<MerchantAppModel>();
            wccaDataList.ForEach(x =>
            {
                list.Add(new
                {
                    Merchant = x,
                    AppCount = appCollection.Find(y => y.uid == x.uid).Count()
                });
            });
            pageContent.PageData = list;
            return pageContent;
        }


    }
    public class PageContent<T>
    {
        public long Sum { get; set; }
        public long PageIndex { get; set; }
        public long PageSum { get; set; }
        public T PageData { get; set; }
    }

    #region 刷新商户及商户的小程序所需的模型
    public class AllCountRequestJsonModel<T>
    {
        public int code { get; set; }
        public string msg { get; set; }
        public AllCountRequestPage<T> data { get; set; }
    }

    public class AllCountRequestPage<T>
    {
        public int count { get; set; }
        public int maxpage { get; set; }
        public List<T> list { get; set; }
    }
    #endregion


}
