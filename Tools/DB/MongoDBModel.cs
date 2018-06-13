using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tools.DB
{
    public class MongoDBModel<T>
    {
        protected MongoDBTool mdbTool;
        public MongoDBTool MdbTool
        {
            get
            {
                if (mdbTool == null)
                {
                    mdbTool = new MongoDBTool();
                }
                return mdbTool;
            }
        }

        public IMongoCollection<T> GetCollection()
        {
            return MdbTool.GetMongoCollection<T>();
        }
        public FilterDefinitionBuilder<T> Filter { get => Builders<T>.Filter; }
        public UpdateDefinitionBuilder<T> Update { get => Builders<T>.Update; }
    }
}
