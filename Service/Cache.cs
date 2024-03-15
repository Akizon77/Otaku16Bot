using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otaku16.Tools;
using System.Diagnostics.CodeAnalysis;
using Otaku16.Model;

namespace Otaku16.Service
{
    public class Cache
    {
        public Dictionary<long, Post> Data;
        public Cache()
        {
            if (!File.Exists("./cache.json"))
            {
                //初始数据
                Data = new();
                //解析成json
                var jo = JObject.FromObject(Data);
                var content = jo.ToString();
                //写入文件
                File.WriteAllText("./cache.json", content);
            }
            else
            {
                var str = File.ReadAllText("./cache.json");
                var jobj = JObject.Parse(str);
                Data = jobj.ToObject<Dictionary<long, Post>>() ?? throw new ArgumentNullException("缓存文件错误");
            }
        }

        public static Dictionary<long, Post> GetCache()
        {
            return Hosting.GetService<Cache>().Data;
        }
        /// <summary>
        /// 保存数据到文件中。
        /// 此方法会首先清除过期的数据，然后将剩余的数据写入到名为"cache.json"的文件中。
        /// </summary>
        public void Save()
        {
            // 清除超时数据
            foreach (var kvp in Data)
            {
                var createdTime = TimeStamp.Prase(kvp.Value.Timestamp);
                // 如果数据创建时间超过一天，则将其从Data字典中移除
                if (DateTime.UtcNow > createdTime.AddDays(1))
                {
                    Data.Remove(kvp.Key);
                }
            }
            // 将Data字典转换为JSON对象
            var jo = JObject.FromObject(Data);
            // 将JSON对象转换为字符串
            var content = jo.ToString();
            // 写入文件
            File.WriteAllText("./cache.json", content);
        }
    }

}
