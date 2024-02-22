using Otaku16.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Otaku16.Commands
{
    public class List
    {
        private static History history => Hosting.GetService<History>();
        private static Config config => Hosting.GetService<Config>();
        public static string GetFirstTen()
        {
            var posts = history.GetAllUnaduitPosts();
            string text = "";
            int i = 1;
            posts.ForEach(x =>
            {
                if (i < 11)
                {
                    text += $"<a href=\"{config.data.Telegram.GroupLink}/{x.GroupMessageID}\">{i}.{x.Post.Title}</a>\n";
                    i++;
                }
            });
            return text;
        }
        public static string GetPage(int page)
        {
            var posts = history.GetAllUnaduitPosts();
            string text = "";
            int i = 1;
            posts.ForEach(x =>
            {
                if (page * 10 < i && i <= (page + 1) * 10)
                {
                    text += $"<a href=\"{config.data.Telegram.GroupLink}/{x.GroupMessageID}\">{i}.{x.Post.Title}</a>\n";
                }
                i++;
            });
            return text;
        }
    }
}
