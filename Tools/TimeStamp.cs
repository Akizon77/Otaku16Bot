using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otaku16.Tools
{
    public class TimeStamp
    {
        /// <summary>
        /// 获取当前时间的Unix时间戳（毫秒）
        /// </summary>
        /// <returns>当前时间的Unix时间戳，以毫秒为单位</returns>
        public static long GetNow()
        {
            // 获取当前的UTC时间
            DateTime currentTime = DateTime.UtcNow;
            // 将当前时间转换为Unix时间戳（毫秒）
            long timestamp = ((DateTimeOffset)currentTime).ToUnixTimeMilliseconds();
            return timestamp;
        }
        /// <summary>
        /// 将时间戳转换为DateTime对象。
        /// </summary>
        /// <param name="timestamp">以毫秒为单位的时间戳。</param>
        /// <returns>转换后的DateTime对象，表示对应的时间戳所代表的日期和时间。</returns>
        public static DateTime Prase(long timestamp)
        {
            // 使用 DateTimeOffset.FromUnixTimeMilliseconds 将时间戳转换为带有时区偏移的日期时间，然后获取其 UTC 时间部分
            DateTime timestampDateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
            return timestampDateTime;
        }
    }
}
