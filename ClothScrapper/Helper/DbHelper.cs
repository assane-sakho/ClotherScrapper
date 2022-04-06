using ClothScrapper.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClothScrapper
{
    class DbHelper
    {
        private static DbHelper _instance;
        MongoClient dbClient;
        IMongoDatabase db;

        private DbHelper()
        {
            dbClient = new MongoClient("mongodb://127.0.0.1:27017");
            db = dbClient.GetDatabase("clother");
        }

        public string AddCloth(Cloth cloth)
        {
            var cloths = db.GetCollection<BsonDocument>("cloths");

            var doc = new BsonDocument
            {
                {"price", cloth.Price},
                {"brand", cloth.Brand},
                {"category", cloth.Category},
                {"image", cloth.Image.Split("/").Last()}
            };

            cloths.InsertOne(doc);
            return doc["_id"].ToString();
        }

        public static DbHelper GetInstance()
        {
            if (_instance == null) ;
            _instance = new DbHelper();
            return _instance;
        }
    }
}
