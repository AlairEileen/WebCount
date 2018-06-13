using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.Models;

namespace WebCount.Models
{
    public class WeChatCountModel : BaseModel
    {
        public string AppName { get; set; }
        public int uid { get; set; }
        public List<WeChatCountDataModel> CountDataList { get; set; }
    }

    public class WeChatCountDataModel
    {
        public WeChatCountType DataType { get; set; }
        [BsonIgnore]
        public string WeChatCountTypeName { get => DataType.ToString(); }
        public string CountData { get; set; }
    }

    public enum WeChatCountType
    {
        昨天的概况 = 0,
        昨天的趋势 = 1,
        上周的趋势 = 11,
        二周前的趋势 = 12,
        三周前的趋势 = 13,
        四周前的趋势 = 14,
        上个月的趋势 = 10,
        昨天的分布 = 2,
        昨天的留存 = 3,
        上周的留存 = 31,
        二周前的留存 = 32,
        三周前的留存 = 33,
        四周前的留存 = 34,
        上个月的留存 = 30,
        昨天的页面 = 4,
        昨天的画像 = 5,
        上周的画像 = 51,
        上月的画像 = 50
    }
    public class WeChatCountAppModel : SurperModel
    {
        public long AccountSum { get; set; }
        public long ShareSum { get; set; }
        public long ShareAccountSum { get; set; }
        public List<MerchantCountAppModel> MerchantCountAppModels { get; set; }
        private DateTime lastChangeTime = DateTime.Now;
        [JsonConverter(typeof(Tools.Response.Json.DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastChangeTime { get => lastChangeTime; set => lastChangeTime = value; }
    }

    public class MerchantCountAppModel
    {
        public int uid { get; set; }
        public string MerchantName { get; set; }
        public long AccountSum { get; set; }
        public long ShareSum { get; set; }
        public long ShareAccountSum { get; set; }
        public List<AppCountModel> AppCountModels { get; set; }
    }

    public class AppCountModel
    {
        public int uniacid { get; set; }
        public string AppName { get; set; }
        public long AccountSum { get; set; }
        public long ShareSum { get; set; }
        public long ShareAccountSum { get; set; }
    }
}
