using Newtonsoft.Json.Linq;
using Otaku16.Model;
using System.Text.RegularExpressions;

namespace Otaku16.Tools
{
    public class QQMusic
    {
        /// <summary>
        /// 自动填充歌曲信息。
        /// </summary>
        /// <param name="post">文章对象，包含初始或部分信息。</param>
        /// <param name="url">文章链接，用于提取更多信息。</param>
        /// <returns>填充完整信息后的文章对象。</returns>
        public static Post AutoFill(Post post, string url)
        {
            // 如果链接不在y.qq.com域名下，直接返回原文章对象
            if (!url.Contains("y.qq.com"))
            {
                return post;
            }

            // 解析URL获取查询参数
            Uri uri = new Uri(url);
            string query = uri.Query;
            System.Collections.Specialized.NameValueCollection queryParameters =
                System.Web.HttpUtility.ParseQueryString(query);
            string? id = queryParameters["songmid"];
            // 如果没有提取到songmid，返回原文章对象
            if (id is null) return post;

            // 设置文章链接
            post.Link = $"https://i.y.qq.com/v8/playsong.html?songmid={id}";

            // 定义正则表达式，用于从页面源码中提取数据
            string pattern = @"<\/div><script crossorigin=""anonymous"">window\.__ssrFirstPageData__ =([\s\S]*?)<\/script>";

            // 发起HTTP请求获取页面源码
            HttpClient httpClient = new();
            var body = httpClient.GetStringAsync(url).Result;

            // 使用正则表达式匹配所需数据
            MatchCollection matches = Regex.Matches(body, pattern);
            JObject jobj = new();

            try
            {
                // 提取并解析匹配到的数据
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

            // 尝试从解析的数据中提取文章标题
#pragma warning disable CS8602
            try
            {
                post.Title = jobj["songList"][0]["title"].ToString();
            }
            catch { }
            // 尝试从解析的数据中提取歌手信息
            try
            {
                string singer = "";
                var list = jobj["songList"][0]["singer"].ToList();
                foreach (var item in list)
                {
                    singer += item["title"] + "、";
                }
                // 如果没有歌手信息，设置作者为null
                if (singer == "") 
                    post.Author = null;
                else if(singer.Length > 1)
                {
                    // 去除最后一个字符（多余的逗号）
                    singer = singer[..(singer.Length - 1)];
                    post.Author = singer;
                }
                else
                    post.Author = null;
            }
            catch { }
            // 尝试从解析的数据中提取专辑信息
            try
            {
                if (jobj["songList"][0]["album"]["title"].ToString() == "默认专辑")
                    post.Album = "单曲";
                else if (string.IsNullOrEmpty(jobj["songList"][0]["album"]["title"].ToString()))
                    post.Album = "单曲";
                else
                    post.Album = jobj["songList"][0]["album"]["title"].ToString();
               
            }
            catch { }
#pragma warning restore CS8602

            // 返回填充完整信息的文章对象
            return post;
        }
    }
}
