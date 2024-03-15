using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Otaku16.Tools
{
    public class Network
    {
        private static Logger log = new Logger("HTTP");
        public static async Task<string> GetRedirectUrl(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    return response.RequestMessage.RequestUri.ToString();
                }
            }
            catch (Exception e)
            {
                log.Warn("无法获取302后地址",e);
                return url;
            }
            
        }

    }
}
