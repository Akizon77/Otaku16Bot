using Newtonsoft.Json.Linq;
using Otaku16.Tools;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Telegram.Bot;

namespace Otaku16
{
    public class Bot
    {
        private static Logger log = new Logger("Bot");
        [AllowNull]
        private static TelegramBotClient bot = null;
        private static bool isLogin = false;
        public static TelegramBotClient GetBot()
        {
            if (isLogin) return bot;
            log.Info($"正在登录Bot");
            log.Debug($"Bot登录私钥: {Config.GetConfig().Telegram.Token}");
            if (Config.GetConfig().Proxy)
            {
                log.Info($"使用代理 {Config.GetConfig().Socks5}");
                WebProxy proxy = new(Config.GetConfig().Socks5);
                HttpClient httpClient = new(
                    new SocketsHttpHandler { Proxy = proxy, UseProxy = true }
                );
                bot = new TelegramBotClient(Config.GetConfig().Telegram.Token,httpClient);
            }
            else
            {
                bot = new TelegramBotClient(Config.GetConfig().Telegram.Token);
            }
            var me = bot.GetMeAsync().Result;
            log.Info($"已成功作为 @{me.Username} 登录");
            isLogin = true;
            return bot;
        }
    }
}
