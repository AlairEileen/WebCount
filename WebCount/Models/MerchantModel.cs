using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Date;
using Tools.Models;

namespace WebCount.Models
{
    public class MerchantModel : SurperModel
    {
        public int uid { get; set; }
        [BsonIgnore]
        public string username { get; set; }
        [BsonIgnore]
        public long starttime { get; set; }
        [BsonIgnore]
        public List<MerchantAppModel> AppModels { get; set; }
        public string MerchatName { get => merchatName ?? username; set => merchatName = value; }

        private string merchatName;
        private DateTime lastChangeTime = DateTime.Now;
        [JsonConverter(typeof(Tools.Response.Json.DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastChangeTime { get => lastChangeTime; set => lastChangeTime = value; }
        private DateTime createTime;
        [JsonConverter(typeof(Tools.Response.Json.DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime
        {
            get
            {
                if (createTime == null)
                {
                    createTime = starttime.ToUnixDateTime();
                }
                return createTime;
            }
            set { createTime = value; }
        }

    }

    public class MerchantAppModel : BaseModel
    {
        public int uid { get; set; }
        [BsonIgnore]
        public string key { get; set; }
        [BsonIgnore]
        public string secret { get; set; }
        [BsonIgnore]
        public string name { get; set; }
        public string AccessToken { get; set; }
        public string AppID { get => appID ?? key; set => appID = value; }
        public string AppSecret { get => appSecret ?? secret; set => appSecret = value; }
        public string AppName { get => appName ?? name; set => appName = value; }
        public int TimeOutLength { get; set; }

        [JsonConverter(typeof(Tools.Response.Json.DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastRefreshTime { get => lastRefreshTime; set =>lastRefreshTime = value; }

        private string appID;
        private string appSecret;
        private string appName;

        private DateTime lastRefreshTime = DateTime.Now;

    }
}
