using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Otaku16.Data;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Otaku16.Service
{
    public class History
    {
        public Dictionary<long, HistoryTable> Data;
        public History()
        {
            if (!File.Exists("./his.json"))
            {
                //初始数据
                Data = new();
                //解析成json
                var jo = JObject.FromObject(Data);
                var content = jo.ToString();
                //写入文件
                File.WriteAllText("./his.json", content);
            }
            else
            {
                var str = File.ReadAllText("./his.json");
                var jobj = JObject.Parse(str);
                Data = jobj.ToObject<Dictionary<long, HistoryTable>>() ?? throw new ArgumentNullException("历史文件错误");
            }
           
        }
        //保存
        public void Save()
        {
            var jo = JObject.FromObject(Data);
            var content = jo.ToString();
            //写入文件
            File.WriteAllText("./his.json", content);
        }
        public long GetIDByMessageID(int msgId)
        {
            foreach (var kvp in Data)
            {
                if (kvp.Value.GroupMessageID == msgId) return kvp.Key;
            }
            return -1;
        }
        //TODO 
        public List<HistoryTable> GetAllUnaduitPosts()
        {
            var list = new List<HistoryTable>();
            foreach (var kvp in Data)
            {
                if (kvp.Value.Passed is null) list.Add(kvp.Value);
            }
            return list;
        }
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
