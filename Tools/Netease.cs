using Newtonsoft.Json.Linq;
using Otaku16.Data;

namespace Otaku16.Tools
{
    public class Netease
    {
        public static int GetIdFromUrl(string url)
        {
            if (!url.Contains("https://music.163.com/song")) return 0;
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
            try
            {
                post.Title = jo["songs"][0]["name"].ToString();
            }
            catch { }
            try
            {
                post.Author = jo["songs"][0]["artists"][0]["name"]?.ToString();
            }
            catch { }
            try
            {
                post.Album = jo["songs"][0]["album"]["name"]?.ToString();
            }
            catch { }
            return post;
        }
    }
}