//using System.Text.Json;

//namespace NoSQLProject.Other
//{
//    public static class SessionExtensions
//    {
//        public static void SetObject<T>(this ISession session, string key, T value)
//        {
//            session.SetString(key, JsonSerializer.Serialize(value));
//        }

//        public static T? GetObject<T>(this ISession session, string key)
//        {
//            string? value = session.GetString(key);
//            return value == null ? default(T) : JsonSerializer.Deserialize<T>(value);


//        }
//    }
//}
using Microsoft.AspNetCore.Http;
//using MongoDB.Bson.IO;
using Newtonsoft.Json;
namespace NoSQLProject.Other
{
public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value)
    {
        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        session.SetString(key, JsonConvert.SerializeObject(value, settings));
    }

    public static T? GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        if (value == null) return default;
        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        return JsonConvert.DeserializeObject<T>(value, settings);
    }
}
}
//                 return View(model); // Return to login view with model