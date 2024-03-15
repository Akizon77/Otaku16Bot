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
        [SugarColumn(IsNullable = true)]
        public string? Comment { get; set; }
        public long Timestamp { get; set; }
        [SugarColumn(IsNullable = true)]
        public bool? Anonymous { get; set; }
        public override string ToString()
        {
            return $"投稿人: {(Anonymous ?? false ? "匿名" : UserName)}\n" +
            $"歌曲名: {Title}\n" +
            $"艺术家名: {Author}\n" +
            $"专辑: {Album}\n" +
            $"Tag: #{Tag}\n" +
            $"附言: {Comment}" +
            $"{(Link is null ? "" : "\n链接: " + Link)}"; ;
        }
    }
}
