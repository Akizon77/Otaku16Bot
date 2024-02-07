using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otaku16.Tools
{
    public class TimeStamp
    {
        public static long GetNow()
        {
            DateTime currentTime = DateTime.UtcNow;
            long timestamp = ((DateTimeOffset)currentTime).ToUnixTimeMilliseconds();
            return timestamp;
        }
        public static DateTime Prase(long timestamp)
        {
            DateTime timestampDateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
            return timestampDateTime;
        }
    }
}
