using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Otaku16.Tools
{
    public class Config
    {
        [JsonIgnore]
        private static bool Loaded = false;
        [AllowNull]
        private static Config _instance;
        public void AddAdmin(long userid)
        {
            Admins.Add(userid);
        }
        public void DelAdmin(long userid)
        {
            Admins.Remove(userid);
        }
        public bool IsOwner(long id)
        {
            if (Owner == id) return true;
            return false;
        }
        public bool IsAdmin(long id)
        {
            return Admins.Contains(id);
        }
        public void Save()
        {
            var jo = JObject.FromObject(this);
            var content = jo.ToString();
            //写入文件
            File.WriteAllText("./config.json", content);
        }
        public static Config GetConfig()
        {
            if (Loaded) return _instance;
            if (!File.Exists("./config.json"))
            {
                //初始数据
                _instance = new Config()
                {
                    Debug = false,
                    Proxy = false,
                    Socks5 = "socks5://127.0.0.1:12612",
                    Telegram = new ()
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
                var jo = JObject.FromObject(_instance);
                var content = jo.ToString();
                //写入文件
                File.WriteAllText( "./config.json",content);
                return _instance;
            }
            var str = File.ReadAllText("./config.json");
            var jobj = JObject.Parse(str);
            _instance = jobj.ToObject<Config>();
            return _instance ?? throw new NullReferenceException("处理配置文件出错");
        }
        //结构
        [JsonProperty("debug")]
        public bool Debug { get; set; }
        [JsonProperty("proxy")]
        public bool Proxy { get; set; }
        [JsonProperty("socks5")]
        public string Socks5 { get; set; }
        [JsonProperty("telegram")]
        public Telegram Telegram { get; set; }
        [JsonProperty("owner")]
        public long Owner {  get; set; }
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
