using Newtonsoft.Json;
using System;

namespace FileUtility
{
    public static class JsonHelper
    {
        public static string Serialize(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj);
            }
            catch (Exception ex)
            { 
                throw new Exception(ex.ToString());
            }
        }

        public static T Deserialize<T>(string json) 
        {
            try
            {
                return (T)JsonConvert.DeserializeObject(json);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
