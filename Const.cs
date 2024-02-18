using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otaku16
{
    public static class Const
    {
        public static string version = "1.1.8";
        public static string updateLog = $"\n" +
            $"1.1.8 更新日志：\r\n\r\n- 新增 QQ音乐 自动识别歌曲信息\r\n- 文件投稿可以删除“修改标题”等按钮\r\n- 新增按钮，如曲目是单曲可直接点击按钮\r\n- 修复了网易云链接识别艺术家时多位艺术家只自动填充一位作曲家的问题" +
            $"";
        public static string about = $"音乐投稿机器人 by @AkizonChan - <code>{version}</code> {updateLog}";
    }
}
