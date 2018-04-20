using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using Tools.DB;

namespace WebCommon
{
    public abstract class BaseData<T>
    {
        protected MongoDBTool mongo;
        protected IMongoCollection<T> collection;
        protected IMongoDatabase mongoDB;
        protected FilterDefinitionBuilder<T> Filter { get => Builders<T>.Filter; }
        protected UpdateDefinitionBuilder<T> Update { get => Builders<T>.Update; }
        protected BaseData()
        {
            mongo = new MongoDBTool();
            mongoDB = mongo.GetMongoDatabase();
            collection = mongo.GetMongoCollection<T>();
        }

        protected BaseData(string collectionName)
        {
            mongo = new MongoDBTool();
            mongoDB = mongo.GetMongoDatabase();
            collection = mongo.GetMongoCollection<T>(collectionName);
        }

        protected T GetModelByID(ObjectId objectId)
        {
            return collection.Find(Filter.Eq("_id", objectId)).FirstOrDefault();
        }
        protected T GetModelByIDAndUniacID(ObjectId objectId, string uniacid)
        {
            return collection.Find(GetModelIDAndUniacIDFilter(objectId, uniacid)).FirstOrDefault();
        }

        protected List<T> GetAllModel()
        {
            return collection.Find(Builders<T>.Filter.Empty).ToList();
        }
        protected FilterDefinition<T> GetModelIDAndUniacIDFilter(ObjectId objectId, string uniacid)
        {
            return Filter.Eq("_id", objectId) & Filter.Eq("uniacid", uniacid);
        }


        //protected List<T> GetPageList(string uniacid,int pageIndex,string queryStr)
        //{
        //    var pageSize = 12;
        //    var tvFind = collection.Find(Filter.Eq(x => x.uniacid, uniacid));
        //    var tvList = tvFind.Skip(pageSize * pageIndex).Limit(pageSize).ToList();
        //    return tvList;
        //}
        
    }
}
