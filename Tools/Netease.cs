using Newtonsoft.Json.Linq;
using Otaku16.Model;

namespace Otaku16.Tools
{
    public class Netease
    {
        /// <summary>
        /// 自动填充歌曲信息，根据提供的网易云音乐URL获取歌曲详情，并填充到Post对象中。
        /// </summary>
        /// <param name="post">需要填充信息的Post对象。</param>
        /// <param name="url">网易云音乐歌曲的URL。</param>
        /// <returns>填充完信息的Post对象。</returns>
        public static async Task<Post> AutoFill(Post post, string url)
        {
            // 从URL中解析出查询参数
            Uri uri = new Uri(url);
            string query = uri.Query;
            System.Collections.Specialized.NameValueCollection queryParameters =
                System.Web.HttpUtility.ParseQueryString(query);
            string? id = queryParameters["id"];

            // 如果没有解析出id，则直接返回原始的Post对象
            if (id is null) return post;

            // 设置帖子的链接
            post.Link = $"https://music.163.com/song?id={id}";

            // 使用HttpClient获取歌曲详情
            HttpClient client = new HttpClient();
            var s = await client.GetAsync($"http://music.163.com/api/song/detail/?id={id}&ids=%5B{id}%5D");
            var jo = JObject.Parse(s.Content.ReadAsStringAsync().Result);

            // 尝试解析歌曲标题
            try
            {
                post.Title = jo["songs"][0]["name"].ToString();

                // 如果有翻译的歌曲名，则添加到标题中
                try
                {
                    if (jo["songs"][0]["transName"] != null)
                        if (jo["songs"][0]["transName"].ToString() != "")
                            post.Title += $"({jo["songs"][0]["transName"]})";
                }
                catch { }

            }
            catch { }

            // 尝试解析歌手信息
            try
            {
                string singer = "";
                var list = jo["songs"][0]["artists"].ToList();
                foreach (var item in list)
                {
                    singer += item["name"].ToString() + "、";
                }
                // 如果没有歌手信息，则设置作者为null
                if (singer == "")
                    post.Author = null;
                else if (singer.Length > 1)
                {
                    // 去除最后一个字符（多余的逗号）并设置作者
                    singer = singer[..(singer.Length - 1)];
                    post.Author = singer;
                }
                else
                    post.Author = null;
            }
            catch { }

            // 尝试解析专辑信息
            try
            {
                post.Album = jo["songs"][0]["album"]["name"]?.ToString();
            }
            catch { }

            return post;
        }
    }
}