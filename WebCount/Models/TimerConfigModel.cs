using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tools.DB;
using Tools.Models;
using Tools.Response.Json;

namespace WebCount.Models
{
    public class TimerConfigModel : SurperModel
    {
        [JsonConverter(typeof(DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get; set; } = DateTime.Now;
        [JsonConverter(typeof(DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastExecuteTime { get; set; } = DateTime.Now;
        public int RefreshMerchantStartHour { get; set; } = 3;
        public int RefreshMerchantStartMinutes { get; set; } = 3;

        public int RefreshWeChatCountStartHour { get; set; } = 8;
        public int RefreshWeChatCountStartMinutes { get; set; } = 13;
        public RefreshDate MerchantRefreshDate { get; set; }
        public RefreshDate MiniAppRefreshDate { get; set; }
        public RefreshDate WeChatCountRefreshDate { get; set; }
    }
    public class RefreshDate
    {
        [JsonConverter(typeof(DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastRefreshEndTime { get; set; }
        [JsonConverter(typeof(DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastRefreshStartTime { get; set; }
        public long RefreshUseSeconds { get; set; }
    }
    public class TimerConfigModelContext : MongoDBModel<TimerConfigModel>
    {
        private static TimerConfigModelContext tcmc;
        private TimerConfigModelContext()
        {

        }
        private static TimerConfigModelContext ConfigModel
        {
            get
            {
                if (tcmc == null)
                {
                    tcmc = new TimerConfigModelContext();
                }
                return tcmc;
            }
        }
        public static TimerConfigModel GetConfig()
        {
            var model = ConfigModel.GetCollection().Find(ConfigModel.Filter.Empty).FirstOrDefault();
            if (model == null)
            {
                model = new TimerConfigModel();
                ConfigModel.GetCollection().InsertOne(model);
            }
            return model;
        }

        public static void SaveConfigLog(TimerConfigModel timerConfigModel)
        {
            ConfigModel.GetCollection().UpdateOne(x => x.ID.Equals(timerConfigModel.ID), ConfigModel.Update.Set(x => x.CreateTime, DateTime.Now)
                .Set(x => x.RefreshMerchantStartHour, timerConfigModel.RefreshMerchantStartHour)
                .Set(x => x.RefreshMerchantStartMinutes, timerConfigModel.RefreshMerchantStartMinutes)
                .Set(x => x.RefreshWeChatCountStartHour, timerConfigModel.RefreshWeChatCountStartHour)
                .Set(x => x.RefreshWeChatCountStartMinutes, timerConfigModel.RefreshWeChatCountStartMinutes));


        }
    }
}
