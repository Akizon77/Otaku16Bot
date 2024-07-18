using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otaku16
{
    public static class Const
    {
        public static string version = "1.3.3";
        public static string about = $"音乐投稿机器人 by @AkizonChan - <code>{version}</code>";
        public static string HTMLEscape(this string text)
        {
            text = text.Replace("&", "&amp;");
            text = text.Replace("<", "&lt;");
            text = text.Replace(">", "&gt;");
            text = text.Replace("\"", "&quot;");
            //text = text.Replace("\'", "&apos;");
            return text;
        }
    }
    
}
