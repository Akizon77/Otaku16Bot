using Otaku16.Repos;
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
        private static Options Opt => Hosting.GetService<Options>();
        private static PostRepo Posts => Hosting.GetService<PostRepo>();
        
                /// <summary>
        /// 获取指定页面的帖子链接文本
        /// </summary>
        /// <param name="page">要获取的页面编号</param>
        /// <returns>返回一个字符串，包含该页面指定范围内的帖子链接</returns>
        public static string GetPage(int page)
        {
            // 从数据库中查询所有未通过审核的帖子
            var posts = Posts.Queryable().Where(x => x.Passed == null).ToList();
            string text = "";
            int i = 1;
            // 遍历所有帖子，将属于指定页面范围的帖子链接添加到文本中
            posts.ForEach(x =>
            {
                if (page * 10 < i && i <= (page + 1) * 10)
                {
                    // 如果帖子在指定的页面范围内，将其链接以特定格式添加到文本中
                    text += $"<a href=\"{Opt.Telegram.GroupLink}/{x.GroupMessageID}\">{i}.{x.Title}</a>\n";
                }
                i++;
            });
            return text;
        }
    }
}
