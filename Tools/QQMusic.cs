using Newtonsoft.Json.Linq;
using Otaku16.Data;
using System.Text.RegularExpressions;

namespace Otaku16.Tools
{
    public class QQMusic
    {
        public static Post AutoFill(Post post,string url)
        {
            if (!url.StartsWith("https://i.y.qq.com"))
            {
                return post;
            }
            string pattern = @"<\/div><script crossorigin=""anonymous"">window\.__ssrFirstPageData__ =([\s\S]*?)<\/script>";
            // 要提取的字符串
            HttpClient httpClient = new();
            var body = httpClient.GetStringAsync(url).Result;

            // 使用正则表达式进行匹配
            MatchCollection matches = Regex.Matches(body, pattern);
            JObject jobj = new();

            try
            {
                var l1 = "</div><script crossorigin=\"anonymous\">window.__ssrFirstPageData__ =".Length;
                var l2 = "</script>".Length;
                foreach (Match match in matches)
                {
                    if (match.Value.Contains("songList"))
                    {
                        var content = match.Value[l1..(match.Value.Length - l2)];
                        jobj = JObject.Parse(content);
                        break;
                    }
                }
            }
            catch { }
#pragma warning disable CS8602
            try
            {
                post.Title = jobj["songList"][0]["title"].ToString();
            }
            catch { }
            try
            {
                string singer = "";
                var list = jobj["songList"][0]["singer"].ToList();
                foreach (var item in list)
                {
                    singer += item["title"] + "、";
                }
                if (singer == "") 
                    post.Author = null;
                else if(singer.Length > 1)
                {
                    singer = singer[..(singer.Length - 1)];
                    post.Author = singer;
                }
                else
                    post.Author = null;
            }
            catch { }
            try
            {
                if (jobj["songList"][0]["album"]["title"].ToString() == "默认专辑")
                    post.Album = "单曲";
                else
                {
                    post.Album = jobj["songList"][0]["album"]["title"].ToString();
                }
               
            }
            catch { }
#pragma warning restore CS8602
            return post;
        }
    }
}
