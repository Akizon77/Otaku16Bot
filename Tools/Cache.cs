using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otaku16.Tools;
using System.Diagnostics.CodeAnalysis;

namespace Otaku16
{
    public class Cache
    {
        [JsonIgnore]
        private static bool Loaded = false;
        [AllowNull]
        private static Cache _instance;

        public static Cache GetCache()
        {
            if (Loaded) return _instance;
            if (!File.Exists("./cache.json"))
            {
                //初始数据
                _instance = new Cache()
                {
                    Caches = new()
                };
                //解析成json
                var jo = JObject.FromObject(_instance);
                var content = jo.ToString();
                //写入文件
                File.WriteAllText("./cache.json", content);
                return _instance;
            }
            var str = File.ReadAllText("./cache.json");
            var jobj = JObject.Parse(str);
            _instance = jobj.ToObject<Cache>();
            return _instance ?? throw new NullReferenceException("处理缓存文件出错");
        }
        //保存
        public static void Save()
        {
            //清楚超时数据
            foreach (var kvp in _instance.Caches)
            {
                var createdTime = TimeStamp.Prase(kvp.Value.Timestamp);
                if (DateTime.UtcNow > createdTime.AddDays(1))
                {
                    _instance.Caches.Remove(kvp.Key);
                }
            }
            var jo = JObject.FromObject(_instance);
            var content = jo.ToString();
            //写入文件
            File.WriteAllText("./cache.json", content);
        }
        //结构
        [JsonProperty("caches")]
        public Dictionary<long, Post> Caches = new();
       
    }
    public struct Post
    {
        public string UserName;
        public string? Title;
        public string? Author;
        public string? Album;
        public string? FileID;
        public string? Link;
        public string? Tag;
        public string? Comment;
        public long Timestamp;
        public bool? Anonymous;
        public override string ToString()
        {
            return $"投稿人: {((Anonymous ?? false) ? "匿名" : UserName)}\n" +
            $"歌曲名: {Title}\n" +
            $"艺术家名: {Author}\n" +
            $"专辑: {Album}\n" +
            $"Tag: #{Tag}\n" +
            $"附言: {Comment}" +
            $"{(Link is null ? "" : "\n链接: " + Link)}"; ;
        }
    }
    
}
