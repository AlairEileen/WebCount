using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tools;
using Tools.Models;
using WebCommon;
using WebCount.Models;

namespace WebCount.AppDatas
{
    public class CountData : BaseData<CountModel>
    {
        internal string GetAccessToken(string uniacid, string appID, string appSecret)
        {
            var filterCountModel = Filter.Eq(x => x.AppID, appID) & Filter.Eq(x => x.AppSecret, appSecret);
            var countModel = collection.Find(filterCountModel).FirstOrDefault();
            var access_token = countModel?.AccessToken;
            var timeOut = 0;
            if (countModel == null)
            {
                GetWeChatAccessToken(ref timeOut, ref access_token, appID, appSecret);
                if (timeOut != 0 && access_token != null)
                    collection.InsertOne(new CountModel
                    {
                        uniacid = uniacid,
                        AppID = appID,
                        AccessToken = access_token,
                        AppSecret = appSecret,
                        CreateTime = DateTime.Now,
                        LastChangeTime = DateTime.Now,
                        TimeOutLength = timeOut
                    });
            }
            else if (DateTime.Now.GetDiffSeconds(countModel.LastChangeTime) >= countModel.TimeOutLength)
            {
                GetWeChatAccessToken(ref timeOut, ref access_token, appID, appSecret);
                if (timeOut != 0 && access_token != null)
                    collection.UpdateOne(filterCountModel, Update
                        .Set(x => x.AccessToken, access_token)
                        .Set(x => x.TimeOutLength, timeOut)
                        .Set(x => x.LastChangeTime, DateTime.Now));
            }
            return countModel.AccessToken;
        }

        internal string GetAccessToken(string uniacID)
        {
            var filterCountModel = Filter.Eq(x => x.uniacid, uniacID);
            var countModel = collection.Find(filterCountModel).FirstOrDefault();
            var access_token = countModel?.AccessToken;
            var timeOut = 0;


            if (countModel == null)
            {
                throw new ExceptionModel { ExceptionParam = Tools.Response.ResponseStatus.验证失败 };
            }
            else if (DateTime.Now.GetDiffSeconds(countModel.LastChangeTime) >= countModel.TimeOutLength)
            {
                GetWeChatAccessToken(ref timeOut, ref access_token, countModel.AppID, countModel.AppSecret);
                if (timeOut != 0 && access_token != null)
                    collection.UpdateOne(filterCountModel, Update
                        .Set(x => x.AccessToken, access_token)
                        .Set(x => x.TimeOutLength, timeOut)
                        .Set(x => x.LastChangeTime, DateTime.Now));
            }
            return countModel.AccessToken;
        }

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
            JToken ei;
            if (jObj.TryGetValue("expires_in", out ei))
            {
                timeOut = Convert.ToInt32(ei.ToString());
            }
        }






        internal void RefreshAccessToken(object state)
        {
            var countModelList = GetAllModel();
            foreach (var item in countModelList)
            {
                var access_token = item.AccessToken;
                var timeOut = 0;
                GetWeChatAccessToken(ref timeOut, ref access_token, item.AppID, item.AppSecret);
                collection.UpdateOne(x => x.ID.Equals(item.ID), Update
                       .Set(x => x.AccessToken, item.AccessToken)
                       .Set(x => x.TimeOutLength, timeOut)
                       .Set(x => x.LastChangeTime, DateTime.Now));
                Thread.Sleep(new Random().Next(100));
            }
        }

    }


}
