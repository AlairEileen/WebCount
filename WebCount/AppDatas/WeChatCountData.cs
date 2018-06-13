using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tools.Models;
using Tools.Strings;
using WebCommon;
using WebCount.Models;

namespace WebCount.AppDatas
{
    public class WeChatCountData : BaseData<WeChatCountModel>
    {
        FilterDefinitionBuilder<MerchantAppModel> mamFilter = Builders<MerchantAppModel>.Filter;
        UpdateDefinitionBuilder<MerchantAppModel> mamUpdate = Builders<MerchantAppModel>.Update;
        AllCountData allCountData = new AllCountData();

        internal void RefreshWeChatCountData(object state)
        {
            RefreshWeChatCountDataAsync();
        }

        private void RefreshCountSumData()
        {
            var wccmCollection = GetWCCMCollection();
            var mmCollection = mongo.GetMongoCollection<MerchantModel>();
            var cmCollection = mongo.GetMongoCollection<WeChatCountAppModel>();
            var cmList = cmCollection.Find(Builders<WeChatCountAppModel>.Filter.Empty).ToList();
            if (cmList != null && cmList.Count > 30)
                cmCollection.DeleteMany(Builders<WeChatCountAppModel>.Filter.Lte(x => x.LastChangeTime, cmList[29].LastChangeTime));
            var list = (wccmCollection.Find(Builders<WeChatCountModel>.Filter.Empty)).ToList();
            var countModel = new WeChatCountAppModel();
            foreach (var wccm in list)
            {
                try
                {
                    var merchant = (mmCollection.Find(x => x.uid == wccm.uid)).FirstOrDefault();
                    var merchantCount = GetMerchantCount(wccm, merchant, countModel);

                    var uniacid = int.Parse(wccm.uniacid);
                    var app = new AppCountModel { AppName = wccm.AppName, uniacid = uniacid };
                    if (merchantCount.AppCountModels == null)
                    {
                        merchantCount.AppCountModels = new List<AppCountModel> { app };
                    }
                    else
                    {
                        merchantCount.AppCountModels.Add(app);
                    }
                    var wccmData = wccm.CountDataList.Find(x => x.DataType == WeChatCountType.昨天的概况);
                    if (wccmData != null)
                    {
                        var dataModel = JsonConvert.DeserializeObject<WeChatCountBaseJsonModel>(wccmData.CountData);
                        app.AccountSum = dataModel.list[0].visit_total;
                        app.ShareAccountSum = dataModel.list[0].share_uv;
                        app.ShareSum = dataModel.list[0].share_pv;
                        merchantCount.ShareSum += app.ShareSum;
                        merchantCount.ShareAccountSum += app.ShareAccountSum;
                        merchantCount.AccountSum += app.AccountSum;
                        countModel.AccountSum += app.AccountSum;
                        countModel.ShareAccountSum += app.ShareAccountSum;
                        countModel.ShareSum += app.ShareSum;
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
            cmCollection.InsertOne(countModel);
        }

        private MerchantCountAppModel GetMerchantCount(WeChatCountModel wccm, MerchantModel merchant, WeChatCountAppModel countModel)
        {
            MerchantCountAppModel merchant2 = null;
            if (countModel.MerchantCountAppModels != null)
            {
                merchant2 = countModel.MerchantCountAppModels.Find(x => x.uid == wccm.uid);
            }
            else
            {
                countModel.MerchantCountAppModels = new List<MerchantCountAppModel>();
            }
            if (merchant2 == null)
            {
                merchant2 = new MerchantCountAppModel { uid = wccm.uid, MerchantName = merchant.MerchatName };
                countModel.MerchantCountAppModels.Add(merchant2);
            }
            return merchant2;
        }

        private void RefreshWeChatCountDataAsync()
        {
            try
            {
                var mamCollection = GetMAMCollection();
                var wccmCollection = GetWCCMCollection();
                var mamList = mamCollection.Find(Builders<MerchantAppModel>.Filter.Empty).ToList();
                var taskFactory = new TaskFactory();
                var tasks = new List<Task>();
                foreach (var item in mamList)
                {
                    if (!item.AppID.CheckEmpty(item.AppSecret))
                    {
                        tasks.Add(new Task(() => GetWeChatDataAsync(mamCollection, item, wccmCollection)));
                    }
                }
                var taskArray = tasks.ToArray();
                taskFactory.ContinueWhenAll(taskArray, x => RefreshCountSumData());
                Array.ForEach(taskArray, x => x.Start());
            }
            catch (Exception e)
            {
                e.Save();
            }
        }


        private IMongoCollection<WeChatCountModel> GetWCCMCollection()
        {
            return mongo.GetMongoCollection<WeChatCountModel>();
        }
        private void GetWeChatDataAsync(IMongoCollection<MerchantAppModel> mamCollection, MerchantAppModel mam, IMongoCollection<WeChatCountModel> wccmCollection)
        {

            try
            {
                using (WebClient wc = new WebClient())
                {
                    var token = allCountData.GetWeChatAccessToken(wc, mam.AppID, mam.AppSecret);
                    if (string.IsNullOrEmpty(token))
                    {
                        return;
                    }
                    var data = GetWeChatRequestData();
                    foreach (var item in data)
                    {
                        string jsonData = "";
                        int num = (int)item.Key;
                        if (num == 5 || num == 51 || num == 50)
                        {
                            jsonData = GetWeChatCountPortrait(wc, item.Value, token, num);
                        }
                        else if (num < 10)
                        {
                            jsonData = GetWeChatCountYesterDay(wc, item.Value, token);

                        }
                        else if (num % 10 != 0)
                        {
                            jsonData = GetWeChatCountLastWeak(wc, item.Value, token, num % 10);
                        }
                        else
                        {
                            jsonData = GetWeChatCountLastMonth(wc, item.Value, token);
                        }
                        if (!string.IsNullOrEmpty(jsonData))
                        {
                            SaveCountData(wccmCollection, jsonData, mam, item);

                        }
                    }
                }
            }
            catch (Exception e)
            {
                e.Save();
            }

        }



        private void SaveCountData(IMongoCollection<WeChatCountModel> wccmCollection, string jsonData, MerchantAppModel mam, KeyValuePair<WeChatCountType, string> item)
        {
            try
            {
                var model = (wccmCollection.Find(x => x.uniacid.Equals(mam.uniacid))).FirstOrDefault();
                if (model == null)
                {
                    wccmCollection.InsertOne(new WeChatCountModel
                    {
                        uniacid = mam.uniacid,
                        AppName = mam.AppName,
                        uid = mam.uid,
                        CountDataList = new List<WeChatCountDataModel> { (new WeChatCountDataModel { DataType = item.Key,
                        CountData = jsonData} )}
                    });
                }
                else if (model.CountDataList == null)
                {
                    wccmCollection.UpdateOne(x => x.uniacid.Equals(mam.uniacid), Builders<WeChatCountModel>.Update.Set(x => x.CountDataList, new List<WeChatCountDataModel> { (new WeChatCountDataModel { DataType = item.Key,
                        CountData = jsonData} )}).Set(x => x.LastChangeTime, DateTime.Now).Set(x => x.AppName, mam.AppName).Set(x => x.uid, mam.uid));
                }
                else if (model.CountDataList.Find(x => x.DataType == item.Key) == null)
                {
                    wccmCollection.UpdateOne(x => x.uniacid.Equals(mam.uniacid), Builders<WeChatCountModel>.Update.Push(x => x.CountDataList, new WeChatCountDataModel
                    {
                        DataType = item.Key,
                        CountData = jsonData
                    }).Set(x => x.LastChangeTime, DateTime.Now).Set(x => x.AppName, mam.AppName).Set(x => x.uid, mam.uid));
                }
                else
                {
                    var filter = Builders<WeChatCountModel>.Filter;
                    wccmCollection.UpdateOne(filter.Eq(x => x.uniacid, mam.uniacid) & filter.Eq("CountDataList.DataType", item.Key), Builders<WeChatCountModel>.Update.Set("CountDataList.$.CountData", jsonData).Set(x => x.LastChangeTime, DateTime.Now).Set(x => x.AppName, mam.AppName).Set(x => x.uid, mam.uid));
                }
            }
            catch (Exception e)
            {
                e.Save();
            }
        }



        private Dictionary<WeChatCountType, string> GetWeChatRequestData()
        {
            var wcd = new Dictionary<WeChatCountType, string> {
                { WeChatCountType.昨天的概况,"https://api.weixin.qq.com/datacube/getweanalysisappiddailysummarytrend?access_token=" },{ WeChatCountType.昨天的趋势,"https://api.weixin.qq.com/datacube/getweanalysisappiddailyvisittrend?access_token="},{ WeChatCountType.上周的趋势,"https://api.weixin.qq.com/datacube/getweanalysisappidweeklyvisittrend?access_token="},{ WeChatCountType.二周前的趋势,"https://api.weixin.qq.com/datacube/getweanalysisappidweeklyvisittrend?access_token="},{ WeChatCountType.三周前的趋势,"https://api.weixin.qq.com/datacube/getweanalysisappidweeklyvisittrend?access_token="},{ WeChatCountType.四周前的趋势,"https://api.weixin.qq.com/datacube/getweanalysisappidweeklyvisittrend?access_token="},
                { WeChatCountType.上个月的趋势,"https://api.weixin.qq.com/datacube/getweanalysisappidmonthlyvisittrend?access_token="},{ WeChatCountType.昨天的分布,"https://api.weixin.qq.com/datacube/getweanalysisappidvisitdistribution?access_token="},{ WeChatCountType.昨天的留存,"https://api.weixin.qq.com/datacube/getweanalysisappiddailyretaininfo?access_token="},{ WeChatCountType.上周的留存,"https://api.weixin.qq.com/datacube/getweanalysisappidweeklyretaininfo?access_token="},{ WeChatCountType.二周前的留存,"https://api.weixin.qq.com/datacube/getweanalysisappidweeklyretaininfo?access_token="},{ WeChatCountType.三周前的留存,"https://api.weixin.qq.com/datacube/getweanalysisappidweeklyretaininfo?access_token="},{ WeChatCountType.四周前的留存,"https://api.weixin.qq.com/datacube/getweanalysisappidweeklyretaininfo?access_token="},{ WeChatCountType.上个月的留存,"https://api.weixin.qq.com/datacube/getweanalysisappidmonthlyretaininfo?access_token="},{ WeChatCountType.昨天的页面,"https://api.weixin.qq.com/datacube/getweanalysisappidvisitpage?access_token="},
                { WeChatCountType.昨天的画像,"https://api.weixin.qq.com/datacube/getweanalysisappiduserportrait?access_token="},
                { WeChatCountType.上周的画像,"https://api.weixin.qq.com/datacube/getweanalysisappiduserportrait?access_token="},
                { WeChatCountType.上月的画像,"https://api.weixin.qq.com/datacube/getweanalysisappiduserportrait?access_token="}
            };

            return wcd;
        }

        private IMongoCollection<MerchantAppModel> GetMAMCollection()
        {
            return mongo.GetMongoCollection<MerchantAppModel>();
        }






        #region 微信统计接口访问
        /// <summary>
        /// 获取画像统计
        /// </summary>
        /// <param name="wc"></param>
        /// <param name="url"></param>
        /// <param name="accessToken"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        private string GetWeChatCountPortrait(WebClient wc, string url, string accessToken, int num)
        {
            string startDate = "", endData = "";
            switch (num)
            {
                case 5:
                    startDate = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                    endData = startDate;
                    break;
                case 50:
                    startDate = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
                    endData = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                    break;
                case 51:
                    startDate = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
                    endData = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                    break;
                default:
                    break;
            }
            return GetData(wc, accessToken, url, startDate, endData);
        }
        /// <summary>
        /// 获取今日统计
        /// </summary>
        /// <param name="url"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private string GetWeChatCountYesterDay(WebClient wc, string url, string accessToken)
        {

            var date = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            return GetData(wc, accessToken, url, date, date);

        }
        private string GetData(WebClient wc, string accessToken, string url, string startDate, string endDate)
        {
            try
            {
                var reObj = new
                {
                    begin_date = startDate,
                    end_date = endDate
                };
                var json = "";
                var response = wc.UploadStringTaskAsync(url + accessToken, JsonConvert.SerializeObject(reObj));
                json = response.Result;
                return json;
            }
            catch (Exception)
            {
                return null;
            }

        }
        /// <summary>
        /// 获取周统计
        /// </summary>
        /// <param name="url"></param>
        /// <param name="accessToken"></param>
        /// <param name="weekNum">周的个数最大值4 （1 2 3 4）</param>
        /// <returns></returns>
        private string GetWeChatCountLastWeak(WebClient wc, string url, string accessToken, int weekNum)
        {
            var currentDateTime = DateTime.Now;
            var currentDayOfWeek = Convert.ToInt32(currentDateTime.DayOfWeek.ToString("d"));
            var currentStartWeek = currentDateTime.AddDays(1 - ((currentDayOfWeek == 0) ? 7 : currentDayOfWeek));
            var currentEndWeek = currentStartWeek.AddDays(6);
            var lastStartWeek = currentStartWeek.AddDays(-7 * weekNum);
            var lastEndWeek = currentEndWeek.AddDays(-7 * weekNum);

            var dateStart = lastStartWeek.ToString("yyyyMMdd");
            var dateEnd = lastEndWeek.ToString("yyyyMMdd");
            return GetData(wc, accessToken, url, dateStart, dateEnd);
        }
        /// <summary>
        /// 获取上月统计
        /// </summary>
        /// <param name="url"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        private string GetWeChatCountLastMonth(WebClient wc, string url, string accessToken)
        {
            var currentDateTime = DateTime.Now;


            var currentStartMonth = currentDateTime.AddDays(1 - currentDateTime.Day);
            var currentEndMonth = currentStartMonth.AddMonths(1).AddDays(-1);
            var lastStartMonth = currentStartMonth.AddMonths(-1);
            var lastEndMonth = currentStartMonth.AddDays(-1);

            var dateStart = lastStartMonth.ToString("yyyyMMdd");
            var dateEnd = lastEndMonth.ToString("yyyyMMdd");
            return GetData(wc, accessToken, url, dateStart, dateEnd);
        }
        #endregion
    }
}
public class WeChatCountBaseJsonModel
{
    public List<WeChatCountBaseJsonItemModel> list
    { get; set; }
}

public class WeChatCountBaseJsonItemModel
{
    public string ref_date { get; set; }
    public int visit_total { get; set; }
    public int share_pv { get; set; }
    public int share_uv { get; set; }
}