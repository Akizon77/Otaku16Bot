using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Otaku16.Tools
{
    public class History
    {
        private static bool Loaded = false;
        [AllowNull]
        private static History _instance;
        public static History GetHistory()
        {
            if (Loaded) return _instance;
            if (!File.Exists("./his.json"))
            {
                //初始数据
                _instance = new History()
                {
                    His = new()
                };
                //解析成json
                var jo = JObject.FromObject(_instance);
                var content = jo.ToString();
                //写入文件
                File.WriteAllText("./his.json", content);
                return _instance;
            }
            var str = File.ReadAllText("./his.json");
            var jobj = JObject.Parse(str);
            _instance = jobj.ToObject<History>();
            return _instance ?? throw new NullReferenceException("处理历史记录文件出错");
        }
        //保存
        public void Save()
        {
            var jo = JObject.FromObject(this);
            var content = jo.ToString();
            //写入文件
            File.WriteAllText("./his.json", content);
        }
        public long GetIDByMessageID(int msgId)
        {
            foreach (var kvp in His)
            {
                if (kvp.Value.GroupMessageID == msgId) return kvp.Key;
            }
            return -1;
        }
        //TODO 
        public List<HistoryTable> GetAllUnaduitPosts()
        {
            var list = new List<HistoryTable>();
            foreach (var kvp in His)
            {
                if (kvp.Value.Passed is null) list.Add(kvp.Value);
            }
            return list;
        }
        [JsonProperty("history")]
        public Dictionary<long, HistoryTable> His = new();
    }
    public struct HistoryTable
    {
        public bool? Passed;
        public long UserID;
        public int GroupMessageID;
        public int ChannelMessageID;
        public Post Post;
        public override string ToString()
        {
            return Post.ToString();
        }
    }
}
