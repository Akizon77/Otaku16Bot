using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otaku16
{
    public static class Const
    {
        public static string version = "1.1.7a";
        public static string updateLog = $"\n" +
            $"1.1.7 更新日志：\r\n\r\n- 新增 网易云链接自动识别歌曲信息\r\n- <code>/about</code> 添加 完整更新日志按钮\r\n- 链接投稿时使用音频文件通过稿件时移除审核框\r\n- 大幅修改代码模式（可能有新的特性）\r\n- 修复了重复回复音频文件导致稿件可以一直被通过的问题\r\n- 对Owner新增投稿类型：每日推荐\r\n- 修复所有链接都会被识别网易云的问题\r\n\r\n" +
            $"";
        public static string about = $"音乐投稿机器人 by @AkizonChan - <code>{version}</code> {updateLog}";
    }
}
