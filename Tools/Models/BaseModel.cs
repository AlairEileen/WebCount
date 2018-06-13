using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.Models
{
    public class BaseModel : SurperModel
    {
        public string uniacid { get; set; }
        [JsonConverter(typeof(Tools.Response.Json.DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateTime { get => createTime; set => createTime = value; }
        [JsonConverter(typeof(Tools.Response.Json.DateConverterEndMinute))]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastChangeTime { get => lastChangeTime; set => lastChangeTime = value; }
        private DateTime createTime = DateTime.Now;
        private DateTime lastChangeTime = DateTime.Now;

    }
    public class SurperModel
    {
        [BsonId]
        [JsonConverter(typeof(Tools.Response.Json.ObjectIdConverter))]
        public ObjectId ID { get; set; }

    }

}
