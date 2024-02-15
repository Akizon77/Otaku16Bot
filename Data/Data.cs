using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otaku16.Data
{
    public struct Post
    {
        public string UserName;
        public string? Title;
        public string? Author;
        public string? Album;
        public string? FileID;
        public string? Link;
        public string? Tag;
        public string? Comment;
        public long Timestamp;
        public bool? Anonymous;
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
