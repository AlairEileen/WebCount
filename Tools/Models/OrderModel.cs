using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Tools.DB;
using Tools.Response.Json;

namespace Tools.Models
{
    public class OrderModel<T> : BaseModel
    {
        public T OrderData { get; set; }
        /// <summary>
        /// 微信订单号
        /// </summary>
        public string WeChatOrderID { get; set; }

        public ObjectId  AccountID { get; set; }
        /// <summary>
        /// 是否支付
        /// </summary>
        public bool IsPaid { get; set; }
        /// <summary>
        /// 支付时间
        /// </summary>
        [JsonConverter(typeof(DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime PaidTime { get; set; }
       
        /// <summary>
        /// 金额
        /// </summary>
        public decimal Total { get; set; }
        /// <summary>
        /// 获取该集合
        /// </summary>
        /// <returns></returns>
        public static IMongoCollection<OrderModel<T>> GetCollection()
        {
            return new MongoDBTool().GetMongoCollection<OrderModel<T>>("OrderModel");
        }
        public OrderType OrderType { get; set; }
    }

    public enum OrderType
    {
        LessonOrder = 0
    }
}
