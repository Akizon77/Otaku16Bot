using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Otaku16.Service
{
    public class Config
    {
        public Data data;
        public void AddAdmin(long userid)
        {
            data.Admins.Add(userid);
        }
        public void DelAdmin(long userid)
        {
            data.Admins.Remove(userid);
        }
        public bool IsOwner(long id)
        {
            if (data.Owner == id) return true;
            return false;
        }
        public bool IsAdmin(long id)
        {
            return data.Admins.Contains(id);
        }
        public void Save()
        {
            var jo = JObject.FromObject(data);
            var content = jo.ToString();
            //写入文件
            File.WriteAllText("./config.json", content);
        }
        public Config()
        {
            if (!File.Exists("./config.json"))
            {
                //初始数据
                data = new Data()
                {
                    Debug = false,
                    Proxy = false,
                    Socks5 = "socks5://127.0.0.1:12612",
                    Telegram = new()
                    {
                        Token = "YOUR_BOT_TOKEN_HERE",
                        ChannelID = 0,
                        GroupID = 0,
                        ChannelLink = "",
                    },
                    Owner = 1977354088,
                    Admins = new List<long>()
                };
                //解析成json
                var jo = JObject.FromObject(data);
                var content = jo.ToString();
                //写入文件
                File.WriteAllText("./config.json", content);
            }
            else
            {
                var str = File.ReadAllText("./config.json");
                Console.WriteLine($"DEBUG {str}");
                var jobj = JObject.Parse(str);
                var d = jobj.ToObject<Data>();
                data = d;
            }
        }
        //结构
        public struct Data
        {
            [JsonProperty("debug")]
            public bool Debug { get; set; }
            [JsonProperty("proxy")]
            public bool Proxy { get; set; }
            [JsonProperty("socks5")]
            public string Socks5 { get; set; }
            [JsonProperty("telegram")]
            public Telegram Telegram { get; set; }
            [JsonProperty("owner")]
            public long Owner { get; set; }
            [JsonProperty("admins")]
            public List<long> Admins { get; set; }
        }
        public struct Telegram
        {
            [JsonProperty("token")]
            public string Token;
            [JsonProperty("channel")]
            public long ChannelID;
            [JsonProperty("group")]
            public long GroupID;
            [JsonProperty("channel_link")]
            public string ChannelLink;
        }

    }
    
   
}
