using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Otaku16.Model
{
    public class Post : SqlReopBase
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }
        [SugarColumn(IsNullable = true)]
        public bool? Passed { get; set; }
        public long UserID { get; set; }
        [SugarColumn(IsNullable = true)]
        public int? GroupMessageID { get; set; }
        [SugarColumn(IsNullable = true)]
        public int? ChannelMessageID { get; set; }
        [SugarColumn(IsNullable = true)]
        public string? UserName { get; set; }
        [SugarColumn(IsNullable = true)]
        public string? Title { get; set; }
        [SugarColumn(IsNullable = true)]
        public string? Author { get; set; }
        [SugarColumn(IsNullable = true)]
        public string? Album { get; set; }
        [SugarColumn(IsNullable = true)]
        public string? FileID { get; set; }
        [SugarColumn(IsNullable = true)]
        public string? Link { get; set; }
        [SugarColumn(IsNullable = true)]
        public string? Tag { get; set; }
        [SugarColumn(IsNullable = true,Length = -1)]
        public string? Comment { get; set; }
        public long Timestamp { get; set; }
        [SugarColumn(IsNullable = true)]
        public bool? Anonymous { get; set; }
        public override string ToString()
        {
            var name = Tools.Telegram.GetName(UserID);
            long userid = 0;
            var success = long.TryParse(name,out userid);
            if (success)
            {
                name = UserName;
            }
            return $"投稿人: {(Anonymous ?? false ? "匿名" : name)}\n" +
            $"歌曲名: {Title?.HTMLEscape()}\n" +
            $"艺术家名: {Author?.HTMLEscape()}\n" +
            $"专辑: {Album?.HTMLEscape()}\n" +
            $"Tag: #{Tag?.HTMLEscape()}\n" +
            $"附言: {Comment?.HTMLEscape()}" +
            $"{(Link is null ? "" : "\n链接: " + $"<a href=\"{Link}\">[跳转]</a>")}"; ;
        }
    }
}
