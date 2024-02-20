using Newtonsoft.Json.Linq;
using Otaku16.Data;

namespace Otaku16.Tools
{
    public class Netease
    {
        public static int GetIdFromUrl(string url)
        {
            if (!url.Contains("music.163.com")) return 0;
            Uri uri = new Uri(url);
            string query = uri.Query;
            System.Collections.Specialized.NameValueCollection queryParameters =
                System.Web.HttpUtility.ParseQueryString(query);
            string? idString = queryParameters["id"];
            int id = 0;
            if (int.TryParse(idString, out id))
            {
                return id;
            }
            return 0; // 如果无法解析id，则返回0或者其他默认值
        }

        public static async Task<Post> AutoFill(Post post, int id)
        {
            HttpClient client = new HttpClient();
            var s = await client.GetAsync($"http://music.163.com/api/song/detail/?id={id}&ids=%5B{id}%5D");
            var jo = JObject.Parse(s.Content.ReadAsStringAsync().Result);
#pragma warning disable CS8602 // 解引用可能出现空引用。
            try
            {
                post.Title = jo["songs"][0]["name"].ToString();
                try
                {
                    if (jo["songs"][0]["transName"] != null)
                        if (jo["songs"][0]["transName"].ToString() != "")
                            post.Title += $"({jo["songs"][0]["transName"]})";
                }
                catch { }
                
            }
            catch { }
            try
            {
                string singer = "";
                var list = jo["songs"][0]["artists"].ToList();
                foreach (var item in list)
                {
                    singer += item["name"].ToString() + "、";
                }
                if (singer == "")
                    post.Author = null;
                else if (singer.Length > 1)
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
                post.Album = jo["songs"][0]["album"]["name"]?.ToString();
            }
            catch { }
#pragma warning restore CS8602 // 解引用可能出现空引用。
            return post;
        }
    }
}